using System;
using UnityEngine;
using UnityEngine.UI;

namespace HighNoon
{
    /// <summary>Match-config / test screen. Weapon row opens <see cref="GunsScreen"/>.</summary>
    public sealed class SetupScreen
    {
        public RectTransform Root { get; private set; }

        Image _pvpImg, _coopImg, _pveImg, _reactionImg, _timingImg, _p1Img, _p2Img, _easyImg, _normalImg, _hardImg;
        Text _weaponName;
        Action _refresh;

        public void Build(RectTransform root, UiBuild ui, Action back, Action<MenuScreenId> go, Action refresh)
        {
            Root = root;
            _refresh = refresh;
            ui.Back(root, back);
            ui.Text(root, "Head", "SETUP", 52, new Vector2(0.5f, 0.90f), Vector2.zero, 900, 70, new Color(0.85f, 0.78f, 0.62f), FontStyle.Bold);

            ui.Text(root, "ModeLabel", "MODE", 28, new Vector2(0.5f, 0.5f), new Vector2(-420f, 300f), 700, 40, UiBuild.Label, FontStyle.Normal);
            var pvp = ui.Button(root, "ModePvP", "PvP", new Vector2(0.5f, 0.5f), new Vector2(-640f, 220f), 200, 80, 32, UiBuild.Normal, out _pvpImg);
            var coop = ui.Button(root, "ModeCoop", "COOP", new Vector2(0.5f, 0.5f), new Vector2(-420f, 220f), 200, 80, 30, UiBuild.Normal, out _coopImg);
            var pve = ui.Button(root, "ModePvE", "PvE", new Vector2(0.5f, 0.5f), new Vector2(-200f, 220f), 200, 80, 32, UiBuild.Normal, out _pveImg);
            pvp.onClick.AddListener(() => { MatchSettings.Mode = GameMode.PvP; _refresh(); });
            coop.onClick.AddListener(() => { MatchSettings.Mode = GameMode.Coop; _refresh(); });
            pve.onClick.AddListener(() => { MatchSettings.Mode = GameMode.PvE; _refresh(); });

            ui.Text(root, "TypeLabel", "DUEL TYPE", 28, new Vector2(0.5f, 0.5f), new Vector2(-420f, 120f), 700, 40, UiBuild.Label, FontStyle.Normal);
            var reaction = ui.Button(root, "Reaction", "REACTION", new Vector2(0.5f, 0.5f), new Vector2(-560f, 40f), 280, 80, 30, UiBuild.Normal, out _reactionImg);
            var timing = ui.Button(root, "Timing", "TIMING", new Vector2(0.5f, 0.5f), new Vector2(-260f, 40f), 280, 80, 30, UiBuild.Normal, out _timingImg);
            reaction.onClick.AddListener(() => { MatchSettings.Type = DuelType.Reaction; _refresh(); });
            timing.onClick.AddListener(() => { MatchSettings.Type = DuelType.Timing; _refresh(); });

            ui.Text(root, "PlayersLabel", "PLAYERS", 28, new Vector2(0.5f, 0.5f), new Vector2(-420f, -60f), 700, 40, UiBuild.Label, FontStyle.Normal);
            var p1 = ui.Button(root, "OnePlayer", "1 PLAYER", new Vector2(0.5f, 0.5f), new Vector2(-560f, -140f), 280, 80, 30, UiBuild.Normal, out _p1Img);
            var p2 = ui.Button(root, "TwoPlayers", "2 PLAYERS", new Vector2(0.5f, 0.5f), new Vector2(-260f, -140f), 280, 80, 30, UiBuild.Normal, out _p2Img);
            p1.onClick.AddListener(() => { MatchSettings.Players = PvPPlayers.OnePlayer; _refresh(); });
            p2.onClick.AddListener(() => { MatchSettings.Players = PvPPlayers.TwoPlayers; _refresh(); });

            ui.Text(root, "DiffLabel", "BOT DIFFICULTY", 28, new Vector2(0.5f, 0.5f), new Vector2(420f, 300f), 700, 40, UiBuild.Label, FontStyle.Normal);
            var easy = ui.Button(root, "Easy", "EASY", new Vector2(0.5f, 0.5f), new Vector2(200f, 220f), 200, 80, 30, UiBuild.Normal, out _easyImg);
            var norm = ui.Button(root, "Normal", "NORMAL", new Vector2(0.5f, 0.5f), new Vector2(420f, 220f), 200, 80, 30, UiBuild.Normal, out _normalImg);
            var hard = ui.Button(root, "Hard", "HARD", new Vector2(0.5f, 0.5f), new Vector2(640f, 220f), 200, 80, 30, UiBuild.Normal, out _hardImg);
            easy.onClick.AddListener(() => { MatchSettings.BotDifficulty = Difficulty.Easy; _refresh(); });
            norm.onClick.AddListener(() => { MatchSettings.BotDifficulty = Difficulty.Normal; _refresh(); });
            hard.onClick.AddListener(() => { MatchSettings.BotDifficulty = Difficulty.Hard; _refresh(); });

            var wopen = ui.Button(root, "SetupWeapon", "WEAPON", new Vector2(0.5f, 0.5f), new Vector2(420f, 40f), 640, 80, 30, UiBuild.Normal, out _);
            _weaponName = wopen.GetComponentInChildren<Text>();
            wopen.onClick.AddListener(() => go(MenuScreenId.Guns));

            if (Campaign.HasSavedRun)
            {
                var cont = ui.Button(root, "Continue", "CONTINUE CAMPAIGN", new Vector2(0.5f, 0.5f), new Vector2(420f, -80f), 640, 80, 30, new Color(0.55f, 0.70f, 0.40f), out _);
                cont.onClick.AddListener(() => { MatchSettings.Mode = GameMode.PvE; DuelFlow.Map(); });
            }

            var play = ui.Button(root, "Play", "PLAY", new Vector2(0.5f, 0.5f), new Vector2(420f, -220f), 640, 120, 56, UiBuild.Gold, out _);
            play.onClick.AddListener(() =>
            {
                if (MatchSettings.Mode == GameMode.PvE)
                {
                    Campaign.StartRun();
                    DuelFlow.Story(StoryKind.ChapterIntro);
                }
                else DuelFlow.Duel();
            });
        }

        public void Refresh()
        {
            var mode = MatchSettings.Mode;
            if (_pvpImg != null)  _pvpImg.color  = mode == GameMode.PvP  ? UiBuild.Selected : UiBuild.Normal;
            if (_coopImg != null) _coopImg.color = mode == GameMode.Coop ? UiBuild.Selected : UiBuild.Normal;
            if (_pveImg != null)  _pveImg.color  = mode == GameMode.PvE  ? UiBuild.Selected : UiBuild.Normal;

            if (_reactionImg != null) _reactionImg.color = MatchSettings.Type == DuelType.Reaction ? UiBuild.Selected : UiBuild.Normal;
            if (_timingImg != null)   _timingImg.color   = MatchSettings.Type == DuelType.Timing   ? UiBuild.Selected : UiBuild.Normal;

            if (_p1Img != null) _p1Img.color = MatchSettings.Players == PvPPlayers.OnePlayer ? UiBuild.Selected : UiBuild.Normal;
            if (_p2Img != null) _p2Img.color = MatchSettings.Players == PvPPlayers.TwoPlayers ? UiBuild.Selected : UiBuild.Normal;

            if (_easyImg != null)   _easyImg.color   = MatchSettings.BotDifficulty == Difficulty.Easy   ? UiBuild.Selected : UiBuild.Normal;
            if (_normalImg != null) _normalImg.color = MatchSettings.BotDifficulty == Difficulty.Normal ? UiBuild.Selected : UiBuild.Normal;
            if (_hardImg != null)   _hardImg.color   = MatchSettings.BotDifficulty == Difficulty.Hard   ? UiBuild.Selected : UiBuild.Normal;

            float ta = mode != GameMode.PvE ? 1f : 0.4f;
            UiBuild.SetAlpha(_reactionImg, ta); UiBuild.SetAlpha(_timingImg, ta);
            float pa = mode != GameMode.Coop ? 1f : 0.4f;
            UiBuild.SetAlpha(_p1Img, pa); UiBuild.SetAlpha(_p2Img, pa);
            bool botUsed = (mode == GameMode.PvP && MatchSettings.Players == PvPPlayers.OnePlayer) || mode == GameMode.Coop;
            float da = botUsed ? 1f : 0.4f;
            UiBuild.SetAlpha(_easyImg, da); UiBuild.SetAlpha(_normalImg, da); UiBuild.SetAlpha(_hardImg, da);

            if (_weaponName != null) _weaponName.text = "WEAPON: " + Weapons.Selected.Name;
        }
    }
}
