using System;
using UnityEngine;
using UnityEngine.UI;

namespace HighNoon
{
    /// <summary>
    /// Pre-duel banter overlay: a bottom dialog panel (speaker portrait + name + line)
    /// advanced by tapping anywhere. Built in code on its own top-most canvas.
    /// </summary>
    public class DialogBox : MonoBehaviour
    {
        Font _font;
        Image _portrait;
        Text _name;
        Text _body;

        DialogLine[] _lines;
        int _index;
        Action _onDone;

        string _youName, _oppName;
        Color _youColor, _oppColor;
        Sprite _youPortrait, _oppPortrait;

        public void Setup()
        {
            _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (_font == null) _font = Resources.GetBuiltinResource<Font>("Arial.ttf");

            var canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100; // above the HUD
            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;
            gameObject.AddComponent<GraphicRaycaster>();

            // Panel (bottom).
            var panel = NewRect("Panel", transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f));
            panel.pivot = new Vector2(0.5f, 0f);
            panel.sizeDelta = new Vector2(1000, 520);
            panel.anchoredPosition = new Vector2(0f, 110f);
            var bg = panel.gameObject.AddComponent<Image>();
            bg.color = new Color(0.06f, 0.05f, 0.04f, 0.9f);

            // Portrait frame + image.
            var frame = NewRect("PortraitFrame", panel, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f));
            frame.pivot = new Vector2(0f, 0.5f);
            frame.sizeDelta = new Vector2(240, 300);
            frame.anchoredPosition = new Vector2(30f, 0f);
            frame.gameObject.AddComponent<Image>().color = new Color(0.14f, 0.12f, 0.10f, 1f);

            var portRt = NewRect("Portrait", frame, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            portRt.pivot = new Vector2(0.5f, 0.5f);
            portRt.sizeDelta = new Vector2(180, 240);
            _portrait = portRt.gameObject.AddComponent<Image>();
            _portrait.preserveAspect = true;

            // Speaker name.
            _name = MakeText(panel, "Name", 52, new Vector2(0f, 1f), new Vector2(300, -20), 640, 80, TextAnchor.UpperLeft);
            _name.fontStyle = FontStyle.Bold;

            // Body line.
            _body = MakeText(panel, "Body", 46, new Vector2(0f, 1f), new Vector2(300, -110), 660, 320, TextAnchor.UpperLeft);
            _body.horizontalOverflow = HorizontalWrapMode.Wrap;
            _body.color = new Color(0.95f, 0.92f, 0.85f);

            // Hint.
            var hint = MakeText(panel, "Hint", 34, new Vector2(1f, 0f), new Vector2(-30, 24), 400, 50, TextAnchor.LowerRight);
            hint.text = "tap to continue  ▶";
            hint.color = new Color(0.7f, 0.66f, 0.58f);

            // Full-screen invisible advance button (on top).
            var adv = NewRect("Advance", transform, Vector2.zero, Vector2.one);
            adv.offsetMin = Vector2.zero; adv.offsetMax = Vector2.zero;
            var advImg = adv.gameObject.AddComponent<Image>();
            advImg.color = new Color(0, 0, 0, 0);
            var advBtn = adv.gameObject.AddComponent<Button>();
            advBtn.targetGraphic = advImg;
            advBtn.transition = Selectable.Transition.None;
            advBtn.onClick.AddListener(Next);
        }

        public void Play(DialogLine[] lines, string youName, Color youColor, Sprite youPortrait,
                         string oppName, Color oppColor, Sprite oppPortrait, Action onDone)
        {
            _lines = lines;
            _onDone = onDone;
            _youName = youName; _oppName = oppName;
            _youColor = youColor; _oppColor = oppColor;
            _youPortrait = youPortrait; _oppPortrait = oppPortrait;
            _index = 0;

            if (_lines == null || _lines.Length == 0) { Finish(); return; }
            ShowLine(0);
        }

        void ShowLine(int i)
        {
            var line = _lines[i];
            bool you = line.Speaker == Speaker.You;
            _name.text = you ? _youName : _oppName;
            _name.color = you ? _youColor : _oppColor;
            _portrait.sprite = you ? _youPortrait : _oppPortrait;
            _body.text = line.Text;
        }

        void Next()
        {
            _index++;
            if (_index >= _lines.Length) { Finish(); return; }
            ShowLine(_index);
        }

        void Finish()
        {
            var cb = _onDone;
            _onDone = null;
            Destroy(gameObject);
            cb?.Invoke();
        }

        // ---- helpers ----

        RectTransform NewRect(string name, Transform parent, Vector2 aMin, Vector2 aMax)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = aMin; rt.anchorMax = aMax;
            return rt;
        }

        Text MakeText(Transform parent, string name, int size, Vector2 anchor, Vector2 pos, float w, float h, TextAnchor align)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = anchor;
            rt.pivot = anchor;
            rt.sizeDelta = new Vector2(w, h);
            rt.anchoredPosition = pos;
            var txt = go.AddComponent<Text>();
            txt.font = _font;
            txt.fontSize = size;
            txt.alignment = align;
            txt.color = Color.white;
            txt.horizontalOverflow = HorizontalWrapMode.Overflow;
            txt.verticalOverflow = VerticalWrapMode.Overflow;
            txt.raycastTarget = false;
            return txt;
        }
    }
}
