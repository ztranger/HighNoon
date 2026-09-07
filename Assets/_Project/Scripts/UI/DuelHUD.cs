using System;
using System.Collections;
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
        Text _bangText;
        Text _bottomReaction, _topReaction;
        GameObject _resultPanel;
        Text _resultText;
        Action _onRematch, _onMenu;

        public void Setup()
        {
            _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (_font == null) _font = Resources.GetBuiltinResource<Font>("Arial.ttf");

            var canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;
            gameObject.AddComponent<GraphicRaycaster>();

            _bangText = MakeText(transform, "BangText", 240, new Vector2(0.5f, 0.5f), Vector2.zero, 1000, 400);
            _bangText.text = "BANG!";
            _bangText.color = new Color(0.9f, 0.1f, 0.1f);
            _bangText.fontStyle = FontStyle.Bold;
            _bangText.gameObject.SetActive(false);

            _topReaction = MakeText(transform, "TopReaction", 72, new Vector2(0.5f, 0.72f), Vector2.zero, 900, 120);
            _topReaction.gameObject.SetActive(false);
            _bottomReaction = MakeText(transform, "BottomReaction", 72, new Vector2(0.5f, 0.28f), Vector2.zero, 900, 120);
            _bottomReaction.gameObject.SetActive(false);

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

            var rematch = MakeButton(_resultPanel.transform, "RematchButton", "REMATCH", new Vector2(0.5f, 0.45f));
            var menu = MakeButton(_resultPanel.transform, "MenuButton", "MENU", new Vector2(0.5f, 0.2f));
            rematch.onClick.AddListener(() => _onRematch?.Invoke());
            menu.onClick.AddListener(() => _onMenu?.Invoke());
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
            var t = MakeText(go.transform, "Label", 50, new Vector2(0.5f, 0.5f), Vector2.zero, 420, 110);
            t.text = label;
            t.color = Color.black;
            return btn;
        }

        public void HideAll()
        {
            _bangText.gameObject.SetActive(false);
            _topReaction.gameObject.SetActive(false);
            _bottomReaction.gameObject.SetActive(false);
            _resultPanel.SetActive(false);
        }

        public void ShowBang()
        {
            _bangText.gameObject.SetActive(true);
            StopAllCoroutines();
            StartCoroutine(BangPop());
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

        public void ShowReaction(DuelSide side, double seconds, bool winner, bool falseStart, bool fired)
        {
            var txt = side == DuelSide.Bottom ? _bottomReaction : _topReaction;
            txt.gameObject.SetActive(true);
            if (falseStart)
            {
                txt.text = "FALSE START!";
                txt.color = new Color(1f, 0.6f, 0.1f);
            }
            else if (!fired)
            {
                txt.text = "— no shot —";
                txt.color = new Color(1f, 0.5f, 0.5f);
            }
            else
            {
                txt.text = $"{seconds * 1000.0:0} ms";
                txt.color = winner ? new Color(0.3f, 1f, 0.3f) : new Color(1f, 0.5f, 0.5f);
            }
        }

        public void ShowResult(string message, Action onRematch, Action onMenu)
        {
            _bangText.gameObject.SetActive(false); // clear the backdrop before the panel
            _onRematch = onRematch;
            _onMenu = onMenu;
            _resultText.text = message;
            _resultPanel.SetActive(true);
        }
    }
}
