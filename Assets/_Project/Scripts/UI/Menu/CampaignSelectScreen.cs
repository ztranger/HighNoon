using UnityEngine;
using UnityEngine.UI;

namespace HighNoon
{
    /// <summary>
    /// Landscape list of PvE roads. The first is open; each later road stays clickable
    /// while locked and answers with a toast naming the road that must be cleared first.
    /// </summary>
    public sealed class CampaignSelectScreen
    {
        public RectTransform Root { get; private set; }

        MenuToast _toast;

        public void Build(RectTransform root, UiBuild ui, System.Action back)
        {
            Root = root;
            ui.Back(root, back);
            ui.Text(root, "Head", "CAMPAIGNS", 64, new Vector2(0.5f, 0.90f), Vector2.zero, 900, 80, UiBuild.Gold, FontStyle.Bold);
            ui.Text(root, "Sub",
                Campaign.FreePick ? "Every road is open. On the map, tap any spot." : "Clear a road to open the next.",
                28, new Vector2(0.5f, 0.82f), Vector2.zero, 1200, 40,
                new Color(0.80f, 0.72f, 0.55f), FontStyle.Italic);

            int n = Campaign.All.Length;
            float rowH = 132f;
            float stride = 148f;
            var scrollRt = ui.NewRect("RoadScroll", root, Vector2.zero, Vector2.one);
            scrollRt.offsetMin = new Vector2(40f, 24f);
            scrollRt.offsetMax = new Vector2(-40f, -210f);
            var scrollImg = scrollRt.gameObject.AddComponent<Image>();
            scrollImg.color = new Color(0f, 0f, 0f, 0f);
            scrollImg.raycastTarget = true;
            var scroll = scrollRt.gameObject.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 40f;

            var view = ui.NewRect("Viewport", scrollRt, Vector2.zero, Vector2.one);
            view.offsetMin = Vector2.zero;
            view.offsetMax = Vector2.zero;
            view.gameObject.AddComponent<RectMask2D>();

            var content = ui.NewRect("Content", view, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));
            content.pivot = new Vector2(0.5f, 1f);
            content.sizeDelta = new Vector2(1600f, n * stride);
            content.anchoredPosition = Vector2.zero;
            scroll.viewport = view;
            scroll.content = content;

            for (int i = 0; i < n; i++)
            {
                int index = i;
                var def = Campaign.All[i];
                bool open = Campaign.IsUnlocked(i);
                bool saved = Campaign.IsSavedRun(i);
                bool cleared = Campaign.IsCleared(def.Id);

                string status = !open ? "LOCKED"
                    : saved ? "CONTINUE"
                    : cleared ? "PLAY AGAIN"
                    : "PLAY";
                Color fill = !open ? UiBuild.Disabled
                    : saved ? new Color(0.55f, 0.70f, 0.40f)
                    : new Color(0.70f, 0.52f, 0.24f);

                var btn = ui.Button(content, "Road" + i, def.Title, new Vector2(0.5f, 1f),
                    new Vector2(0f, -(rowH * 0.5f + i * stride)),
                    1480, rowH, 36, fill, out var image);
                var title = btn.GetComponentInChildren<Text>();
                title.rectTransform.anchoredPosition = new Vector2(0f, 36f);
                title.rectTransform.sizeDelta = new Vector2(1400f, 56f);
                if (!open) title.color = new Color(0.78f, 0.74f, 0.66f);

                var blurb = ui.Text(btn.transform, "Blurb", def.Blurb, 26, new Vector2(0.5f, 0.5f), new Vector2(0f, -8f),
                    1400, 40, open ? UiBuild.Ink : new Color(0.62f, 0.58f, 0.52f), FontStyle.Italic);
                blurb.horizontalOverflow = HorizontalWrapMode.Wrap;

                var state = ui.Text(btn.transform, "Status", status, 24, new Vector2(0.5f, 0.5f), new Vector2(0f, -48f),
                    1400, 36, open ? new Color(0.28f, 0.18f, 0.08f) : new Color(0.90f, 0.78f, 0.45f), FontStyle.Bold);
                state.horizontalOverflow = HorizontalWrapMode.Overflow;

                image.color = fill;
                btn.onClick.AddListener(() => OnRow(index));
            }

            _toast = MenuToast.Build(root, ui);
        }

        void OnRow(int index)
        {
            if (!Campaign.IsUnlocked(index))
            {
                _toast.Show(Campaign.UnlockHint(index));
                return;
            }

            MatchSettings.Mode = GameMode.PvE;
            MatchSettings.Players = PvPPlayers.OnePlayer;
            Campaign.Select(index);
            if (Campaign.IsSavedRun(index)) DuelFlow.Map();
            else
            {
                Campaign.StartRun();
                DuelFlow.Story(StoryKind.ChapterIntro);
            }
        }
    }

    /// <summary>Short-lived line at the bottom of a menu screen.</summary>
    public sealed class MenuToast : MonoBehaviour
    {
        Text _text;
        Image _plate;
        float _until;

        public static MenuToast Build(RectTransform parent, UiBuild ui)
        {
            var go = new GameObject("Toast");
            var rt = go.AddComponent<RectTransform>();
            rt.SetParent(parent, false);
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.sizeDelta = new Vector2(1100f, 88f);
            rt.anchoredPosition = new Vector2(0f, 36f);

            var plate = go.AddComponent<Image>();
            plate.color = new Color(0.10f, 0.08f, 0.06f, 0.94f);
            plate.raycastTarget = false;

            var toast = go.AddComponent<MenuToast>();
            toast._plate = plate;
            toast._text = ui.Text(rt, "Line", "", 30, new Vector2(0.5f, 0.5f), Vector2.zero, 1040, 72,
                new Color(0.95f, 0.86f, 0.55f), FontStyle.Bold);
            toast._text.horizontalOverflow = HorizontalWrapMode.Wrap;
            go.SetActive(false);
            return toast;
        }

        public void Show(string message)
        {
            _text.text = message;
            gameObject.SetActive(true);
            transform.SetAsLastSibling();
            _until = Time.unscaledTime + 2.6f;
        }

        void Update()
        {
            if (!gameObject.activeSelf) return;
            float left = _until - Time.unscaledTime;
            if (left <= 0f)
            {
                gameObject.SetActive(false);
                return;
            }
            var c = _plate.color;
            c.a = left < 0.4f ? left / 0.4f * 0.94f : 0.94f;
            _plate.color = c;
            var tc = _text.color;
            tc.a = left < 0.4f ? left / 0.4f : 1f;
            _text.color = tc;
        }
    }
}
