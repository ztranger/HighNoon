using System;
using UnityEngine;
using UnityEngine.UI;

namespace HighNoon
{
    /// <summary>Code-built uGUI primitives shared by menu screens (legacy Text, no prefabs).</summary>
    public sealed class UiBuild
    {
        public static readonly Color Selected = new Color(0.88f, 0.66f, 0.22f);
        public static readonly Color Normal   = new Color(0.48f, 0.38f, 0.24f);
        public static readonly Color Disabled = new Color(0.34f, 0.31f, 0.29f);
        public static readonly Color Gold     = new Color(0.92f, 0.78f, 0.35f);
        public static readonly Color Label    = new Color(0.70f, 0.62f, 0.50f);
        public static readonly Color Ink      = new Color(0.15f, 0.10f, 0.06f);

        readonly Font _font;

        public UiBuild(Font font) { _font = font; }

        public RectTransform Stretch(string name, Transform parent)
        {
            var rt = NewRect(name, parent, Vector2.zero, Vector2.one);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            return rt;
        }

        public RectTransform NewRect(string name, Transform parent, Vector2 aMin, Vector2 aMax)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = aMin;
            rt.anchorMax = aMax;
            return rt;
        }

        public Text Text(Transform parent, string name, string content, int size, Vector2 anchor, Vector2 pos, float w, float h, Color color, FontStyle style)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = anchor;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(w, h);
            rt.anchoredPosition = pos;
            var txt = go.AddComponent<Text>();
            txt.text = content;
            txt.font = _font;
            txt.fontSize = size;
            txt.fontStyle = style;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color = color;
            txt.horizontalOverflow = HorizontalWrapMode.Overflow;
            txt.verticalOverflow = VerticalWrapMode.Overflow;
            txt.raycastTarget = false;
            return txt;
        }

        public Button Button(Transform parent, string name, string label, Vector2 anchor, Vector2 pos, float w, float h, int fontSize, Color color, out Image image, bool interactable = true)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = anchor;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(w, h);
            rt.anchoredPosition = pos;
            image = go.AddComponent<Image>();
            image.color = color;
            var btn = go.AddComponent<Button>();
            btn.targetGraphic = image;
            btn.interactable = interactable;
            btn.onClick.AddListener(() => { Sfx.Click(); Haptics.Light(); });
            var t = Text(go.transform, "Label", label, fontSize, new Vector2(0.5f, 0.5f), Vector2.zero, w - 20, h - 10, Ink, FontStyle.Bold);
            t.color = Ink;
            return btn;
        }

        public Button Back(RectTransform parent, Action onBack)
        {
            var btn = Button(parent, "Back", "‹  BACK", new Vector2(0f, 1f), new Vector2(140f, -56f), 220, 72, 32, Normal, out _, true);
            btn.onClick.AddListener(() => onBack());
            return btn;
        }

        public static void SetAlpha(Image img, float a)
        {
            if (img == null) return;
            var c = img.color;
            c.a = a;
            img.color = c;
        }
    }
}
