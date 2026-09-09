using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

namespace HighNoon
{
    /// <summary>
    /// Shared full-screen story beat: chapter intro, campaign victory, or defeat
    /// (chosen by <see cref="Story.Kind"/>). Built in code as overlay UI.
    /// </summary>
    public class StoryBootstrap : MonoBehaviour
    {
        static readonly Color Gold = new Color(0.95f, 0.82f, 0.38f);
        static readonly Color Red  = new Color(0.88f, 0.28f, 0.24f);
        static readonly Color Btn  = new Color(0.78f, 0.58f, 0.22f);

        Font _font;
        AudioSource _music; // victory/defeat stings play here so they stop when this scene unloads

        void Start()
        {
            AppInit.Apply();

            _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (_font == null) _font = Resources.GetBuiltinResource<Font>("Arial.ttf");

            SetupCamera();
            SetupEventSystem();
            BuildCanvas();

            _music = gameObject.AddComponent<AudioSource>();
            _music.playOnAwake = false;

            switch (Story.Kind)
            {
                case StoryKind.ChapterIntro: BuildChapterIntro(); break;
                case StoryKind.Victory:      BuildVictory();      break;
                case StoryKind.Defeat:       BuildDefeat();       break;
            }
        }

        void BuildCanvas()
        {
            UiCanvas.Overlay(gameObject);
        }

        void Background(Color c)
        {
            var bg = NewRect("BG", Vector2.zero, Vector2.one);
            bg.offsetMin = Vector2.zero; bg.offsetMax = Vector2.zero;
            var img = bg.gameObject.AddComponent<Image>();
            c.a = 1f; img.color = c;
            img.raycastTarget = false;
        }

        void BuildChapterIntro()
        {
            var ch = Campaign.CurrentChapter;
            Background(ch.Theme * 0.5f);

            Text("Kicker", $"CHAPTER {Campaign.Chapter + 1} / {Campaign.Chapters.Length}", 36, 380, 900, 56,
                new Color(0.9f, 0.85f, 0.72f), FontStyle.Normal);
            Text("Title", ch.Title, 88, 250, 1200, 140, Gold, FontStyle.Bold);
            Text("Tagline", ch.Tagline, 36, 120, 1100, 80, new Color(0.86f, 0.82f, 0.72f), FontStyle.Italic);

            Cowboy(new Vector2(-420f, -80f), new Vector2(280f, 360f));

            Button("Begin", "BEGIN", new Vector2(360f, -120f), 480, 130, 56, Gold, DuelFlow.Map);
        }

        void BuildVictory()
        {
            Background(new Color(0.22f, 0.17f, 0.08f));
            _music.PlayOneShot(AudioBank.GetRandom("victory", ProcAudio.Victory), 0.9f * GameSettings.SfxVolume);
            Haptics.Success();

            Text("Title", "VICTORY", 110, 360, 1040, 160, Gold, FontStyle.Bold);
            Cowboy(new Vector2(0f, 40f), new Vector2(260f, 340f));
            Text("Flavor", "You cleaned up the West.  No one draws faster.", 36, -200, 1200, 80,
                new Color(0.92f, 0.88f, 0.78f), FontStyle.Normal);

            Button("Again", "PLAY AGAIN", new Vector2(-320f, -380f), 520, 120, 48, Btn, Restart);
            Button("Menu", "MENU", new Vector2(320f, -380f), 400, 120, 44, new Color(0.45f, 0.4f, 0.3f), DuelFlow.Menu);
        }

        void BuildDefeat()
        {
            Background(new Color(0.12f, 0.11f, 0.11f));
            _music.PlayOneShot(AudioBank.GetRandom("defeat", ProcAudio.Defeat), 0.9f * GameSettings.SfxVolume);
            Haptics.Heavy();

            Text("Title", "DEFEAT", 110, 360, 1040, 160, Red, FontStyle.Bold);
            Prop(PropArt.Tombstone(), new Vector2(0f, 40f), new Vector2(240f, 300f));
            Text("Flavor", "The frontier claims another.  Boot Hill has a fresh plot.", 36, -200, 1200, 80,
                new Color(0.82f, 0.8f, 0.78f), FontStyle.Normal);

            Button("Again", "TRY AGAIN", new Vector2(-320f, -380f), 520, 120, 48, Btn, Restart);
            Button("Menu", "MENU", new Vector2(320f, -380f), 400, 120, 44, new Color(0.4f, 0.38f, 0.36f), DuelFlow.Menu);
        }

        void Restart()
        {
            Campaign.StartRun();
            DuelFlow.Story(StoryKind.ChapterIntro);
        }

        // ---- builders ----

        RectTransform NewRect(string name, Vector2 aMin, Vector2 aMax)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = aMin; rt.anchorMax = aMax;
            return rt;
        }

        Text Text(string name, string content, int size, float y, float w, float h, Color color, FontStyle style)
        {
            var rt = NewRect(name, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(w, h);
            rt.anchoredPosition = new Vector2(0f, y);
            var txt = rt.gameObject.AddComponent<Text>();
            txt.text = content;
            txt.font = _font; txt.fontSize = size; txt.fontStyle = style;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color = color;
            txt.horizontalOverflow = HorizontalWrapMode.Overflow;
            txt.verticalOverflow = VerticalWrapMode.Overflow;
            txt.raycastTarget = false;
            return txt;
        }

        void Cowboy(Vector2 pos, Vector2 size)
            => Prop(CowboyArt.Build(CowboyLook.Player()).Idle[0], pos, size);

        void Prop(Sprite sprite, Vector2 pos, Vector2 size)
        {
            var rt = NewRect("Art", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = size;
            rt.anchoredPosition = pos;
            var img = rt.gameObject.AddComponent<Image>();
            img.sprite = sprite;
            img.preserveAspect = true;
            img.raycastTarget = false;
        }

        void Button(string name, string label, Vector2 pos, float w, float h, int fontSize, Color color, System.Action onClick)
        {
            var rt = NewRect(name, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(w, h);
            rt.anchoredPosition = pos;
            var img = rt.gameObject.AddComponent<Image>();
            img.color = color;
            var btn = rt.gameObject.AddComponent<Button>();
            btn.targetGraphic = img;
            btn.onClick.AddListener(() => { Sfx.Click(); Haptics.Light(); });
            btn.onClick.AddListener(() => onClick());
            var t = Text(name + "Label", label, fontSize, 0f, w, h, new Color(0.15f, 0.10f, 0.06f), FontStyle.Bold);
            t.transform.SetParent(rt, false);
            t.rectTransform.anchoredPosition = Vector2.zero;
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
            cam.backgroundColor = new Color(0.1f, 0.09f, 0.08f);
            AppInit.EnsureAudioListener(cam);
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
