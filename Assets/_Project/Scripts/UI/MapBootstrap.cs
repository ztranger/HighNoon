using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem.UI;

namespace HighNoon
{
    /// <summary>
    /// Visual mission map for the current PvE chapter. Nodes run bottom → top (players
    /// climb toward the boss). Cleared nodes are green, the current node glows and is
    /// tappable (→ starts its duel), later nodes are locked. Built in code (overlay UI).
    /// </summary>
    public class MapBootstrap : MonoBehaviour
    {
        static readonly Color Cleared = new Color(0.35f, 0.68f, 0.35f);
        static readonly Color Current = new Color(0.92f, 0.74f, 0.28f);
        static readonly Color Locked  = new Color(0.40f, 0.38f, 0.36f);
        static readonly Color LineCol = new Color(0.20f, 0.15f, 0.10f, 0.85f);
        static readonly Color Gold    = new Color(0.95f, 0.82f, 0.38f);

        static Sprite _circle;

        Font _font;
        RectTransform _pulseNode;

        void Start()
        {
            AppInit.Apply();
            MatchSettings.Mode = GameMode.PvE; // the map is the PvE hub; duels launched from it are PvE
            if (!Campaign.Active) Campaign.StartRun();

            _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (_font == null) _font = Resources.GetBuiltinResource<Font>("Arial.ttf");

            SetupCamera();
            SetupEventSystem();
            BuildUI();
        }

        void BuildUI()
        {
            var chapter = Campaign.CurrentChapter;

            var canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 1f; // match height so the vertical map always fits
            gameObject.AddComponent<GraphicRaycaster>();

            // Background (chapter theme, slightly darkened).
            var bg = NewRect("BG", transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var bgImg = bg.gameObject.AddComponent<Image>();
            var bgc = chapter.Theme * 0.55f; bgc.a = 1f;
            bgImg.color = bgc;
            bgImg.raycastTarget = false;

            // Header (all in reference-px, center-anchored, so the whole layout scales as one).
            Label("ChapterKicker", $"CHAPTER {Campaign.Chapter + 1} / {Campaign.Chapters.Length}", 42,
                new Vector2(0.5f, 0.5f), 900, 70, new Color(0.9f, 0.85f, 0.7f), FontStyle.Normal)
                .rectTransform.anchoredPosition = new Vector2(0f, 850f);
            Label("ChapterTitle", chapter.Title, 96,
                new Vector2(0.5f, 0.5f), 1000, 150, Gold, FontStyle.Bold)
                .rectTransform.anchoredPosition = new Vector2(0f, 750f);
            Label("Lives", $"LIVES  {Campaign.Lives}", 46,
                new Vector2(0.5f, 0.5f), 900, 70, new Color(1f, 0.6f, 0.55f), FontStyle.Bold)
                .rectTransform.anchoredPosition = new Vector2(0f, 655f);

            // Node positions (bottom → top).
            int n = chapter.Stages.Length;
            var pos = new Vector2[n];
            for (int i = 0; i < n; i++)
            {
                float t = n == 1 ? 0.5f : i / (float)(n - 1);
                float y = Mathf.Lerp(-520f, 500f, t);
                float x = (i % 2 == 0) ? -170f : 170f;
                pos[i] = new Vector2(x, y);
            }

            // Path lines (behind nodes).
            for (int i = 0; i < n - 1; i++) MakeLine(pos[i], pos[i + 1]);

            // Nodes.
            for (int i = 0; i < n; i++)
            {
                bool cleared = i < Campaign.Stage;
                bool current = i == Campaign.Stage;
                bool boss = i == n - 1;
                Color col = cleared ? Cleared : current ? Current : Locked;
                float size = (current ? 150f : 118f) + (boss ? 16f : 0f);

                var node = NewRect($"Node{i}", transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
                node.sizeDelta = new Vector2(size, size);
                node.anchoredPosition = pos[i];
                var img = node.gameObject.AddComponent<Image>();
                img.sprite = Circle();
                img.color = col;

                // number / boss marker inside
                var num = Label($"Node{i}Num", boss ? "!" : (i + 1).ToString(), boss ? 70 : 54,
                    new Vector2(0.5f, 0.5f), size, size, new Color(0.15f, 0.10f, 0.06f), FontStyle.Bold);
                num.transform.SetParent(node, false);
                num.rectTransform.anchoredPosition = Vector2.zero;

                // opponent label, kept inboard so it never runs off-screen
                float lx = pos[i].x < 0 ? pos[i].x + 210f : pos[i].x - 210f;
                var name = Label($"Node{i}Name", chapter.Stages[i].Title, 40,
                    new Vector2(0.5f, 0.5f), 380, 80, cleared ? new Color(0.8f, 0.9f, 0.8f) : current ? Gold : new Color(0.7f, 0.68f, 0.65f), FontStyle.Bold);
                name.rectTransform.anchoredPosition = new Vector2(lx, pos[i].y);

                if (current)
                {
                    var btn = node.gameObject.AddComponent<Button>();
                    btn.targetGraphic = img;
                    btn.onClick.AddListener(StartStage);
                    _pulseNode = node;

                    // player token stands on the current node
                    var tokRt = NewRect("PlayerToken", transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
                    tokRt.sizeDelta = new Vector2(96, 128);
                    tokRt.anchoredPosition = pos[i] + new Vector2(0f, size * 0.5f + 50f);
                    var tok = tokRt.gameObject.AddComponent<Image>();
                    tok.sprite = CowboyArt.Build(CowboyLook.Player()).Idle[0];
                    tok.raycastTarget = false;
                }
            }

            // Hint + menu.
            Label("Hint", "Tap the glowing spot to draw", 38,
                new Vector2(0.5f, 0.5f), 1000, 70, new Color(0.85f, 0.82f, 0.72f), FontStyle.Normal)
                .rectTransform.anchoredPosition = new Vector2(0f, -700f);
            var menu = MakeButton("MenuButton", "MENU", new Vector2(0.5f, 0.5f), 360, 110, 44, Locked);
            ((RectTransform)menu.transform).anchoredPosition = new Vector2(0f, -850f);
            menu.onClick.AddListener(() => SceneManager.LoadScene("MainMenu"));

            if (_pulseNode != null) StartCoroutine(Pulse());
        }

        void StartStage()
        {
            Campaign.ShowIntro = true; // play this stage's intro banter before the duel
            SceneManager.LoadScene("Duel");
        }

        IEnumerator Pulse()
        {
            while (_pulseNode != null)
            {
                float s = 1f + 0.08f * Mathf.Sin(Time.unscaledTime * 4f);
                _pulseNode.localScale = new Vector3(s, s, 1f);
                yield return null;
            }
        }

        // ---- helpers ----

        RectTransform NewRect(string name, Transform parent, Vector2 aMin, Vector2 aMax, Vector2 offMin, Vector2 offMax)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = aMin; rt.anchorMax = aMax;
            rt.offsetMin = offMin; rt.offsetMax = offMax;
            return rt;
        }

        Text Label(string name, string content, int size, Vector2 anchor, float w, float h, Color color, FontStyle style)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = anchor;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(w, h);
            rt.anchoredPosition = Vector2.zero;
            var txt = go.AddComponent<Text>();
            txt.text = content;
            txt.font = _font; txt.fontSize = size; txt.fontStyle = style;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color = color;
            txt.horizontalOverflow = HorizontalWrapMode.Overflow;
            txt.verticalOverflow = VerticalWrapMode.Overflow;
            txt.raycastTarget = false;
            return txt;
        }

        void MakeLine(Vector2 a, Vector2 b)
        {
            var rt = NewRect("Line", transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            Vector2 mid = (a + b) * 0.5f;
            float len = Vector2.Distance(a, b);
            float ang = Mathf.Atan2(b.y - a.y, b.x - a.x) * Mathf.Rad2Deg;
            rt.sizeDelta = new Vector2(len, 16f);
            rt.anchoredPosition = mid;
            rt.localEulerAngles = new Vector3(0f, 0f, ang);
            var img = rt.gameObject.AddComponent<Image>();
            img.color = LineCol;
            img.raycastTarget = false;
        }

        Button MakeButton(string name, string label, Vector2 anchor, float w, float h, int fontSize, Color color)
        {
            var rt = NewRect(name, transform, anchor, anchor, Vector2.zero, Vector2.zero);
            rt.sizeDelta = new Vector2(w, h);
            rt.anchoredPosition = Vector2.zero;
            var img = rt.gameObject.AddComponent<Image>();
            img.color = color;
            var btn = rt.gameObject.AddComponent<Button>();
            btn.targetGraphic = img;
            var t = Label(name + "Label", label, fontSize, new Vector2(0.5f, 0.5f), w, h, Color.white, FontStyle.Bold);
            t.transform.SetParent(rt, false);
            t.rectTransform.anchoredPosition = Vector2.zero;
            return btn;
        }

        static Sprite Circle()
        {
            if (_circle != null) return _circle;
            int s = 64;
            var tex = new Texture2D(s, s, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear };
            var px = new Color32[s * s];
            float c = (s - 1) / 2f, r = s / 2f - 1.5f;
            for (int y = 0; y < s; y++)
                for (int x = 0; x < s; x++)
                {
                    float dx = x - c, dy = y - c;
                    bool inside = dx * dx + dy * dy <= r * r;
                    px[y * s + x] = new Color32(255, 255, 255, (byte)(inside ? 255 : 0));
                }
            tex.SetPixels32(px);
            tex.Apply();
            _circle = Sprite.Create(tex, new Rect(0, 0, s, s), new Vector2(0.5f, 0.5f), 64);
            return _circle;
        }

        void SetupCamera()
        {
            var cam = Camera.main;
            if (cam == null)
            {
                var camGo = new GameObject("Main Camera") { tag = "MainCamera" };
                cam = camGo.AddComponent<Camera>();
            }
            cam.orthographic = true;
            cam.transform.position = new Vector3(0f, 0f, -10f);
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Campaign.CurrentChapter.Theme * 0.5f;
        }

        void SetupEventSystem()
        {
            if (FindFirstObjectByType<EventSystem>() != null) return;
            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<InputSystemUIInputModule>().AssignDefaultActions();
        }
    }
}
