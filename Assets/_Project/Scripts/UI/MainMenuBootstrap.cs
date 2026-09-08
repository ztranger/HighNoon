using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem.UI;

namespace HighNoon
{
    /// <summary>
    /// Classic portrait mobile menu: a bottom bar of 5 tabs switching full-screen panels.
    /// Tabs: STATS (records) · GUNS (weapon select) · HOME (idle hero + PvP/PvE) · SETUP (the
    /// full match-config / test menu) · SOUND (audio + tutorial). Home is the default tab.
    /// Everything is built in code (no scene wiring). Writes selections to <see cref="MatchSettings"/>.
    /// </summary>
    public class MainMenuBootstrap : MonoBehaviour
    {
        static readonly Color Selected  = new Color(0.88f, 0.66f, 0.22f);
        static readonly Color Normal    = new Color(0.48f, 0.38f, 0.24f);
        static readonly Color Disabled  = new Color(0.34f, 0.31f, 0.29f);
        static readonly Color Gold      = new Color(0.92f, 0.78f, 0.35f);
        static readonly Color TabActive = new Color(0.86f, 0.64f, 0.22f);
        static readonly Color TabNormal = new Color(0.28f, 0.22f, 0.16f);
        const float TabBar = 176f; // reference px reserved at the bottom for the tab strip

        Font _font;
        readonly RectTransform[] _panels = new RectTransform[5];
        readonly Image[] _tabImgs = new Image[5];
        readonly Text[] _tabLabels = new Text[5];

        // Setup-tab selection highlights
        Image _pvpImg, _coopImg, _pveImg, _reactionImg, _timingImg, _p1Img, _p2Img, _easyImg, _normalImg, _hardImg;
        // Sound-tab toggles
        Image _sfxImg, _musicImg, _vibImg;
        Text _sfxLabel, _musicLabel, _vibLabel;
        // Weapon displays (setup carousel + guns tab)
        Text _weaponSetupName, _weaponTabName;
        Image _weaponIcon;

        void Start()
        {
            AppInit.Apply();
            if (!GameSettings.TutorialDone) { SceneManager.LoadScene("Tutorial"); return; } // first launch → teach

            _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (_font == null) _font = Resources.GetBuiltinResource<Font>("Arial.ttf");

            Campaign.LoadSavedRun();
            SetupCamera();
            SetupEventSystem();
            BuildUI();
            SelectTab(2);            // HOME
            RefreshHighlights();
        }

        void BuildUI()
        {
            var canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 1f; // match height so the bottom tab bar stays put
            gameObject.AddComponent<GraphicRaycaster>();

            var bg = NewRect("BG", transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            bg.gameObject.AddComponent<Image>().color = new Color(0.16f, 0.11f, 0.09f);

            for (int i = 0; i < 5; i++) _panels[i] = MakePanel("Panel" + i);
            BuildRecordsPanel(_panels[0]);
            BuildWeaponsPanel(_panels[1]);
            BuildHomePanel(_panels[2]);
            BuildSetupPanel(_panels[3]);
            BuildSettingsPanel(_panels[4]);

            BuildTabBar();
        }

        // ---------- tabs ----------

        void BuildTabBar()
        {
            var bar = NewRect("TabBar", transform, new Vector2(0f, 0f), new Vector2(1f, 0f), Vector2.zero, Vector2.zero);
            bar.sizeDelta = new Vector2(0f, TabBar);
            bar.pivot = new Vector2(0.5f, 0f);
            bar.anchoredPosition = Vector2.zero;
            bar.gameObject.AddComponent<Image>().color = new Color(0.11f, 0.08f, 0.06f);

            string[] labels = { "STATS", "GUNS", "HOME", "SETUP", "SOUND" };
            for (int i = 0; i < 5; i++)
            {
                int idx = i;
                var go = new GameObject("Tab" + i);
                var rt = go.AddComponent<RectTransform>();
                rt.SetParent(bar, false);
                rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.sizeDelta = new Vector2(206f, TabBar - 16f);
                rt.anchoredPosition = new Vector2((i - 2) * 212f, 0f);
                _tabImgs[i] = go.AddComponent<Image>();
                _tabImgs[i].color = TabNormal;
                var btn = go.AddComponent<Button>();
                btn.targetGraphic = _tabImgs[i];
                btn.onClick.AddListener(() => { Sfx.Click(); Haptics.Light(); SelectTab(idx); });
                _tabLabels[i] = MakeText(rt, "L", labels[i], 30, new Vector2(0.5f, 0.5f), Vector2.zero, 200, 100, Color.white, FontStyle.Bold);
            }
        }

        void SelectTab(int i)
        {
            for (int k = 0; k < 5; k++)
            {
                if (_panels[k] != null) _panels[k].gameObject.SetActive(k == i);
                if (_tabImgs[k] != null) _tabImgs[k].color = k == i ? TabActive : TabNormal;
                if (_tabLabels[k] != null) _tabLabels[k].color = k == i ? new Color(0.15f, 0.10f, 0.06f) : new Color(0.85f, 0.78f, 0.66f);
            }
        }

        RectTransform MakePanel(string name)
        {
            var rt = NewRect(name, transform, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0f, TabBar), Vector2.zero);
            return rt;
        }

        // ---------- HOME ----------

        void BuildHomePanel(RectTransform p)
        {
            MakeText(p, "Title", "HIGH NOON", 140, new Vector2(0.5f, 0.93f), Vector2.zero, 1000, 200, Gold, FontStyle.Bold);
            MakeText(p, "Sub", "— Wild West Duel —", 46, new Vector2(0.5f, 0.855f), Vector2.zero, 900, 80, new Color(0.8f, 0.7f, 0.5f), FontStyle.Normal);

            // Idle hero, centred and a little up.
            var heroGo = new GameObject("Hero");
            var hrt = heroGo.AddComponent<RectTransform>();
            hrt.SetParent(p, false);
            hrt.anchorMin = hrt.anchorMax = new Vector2(0.5f, 0.60f);
            hrt.pivot = new Vector2(0.5f, 0.5f);
            hrt.sizeDelta = new Vector2(300f, 400f);
            var heroImg = heroGo.AddComponent<Image>();
            heroImg.raycastTarget = false;
            heroImg.preserveAspect = true;
            var frames = CowboyArt.Build(CowboyLook.Player());
            heroImg.sprite = frames.Idle[0];
            heroGo.AddComponent<UiSpriteAnim>().Play(heroImg, frames.Idle, 2.5f);

            var pvp = MakeButton(p, "HomePvP", "PvP  DUEL", new Vector2(0.5f, 0.285f), Vector2.zero, 660, 150, 60, Gold, out _, true);
            pvp.onClick.AddListener(() => { MatchSettings.Mode = GameMode.PvP; SceneManager.LoadScene("Duel"); });

            var pve = MakeButton(p, "HomePvE", "PvE  CAMPAIGN", new Vector2(0.5f, 0.135f), Vector2.zero, 660, 150, 56, new Color(0.70f, 0.52f, 0.24f), out _, true);
            pve.onClick.AddListener(() =>
            {
                MatchSettings.Mode = GameMode.PvE;
                if (Campaign.HasSavedRun) SceneManager.LoadScene("Map");
                else { Campaign.StartRun(); Story.Kind = StoryKind.ChapterIntro; SceneManager.LoadScene("Story"); }
            });
        }

        // ---------- GUNS ----------

        void BuildWeaponsPanel(RectTransform p)
        {
            MakeText(p, "Head", "WEAPON", 60, new Vector2(0.5f, 0.92f), Vector2.zero, 900, 100, Gold, FontStyle.Bold);

            var iconGo = new GameObject("WeaponIcon");
            var irt = iconGo.AddComponent<RectTransform>();
            irt.SetParent(p, false);
            irt.anchorMin = irt.anchorMax = new Vector2(0.5f, 0.62f);
            irt.pivot = new Vector2(0.5f, 0.5f);
            irt.sizeDelta = new Vector2(520f, 340f);
            _weaponIcon = iconGo.AddComponent<Image>();
            _weaponIcon.raycastTarget = false;
            _weaponIcon.preserveAspect = true;

            _weaponTabName = MakeText(p, "WName", "REVOLVER", 64, new Vector2(0.5f, 0.36f), Vector2.zero, 700, 120, Gold, FontStyle.Bold);

            var prev = MakeButton(p, "GunPrev", "<", new Vector2(0.5f, 0.36f), new Vector2(-420, 0), 150, 150, 64, Normal, out _, true);
            var next = MakeButton(p, "GunNext", ">", new Vector2(0.5f, 0.36f), new Vector2(420, 0), 150, 150, 64, Normal, out _, true);
            prev.onClick.AddListener(() => CycleWeapon(-1));
            next.onClick.AddListener(() => CycleWeapon(1));

            MakeText(p, "Hint", "tap  ◄  ►  to choose your iron", 34, new Vector2(0.5f, 0.20f), Vector2.zero, 900, 70, new Color(0.72f, 0.66f, 0.52f), FontStyle.Normal);
        }

        // ---------- STATS ----------

        void BuildRecordsPanel(RectTransform p)
        {
            MakeText(p, "Head", "RECORDS", 60, new Vector2(0.5f, 0.92f), Vector2.zero, 900, 100, Gold, FontStyle.Bold);

            StatRow(p, "FASTEST DRAW", Records.HasReaction ? $"{Records.BestReactionMs:0} ms" : "—", 0.72f);
            StatRow(p, "BEST AIM",     Records.HasAccuracy ? $"{Records.BestAccuracy * 100f:0}%" : "—", 0.60f);
            StatRow(p, "CAMPAIGNS WON", $"{Records.Completions}", 0.48f);
            StatRow(p, "FURTHEST",     (Records.FurthestChapter > 0 || Records.FurthestStage > 0)
                                        ? $"CH {Records.FurthestChapter + 1} · {Records.FurthestStage + 1}" : "—", 0.36f);

            if (!Records.HasReaction && !Records.HasAccuracy && Records.Completions == 0)
                MakeText(p, "Empty", "No records yet — go make history.", 34, new Vector2(0.5f, 0.20f), Vector2.zero, 900, 70, new Color(0.7f, 0.64f, 0.5f), FontStyle.Italic);
        }

        void StatRow(RectTransform p, string label, string value, float y)
        {
            MakeText(p, label + "L", label, 40, new Vector2(0.5f, y), new Vector2(-230, 0), 520, 70, new Color(0.72f, 0.66f, 0.52f), FontStyle.Normal);
            MakeText(p, label + "V", value, 46, new Vector2(0.5f, y), new Vector2(260, 0), 380, 70, Gold, FontStyle.Bold);
        }

        // ---------- SETUP (the full test menu) ----------

        void BuildSetupPanel(RectTransform p)
        {
            MakeText(p, "Head", "SETUP", 52, new Vector2(0.5f, 0.965f), Vector2.zero, 900, 80, new Color(0.85f, 0.78f, 0.62f), FontStyle.Bold);

            MakeText(p, "ModeLabel", "MODE", 38, new Vector2(0.5f, 0.90f), Vector2.zero, 900, 56, LabelCol(), FontStyle.Normal);
            var pvp = MakeButton(p, "ModePvP", "PvP", new Vector2(0.5f, 0.85f), new Vector2(-320, 0), 300, 90, 42, Normal, out _pvpImg, true);
            var coop = MakeButton(p, "ModeCoop", "COOP", new Vector2(0.5f, 0.85f), new Vector2(0, 0), 300, 90, 38, Normal, out _coopImg, true);
            var pve = MakeButton(p, "ModePvE", "PvE", new Vector2(0.5f, 0.85f), new Vector2(320, 0), 300, 90, 42, Normal, out _pveImg, true);
            pvp.onClick.AddListener(() => { MatchSettings.Mode = GameMode.PvP; RefreshHighlights(); });
            coop.onClick.AddListener(() => { MatchSettings.Mode = GameMode.Coop; RefreshHighlights(); });
            pve.onClick.AddListener(() => { MatchSettings.Mode = GameMode.PvE; RefreshHighlights(); });

            MakeText(p, "TypeLabel", "DUEL TYPE", 38, new Vector2(0.5f, 0.77f), Vector2.zero, 900, 56, LabelCol(), FontStyle.Normal);
            var reaction = MakeButton(p, "Reaction", "REACTION", new Vector2(0.5f, 0.72f), new Vector2(-250, 0), 470, 90, 38, Normal, out _reactionImg, true);
            var timing = MakeButton(p, "Timing", "TIMING", new Vector2(0.5f, 0.72f), new Vector2(250, 0), 470, 90, 38, Normal, out _timingImg, true);
            reaction.onClick.AddListener(() => { MatchSettings.Type = DuelType.Reaction; RefreshHighlights(); });
            timing.onClick.AddListener(() => { MatchSettings.Type = DuelType.Timing; RefreshHighlights(); });

            MakeText(p, "PlayersLabel", "PLAYERS", 38, new Vector2(0.5f, 0.64f), Vector2.zero, 900, 56, LabelCol(), FontStyle.Normal);
            var p1 = MakeButton(p, "OnePlayer", "1 PLAYER", new Vector2(0.5f, 0.59f), new Vector2(-250, 0), 470, 90, 42, Normal, out _p1Img, true);
            var p2 = MakeButton(p, "TwoPlayers", "2 PLAYERS", new Vector2(0.5f, 0.59f), new Vector2(250, 0), 470, 90, 42, Normal, out _p2Img, true);
            p1.onClick.AddListener(() => { MatchSettings.Players = PvPPlayers.OnePlayer; RefreshHighlights(); });
            p2.onClick.AddListener(() => { MatchSettings.Players = PvPPlayers.TwoPlayers; RefreshHighlights(); });

            MakeText(p, "DiffLabel", "BOT DIFFICULTY", 38, new Vector2(0.5f, 0.51f), Vector2.zero, 900, 56, LabelCol(), FontStyle.Normal);
            var easy = MakeButton(p, "Easy", "EASY", new Vector2(0.5f, 0.46f), new Vector2(-320, 0), 300, 90, 38, Normal, out _easyImg, true);
            var norm = MakeButton(p, "Normal", "NORMAL", new Vector2(0.5f, 0.46f), new Vector2(0, 0), 300, 90, 38, Normal, out _normalImg, true);
            var hard = MakeButton(p, "Hard", "HARD", new Vector2(0.5f, 0.46f), new Vector2(320, 0), 300, 90, 38, Normal, out _hardImg, true);
            easy.onClick.AddListener(() => { MatchSettings.BotDifficulty = Difficulty.Easy; RefreshHighlights(); });
            norm.onClick.AddListener(() => { MatchSettings.BotDifficulty = Difficulty.Normal; RefreshHighlights(); });
            hard.onClick.AddListener(() => { MatchSettings.BotDifficulty = Difficulty.Hard; RefreshHighlights(); });

            var wprev = MakeButton(p, "SWeaponPrev", "<", new Vector2(0.5f, 0.375f), new Vector2(-360, 0), 120, 84, 40, Normal, out _, true);
            var wnext = MakeButton(p, "SWeaponNext", ">", new Vector2(0.5f, 0.375f), new Vector2(360, 0), 120, 84, 40, Normal, out _, true);
            _weaponSetupName = MakeText(p, "SWeaponName", "WEAPON", 38, new Vector2(0.5f, 0.375f), Vector2.zero, 560, 84, Gold, FontStyle.Bold);
            wprev.onClick.AddListener(() => CycleWeapon(-1));
            wnext.onClick.AddListener(() => CycleWeapon(1));

            if (Campaign.HasSavedRun)
            {
                var cont = MakeButton(p, "Continue", "CONTINUE CAMPAIGN", new Vector2(0.5f, 0.26f), Vector2.zero, 620, 92, 40, new Color(0.55f, 0.70f, 0.40f), out _, true);
                cont.onClick.AddListener(() => { MatchSettings.Mode = GameMode.PvE; SceneManager.LoadScene("Map"); });
            }

            var play = MakeButton(p, "Play", "PLAY", new Vector2(0.5f, 0.135f), Vector2.zero, 620, 140, 66, Gold, out _, true);
            play.onClick.AddListener(() =>
            {
                if (MatchSettings.Mode == GameMode.PvE)
                {
                    Campaign.StartRun();
                    Story.Kind = StoryKind.ChapterIntro;
                    SceneManager.LoadScene("Story");
                }
                else SceneManager.LoadScene("Duel");
            });
        }

        // ---------- SOUND ----------

        void BuildSettingsPanel(RectTransform p)
        {
            MakeText(p, "Head", "SETTINGS", 60, new Vector2(0.5f, 0.92f), Vector2.zero, 900, 100, Gold, FontStyle.Bold);

            var sfx = MakeButton(p, "SfxToggle", "SFX", new Vector2(0.5f, 0.74f), Vector2.zero, 640, 110, 44, Normal, out _sfxImg, true);
            var mus = MakeButton(p, "MusicToggle", "MUSIC", new Vector2(0.5f, 0.61f), Vector2.zero, 640, 110, 44, Normal, out _musicImg, true);
            var vib = MakeButton(p, "VibToggle", "VIBRATION", new Vector2(0.5f, 0.48f), Vector2.zero, 640, 110, 44, Normal, out _vibImg, true);
            _sfxLabel = sfx.GetComponentInChildren<Text>();
            _musicLabel = mus.GetComponentInChildren<Text>();
            _vibLabel = vib.GetComponentInChildren<Text>();
            sfx.onClick.AddListener(() => { GameSettings.SfxEnabled = !GameSettings.SfxEnabled; RefreshHighlights(); });
            mus.onClick.AddListener(() => { GameSettings.MusicEnabled = !GameSettings.MusicEnabled; RefreshHighlights(); });
            vib.onClick.AddListener(() => { GameSettings.HapticsEnabled = !GameSettings.HapticsEnabled; RefreshHighlights(); Haptics.Medium(); });

            var howto = MakeButton(p, "HowTo", "HOW TO PLAY", new Vector2(0.5f, 0.31f), Vector2.zero, 640, 110, 42, new Color(0.60f, 0.50f, 0.34f), out _, true);
            howto.onClick.AddListener(() => SceneManager.LoadScene("Tutorial"));
        }

        // ---------- shared ----------

        void CycleWeapon(int dir)
        {
            GameSettings.SelectedWeapon = (GameSettings.SelectedWeapon + dir + Weapons.Count) % Weapons.Count;
            RefreshWeapon();
            Sfx.PlayClip(AudioBank.GunshotFor(Weapons.Selected, ProcAudio.Gunshot)); // hear the pick
        }

        void RefreshWeapon()
        {
            var w = Weapons.Selected;
            if (_weaponTabName != null) _weaponTabName.text = w.Name;
            if (_weaponSetupName != null) _weaponSetupName.text = "WEAPON: " + w.Name;
            if (_weaponIcon != null)
                _weaponIcon.sprite = WeaponArt.For(GameSettings.SelectedWeapon); // distinct silhouette per weapon
        }

        void RefreshHighlights()
        {
            var mode = MatchSettings.Mode;
            if (_pvpImg != null)  _pvpImg.color  = mode == GameMode.PvP  ? Selected : Normal;
            if (_coopImg != null) _coopImg.color = mode == GameMode.Coop ? Selected : Normal;
            if (_pveImg != null)  _pveImg.color  = mode == GameMode.PvE  ? Selected : Normal;

            if (_reactionImg != null) _reactionImg.color = MatchSettings.Type == DuelType.Reaction ? Selected : Normal;
            if (_timingImg != null)   _timingImg.color   = MatchSettings.Type == DuelType.Timing   ? Selected : Normal;

            if (_p1Img != null) _p1Img.color = MatchSettings.Players == PvPPlayers.OnePlayer ? Selected : Normal;
            if (_p2Img != null) _p2Img.color = MatchSettings.Players == PvPPlayers.TwoPlayers ? Selected : Normal;

            if (_easyImg != null)   _easyImg.color   = MatchSettings.BotDifficulty == Difficulty.Easy   ? Selected : Normal;
            if (_normalImg != null) _normalImg.color = MatchSettings.BotDifficulty == Difficulty.Normal ? Selected : Normal;
            if (_hardImg != null)   _hardImg.color   = MatchSettings.BotDifficulty == Difficulty.Hard   ? Selected : Normal;

            float ta = mode != GameMode.PvE ? 1f : 0.4f;   // duel type: PvE decides per stage
            SetAlpha(_reactionImg, ta); SetAlpha(_timingImg, ta);
            float pa = mode != GameMode.Coop ? 1f : 0.4f;  // Coop is always 2 players
            SetAlpha(_p1Img, pa); SetAlpha(_p2Img, pa);
            bool botUsed = (mode == GameMode.PvP && MatchSettings.Players == PvPPlayers.OnePlayer) || mode == GameMode.Coop;
            float da = botUsed ? 1f : 0.4f;
            SetAlpha(_easyImg, da); SetAlpha(_normalImg, da); SetAlpha(_hardImg, da);

            if (_sfxImg != null) { _sfxImg.color = GameSettings.SfxEnabled ? Selected : Disabled; if (_sfxLabel != null) _sfxLabel.text = GameSettings.SfxEnabled ? "SFX: ON" : "SFX: OFF"; }
            if (_musicImg != null) { _musicImg.color = GameSettings.MusicEnabled ? Selected : Disabled; if (_musicLabel != null) _musicLabel.text = GameSettings.MusicEnabled ? "MUSIC: ON" : "MUSIC: OFF"; }
            if (_vibImg != null) { _vibImg.color = GameSettings.HapticsEnabled ? Selected : Disabled; if (_vibLabel != null) _vibLabel.text = GameSettings.HapticsEnabled ? "VIBRATION: ON" : "VIBRATION: OFF"; }

            RefreshWeapon();
        }

        static Color LabelCol() => new Color(0.7f, 0.62f, 0.5f);

        static void SetAlpha(Image img, float a)
        {
            if (img == null) return;
            var c = img.color; c.a = a; img.color = c;
        }

        // ---------- UI builders ----------

        RectTransform NewRect(string name, Transform parent, Vector2 aMin, Vector2 aMax, Vector2 offMin, Vector2 offMax)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = aMin; rt.anchorMax = aMax;
            rt.offsetMin = offMin; rt.offsetMax = offMax;
            return rt;
        }

        Text MakeText(Transform parent, string name, string content, int size, Vector2 anchor, Vector2 pos, float w, float h, Color color, FontStyle style)
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
            txt.font = _font; txt.fontSize = size; txt.fontStyle = style;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color = color;
            txt.horizontalOverflow = HorizontalWrapMode.Overflow;
            txt.verticalOverflow = VerticalWrapMode.Overflow;
            txt.raycastTarget = false;
            return txt;
        }

        Button MakeButton(Transform parent, string name, string label, Vector2 anchor, Vector2 pos, float w, float h, int fontSize, Color color, out Image image, bool interactable)
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
            var t = MakeText(go.transform, "Label", label, fontSize, new Vector2(0.5f, 0.5f), Vector2.zero, w - 20, h - 10, Color.black, FontStyle.Bold);
            t.color = new Color(0.15f, 0.10f, 0.06f);
            return btn;
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
            cam.backgroundColor = new Color(0.16f, 0.11f, 0.09f);
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
