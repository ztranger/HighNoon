using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HighNoon
{
    /// <summary>
    /// Screen-space HUD built entirely in code (uGUI + legacy font) so no scene
    /// wiring is needed. Shows the central BANG!, per-side reaction popups, and the
    /// result panel with Rematch / Menu buttons.
    /// </summary>
    public class DuelHUD : MonoBehaviour
    {
        Font _font;
        Image _flash;
        Text _bangText;
        readonly List<GameObject> _reactions = new List<GameObject>();
        readonly List<TimingBar> _timingBars = new List<TimingBar>();
        GameObject _resultPanel;
        Text _resultText;
        Text _pveStatus;
        Button _btn1, _btn2;
        Text _btn1Label, _btn2Label;
        Action _act1, _act2;

        public void Setup()
        {
            _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (_font == null) _font = Resources.GetBuiltinResource<Font>("Arial.ttf");

            var canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080); // the duel plays in landscape
            scaler.matchWidthOrHeight = 0.5f;
            gameObject.AddComponent<GraphicRaycaster>();

            // Full-screen flash overlay (behind text/panels, above the world sprites).
            var flashGo = new GameObject("Flash");
            flashGo.transform.SetParent(transform, false);
            var frt = flashGo.AddComponent<RectTransform>();
            frt.anchorMin = Vector2.zero; frt.anchorMax = Vector2.one;
            frt.offsetMin = Vector2.zero; frt.offsetMax = Vector2.zero;
            _flash = flashGo.AddComponent<Image>();
            _flash.color = new Color(1f, 1f, 1f, 0f);
            _flash.raycastTarget = false;

            _bangText = MakeText(transform, "BangText", 240, new Vector2(0.5f, 0.5f), Vector2.zero, 1000, 400);
            _bangText.text = "BANG!";
            _bangText.color = new Color(0.9f, 0.1f, 0.1f);
            _bangText.fontStyle = FontStyle.Bold;
            _bangText.gameObject.SetActive(false);

            _pveStatus = MakeText(transform, "PveStatus", 40, new Vector2(0.5f, 0.955f), Vector2.zero, 1040, 70);
            _pveStatus.color = new Color(1f, 0.94f, 0.78f);
            _pveStatus.fontStyle = FontStyle.Bold;
            _pveStatus.gameObject.SetActive(false);

            BuildResultPanel();
            _resultPanel.SetActive(false);
        }

        Text MakeText(Transform parent, string name, int size, Vector2 anchor, Vector2 pos, float w, float h)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = anchor;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(w, h);
            rt.anchoredPosition = pos;
            var txt = go.AddComponent<Text>();
            txt.font = _font;
            txt.fontSize = size;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color = Color.white;
            txt.horizontalOverflow = HorizontalWrapMode.Overflow;
            txt.verticalOverflow = VerticalWrapMode.Overflow;
            txt.raycastTarget = false; // decorative labels/popups must never block the result buttons under them
            return txt;
        }

        void BuildResultPanel()
        {
            _resultPanel = new GameObject("ResultPanel");
            _resultPanel.transform.SetParent(transform, false);
            var rt = _resultPanel.AddComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(780, 680);
            rt.anchoredPosition = Vector2.zero;
            var bg = _resultPanel.AddComponent<Image>();
            bg.color = new Color(0f, 0f, 0f, 0.65f);

            _resultText = MakeText(_resultPanel.transform, "ResultText", 68, new Vector2(0.5f, 0.8f), Vector2.zero, 720, 200);
            _resultText.text = "Winner";

            _btn1 = MakeButton(_resultPanel.transform, "Button1", "REMATCH", new Vector2(0.5f, 0.45f));
            _btn2 = MakeButton(_resultPanel.transform, "Button2", "MENU", new Vector2(0.5f, 0.2f));
            _btn1Label = _btn1.GetComponentInChildren<Text>();
            _btn2Label = _btn2.GetComponentInChildren<Text>();
            _btn1.onClick.AddListener(() => _act1?.Invoke());
            _btn2.onClick.AddListener(() => _act2?.Invoke());
        }

        Button MakeButton(Transform parent, string name, string label, Vector2 anchor)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = anchor;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(440, 120);
            rt.anchoredPosition = Vector2.zero;
            var img = go.AddComponent<Image>();
            img.color = new Color(0.78f, 0.58f, 0.22f);
            var btn = go.AddComponent<Button>();
            btn.targetGraphic = img;
            btn.onClick.AddListener(() => { Sfx.Click(); Haptics.Light(); });
            var t = MakeText(go.transform, "Label", 50, new Vector2(0.5f, 0.5f), Vector2.zero, 420, 110);
            t.text = label;
            t.color = Color.black;
            return btn;
        }

        public void HideAll()
        {
            _bangText.gameObject.SetActive(false);
            foreach (var r in _reactions) if (r != null) Destroy(r);
            _reactions.Clear();
            foreach (var b in _timingBars) if (b != null) Destroy(b.gameObject);
            _timingBars.Clear();
            _resultPanel.SetActive(false);
        }

        /// <summary>Build a sweet-spot timing bar on the HUD (Timing duel). Cleared by HideAll.</summary>
        public TimingBar AddTimingBar(bool topSide, Vector2 anchoredPos, float width, float height,
                                      float greenCenter, float greenHalf, string label)
        {
            var bar = new GameObject("TimingBar").AddComponent<TimingBar>();
            bar.Build(transform, _font, topSide, anchoredPos, width, height, greenCenter, greenHalf, label);
            _timingBars.Add(bar);
            return bar;
        }

        public void ShowBang()
        {
            _bangText.gameObject.SetActive(true);
            StartCoroutine(BangPop());
        }

        /// <summary>Full-screen colour flash that fades out (unscaled, survives hit-stop).</summary>
        public void Flash(Color color, float duration)
        {
            if (_flash == null) return;
            StartCoroutine(FlashRoutine(color, duration));
        }

        IEnumerator FlashRoutine(Color color, float duration)
        {
            float t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                float a = Mathf.Lerp(color.a, 0f, t / duration);
                _flash.color = new Color(color.r, color.g, color.b, a);
                yield return null;
            }
            _flash.color = new Color(color.r, color.g, color.b, 0f);
        }

        IEnumerator BangPop()
        {
            var rt = _bangText.rectTransform;
            float t = 0f, dur = 0.22f;
            while (t < dur)
            {
                t += Time.deltaTime;
                float s = Mathf.Lerp(0.4f, 1.15f, Mathf.Clamp01(t / dur));
                rt.localScale = new Vector3(s, s, 1f);
                yield return null;
            }
            rt.localScale = Vector3.one;
        }

        /// <summary>Floating reaction popup pinned above a duelist's world position (drawn on top).</summary>
        public void ShowReactionAt(Vector3 worldPos, string text, Color color)
        {
            var cam = Camera.main;
            if (cam == null) return;
            Vector3 screen = cam.WorldToScreenPoint(worldPos);
            RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)transform, screen, null, out var local);
            var txt = MakeText(transform, "Reaction", 84, new Vector2(0.5f, 0.5f), local, 700, 150);
            txt.text = text;
            txt.color = color;
            txt.fontStyle = FontStyle.Bold;
            var outline = txt.gameObject.AddComponent<Outline>(); // read clearly over the bright sun
            outline.effectColor = new Color(0f, 0f, 0f, 0.85f);
            outline.effectDistance = new Vector2(3f, -3f);
            txt.transform.SetAsLastSibling(); // above the result panel
            _reactions.Add(txt.gameObject);
        }

        /// <summary>A celebratory banner near the top (e.g. a new record). Cleared with the reactions.</summary>
        public void ShowBanner(string text, Color color)
        {
            var txt = MakeText(transform, "Banner", 78, new Vector2(0.5f, 0.74f), Vector2.zero, 1040, 160);
            txt.text = text;
            txt.color = color;
            txt.fontStyle = FontStyle.Bold;
            var outline = txt.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 0f, 0f, 0.85f);
            outline.effectDistance = new Vector2(3f, -3f);
            txt.transform.SetAsLastSibling();
            _reactions.Add(txt.gameObject);
            StartCoroutine(BannerPop(txt.rectTransform));
        }

        IEnumerator BannerPop(RectTransform rt)
        {
            float t = 0f, dur = 0.3f;
            while (rt != null && t < dur)
            {
                t += Time.unscaledDeltaTime;
                float s = Mathf.Lerp(0.5f, 1.1f, Mathf.Clamp01(t / dur));
                rt.localScale = new Vector3(s, s, 1f);
                yield return null;
            }
            if (rt != null) rt.localScale = Vector3.one;
        }

        public void ShowResult(string message, string label1, Action act1, string label2, Action act2)
        {
            _bangText.gameObject.SetActive(false); // clear the backdrop before the panel
            _resultText.text = message;
            _btn1Label.text = label1;
            _act1 = act1;
            _btn2Label.text = label2;
            _act2 = act2;
            _btn2.gameObject.SetActive(!string.IsNullOrEmpty(label2));
            _resultPanel.SetActive(true);
        }

        /// <summary>Top-of-screen PvE status line (stage / opponent / lives). Empty hides it.</summary>
        public void SetPveStatus(string text)
        {
            if (_pveStatus == null) return;
            _pveStatus.text = text;
            _pveStatus.gameObject.SetActive(!string.IsNullOrEmpty(text));
        }
    }
}
