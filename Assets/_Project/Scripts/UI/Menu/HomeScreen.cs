using System;
using UnityEngine;
using UnityEngine.UI;

namespace HighNoon
{
    /// <summary>Landscape hub: idle cowboy, PvP / PvE, corners to the other menu screens.</summary>
    public sealed class HomeScreen
    {
        public RectTransform Root { get; private set; }

        public void Build(RectTransform root, UiBuild ui, Action<MenuScreenId> go)
        {
            Root = root;

            Chip(ui, root, "NavStats", "STATS", new Vector2(0f, 1f), new Vector2(140f, -56f), () => go(MenuScreenId.Stats));
            Chip(ui, root, "NavGuns", "GUNS", new Vector2(0f, 1f), new Vector2(380f, -56f), () => go(MenuScreenId.Guns));
            Chip(ui, root, "NavSetup", "SETUP", new Vector2(1f, 1f), new Vector2(-380f, -56f), () => go(MenuScreenId.Setup));
            Chip(ui, root, "NavSettings", "SETTINGS", new Vector2(1f, 1f), new Vector2(-120f, -56f), () => go(MenuScreenId.Settings));

            ui.Text(root, "Title", "HIGH NOON", 88, new Vector2(0.5f, 0.76f), Vector2.zero, 800, 110, UiBuild.Gold, FontStyle.Bold);
            ui.Text(root, "Sub", "— Wild West Duel —", 34, new Vector2(0.5f, 0.66f), Vector2.zero, 700, 50, new Color(0.8f, 0.7f, 0.5f), FontStyle.Normal);

            var heroGo = new GameObject("Hero");
            var hrt = heroGo.AddComponent<RectTransform>();
            hrt.SetParent(root, false);
            hrt.anchorMin = hrt.anchorMax = new Vector2(0.22f, 0.42f);
            hrt.pivot = new Vector2(0.5f, 0.5f);
            hrt.sizeDelta = new Vector2(340f, 460f);
            var heroImg = heroGo.AddComponent<Image>();
            heroImg.raycastTarget = false;
            heroImg.preserveAspect = true;
            var frames = CowboyArt.Build(CowboyLook.Player());
            heroImg.sprite = frames.Idle[0];
            heroGo.AddComponent<UiSpriteAnim>().Play(heroImg, frames.Idle, 2.5f);

            var pvp = ui.Button(root, "HomePvP", "PvP  DUEL", new Vector2(0.70f, 0.46f), Vector2.zero, 560, 120, 52, UiBuild.Gold, out _);
            pvp.onClick.AddListener(() => { MatchSettings.Mode = GameMode.PvP; DuelFlow.Duel(); });

            var pve = ui.Button(root, "HomePvE", "PvE  CAMPAIGN", new Vector2(0.70f, 0.28f), Vector2.zero, 560, 120, 48, new Color(0.70f, 0.52f, 0.24f), out _);
            pve.onClick.AddListener(() =>
            {
                MatchSettings.Mode = GameMode.PvE;
                if (Campaign.HasSavedRun) DuelFlow.Map();
                else { Campaign.StartRun(); DuelFlow.Story(StoryKind.ChapterIntro); }
            });
        }

        static void Chip(UiBuild ui, RectTransform p, string name, string label, Vector2 anchor, Vector2 pos, Action go)
        {
            var btn = ui.Button(p, name, label, anchor, pos, 220, 72, 28, UiBuild.Normal, out _);
            btn.onClick.AddListener(() => go());
        }
    }
}
