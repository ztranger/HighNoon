using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.InputSystem.EnhancedTouch;

namespace HighNoon
{
    /// <summary>
    /// Assembles a duel scene at runtime: arena background, camera, HUD, audio, and the
    /// duelists for the chosen mode (PvP = 1 lane, Coop = 2v2 across two lanes, Volley = one
    /// human against 2–3 foes on the right).
    /// </summary>
    public class DuelBootstrap : MonoBehaviour
    {
        [Header("Match setup")]
        [Tooltip("Read mode/players/difficulty from the main menu. Uncheck to use the manual PvP flags when opening this scene directly.")]
        public bool useMatchSettings = true;

        [Tooltip("PvP only, used when 'useMatchSettings' is off.")]
        public bool topIsBot = true;
        public bool bottomIsBot = false;

        [Tooltip("Duel type used when 'useMatchSettings' is off (direct-scene testing).")]
        public DuelType manualDuelType = DuelType.Reaction;

        [Tooltip("Volley foe count when 'useMatchSettings' is off. Clamped to 2–3.")]
        public int manualVolleyOpponents = 3;

        [Tooltip("Sync foe count when 'useMatchSettings' is off. Even, 2 or 4. Shots per hand = count / 2.")]
        public int manualSyncOpponents = 2;

        [Header("Configs (optional — runtime defaults created if empty)")]
        public DuelConfig duelConfig;
        public BotConfig botConfig;

        CameraShake _shake;
        ArenaDef _arena;

        // Session-cached fallbacks so Play-from-Duel / match-settings don't leak a
        // new ScriptableObject on every scene load (UnityEngine.Object until domain unload).
        static DuelConfig _runtimeDuel;
        static BotConfig _runtimeBot;

        // Landscape showdown tap zones: each player taps their side of the street.
        static readonly Rect LeftHalf  = new Rect(0f, 0f, 0.5f, 1f);
        static readonly Rect RightHalf = new Rect(0.5f, 0f, 0.5f, 1f);
        // Coop: the two humans share the LEFT side → split it into upper / lower.
        static readonly Rect LeftUpper = new Rect(0f, 0.5f, 0.5f, 0.5f);
        static readonly Rect LeftLower = new Rect(0f, 0f, 0.5f, 0.5f);

        // Field positions (world units) — cowboys stand at opposite ends of the street.
        static readonly Vector2 SoloLeft  = new Vector2(-5.0f, 0f);
        static readonly Vector2 SoloRight = new Vector2( 5.0f, 0f);
        static readonly Vector2 PairLeftA  = new Vector2(-5.6f,  1.7f);
        static readonly Vector2 PairLeftB  = new Vector2(-4.7f, -1.8f);
        static readonly Vector2 PairRightA = new Vector2( 5.6f,  1.7f);
        static readonly Vector2 PairRightB = new Vector2( 4.7f, -1.8f);

        void Start()
        {
            AppInit.Apply();
            EnhancedTouchSupport.Enable();

            bool pve = useMatchSettings && MatchSettings.Mode == GameMode.PvE;
            if (pve)
            {
                if (!Campaign.Active) Campaign.StartRun();
                Campaign.ApplyToMatch(); // pins this stage's arena + difficulty
            }
            else if (useMatchSettings)
            {
                // PvP/Coop: honor the SETUP/HOME location pick (null = random).
                MatchSettings.ForcedArena = MatchSettings.MenuArena;
            }

            if (duelConfig == null) duelConfig = RuntimeDuelConfig();
            if (useMatchSettings)
            {
                botConfig = RuntimeBotConfig();
                MatchSettings.ApplyDifficulty(botConfig);
            }
            else if (botConfig == null)
            {
                botConfig = RuntimeBotConfig();
                botConfig.ApplyDefaults();
            }

            _arena = Arenas.Pick(MatchSettings.ForcedArena);
            SetupCamera();
            BackgroundBuilder.Build(_arena);
            SetupEventSystem();

            var hud = new GameObject("HUD").AddComponent<DuelHUD>();
            hud.Setup();

            var audio = new GameObject("DuelAudio").AddComponent<DuelAudio>();
            audio.Setup();

            bool coop = useMatchSettings && MatchSettings.Mode == GameMode.Coop;
            DuelType type = useMatchSettings ? MatchSettings.Type : manualDuelType;
            // Volley and Sync are stage types. A campaign duel leaves them on MatchSettings;
            // PvP/Coop only play Reaction or Timing.
            if (useMatchSettings && !pve && type != DuelType.Reaction && type != DuelType.Timing)
                type = DuelType.Timing;
            var duelists = type == DuelType.Volley ? BuildVolley(pve)
                : type == DuelType.Sync ? BuildSync(pve)
                : pve ? BuildPve()
                : coop ? BuildCoop()
                : BuildPvp();

            var manager = new GameObject("DuelManager").AddComponent<DuelManager>();
            manager.Config = duelConfig;
            manager.Hud = hud;
            manager.Audio = audio;
            manager.Shake = _shake;
            manager.PveMode = pve;
            manager.Type = type;
            manager.Duelists = duelists;
            if (pve)
            {
                // Volley and Sync are one gunslinger — the 2-player toggle does not add a partner.
                string tag = type == DuelType.Volley ? "VOLLEY"
                    : type == DuelType.Sync ? "SYNC"
                    : MatchSettings.Players == PvPPlayers.TwoPlayers ? "CO-OP" : "SOLO";
                hud.SetPveStatus($"CH{Campaign.Chapter + 1} · {Campaign.Stage + 1}/{Campaign.CurrentChapter.Stages.Length}   ·   {Campaign.CurrentStage.Title}   ·   LIVES {Campaign.Lives}   ·   {tag}");
            }

            var stage = pve ? Campaign.CurrentStage : null;
            bool playIntro = pve && Campaign.ShowIntro && stage.Intro != null && stage.Intro.Length > 0;
            Campaign.ShowIntro = false; // consume — a RETRY of the same stage won't replay the banter

            if (playIntro)
            {
                // Duelists already stand on the field; talk first, then StartDuel skips the walk-in.
                manager.SkipFirstIntro = true;
                foreach (var d in duelists) d.View.Stance();
                var dialog = new GameObject("Dialog").AddComponent<DialogBox>();
                dialog.Setup();
                var playerLook = CowboyLook.Player();
                var oppLook = stage.Look ?? CowboyLook.Enemy();
                dialog.Play(stage.Intro,
                    "YOU", playerLook.Shirt, PortraitOf(duelists, DuelSide.Bottom),
                    stage.Title, oppLook.Shirt, PortraitOf(duelists, DuelSide.Top),
                    manager.StartDuel);
            }
            else
            {
                manager.StartDuel();
            }
        }

        /// <summary>
        /// One human on the left, <see cref="Volley.FoeCount"/> foes on the right.
        /// Kill order is list order: front of the line first. PvE reads the stage; a direct
        /// scene play uses <see cref="manualVolleyOpponents"/>.
        /// </summary>
        List<Duelist> BuildVolley(bool pve)
        {
            int n = Volley.FoeCount(pve ? Campaign.CurrentStage.Opponents : manualVolleyOpponents);
            string title = pve ? Campaign.CurrentStage.Title : "GUNMAN";
            var leader = pve ? (Campaign.CurrentStage.Look ?? CowboyLook.Enemy()) : CowboyLook.Enemy();

            var list = new List<Duelist>
            {
                MakeDuelist(DuelSide.Bottom, 0, SoloLeft, false, CowboyLook.Player(), "YOU", LeftHalf, Key.S),
            };
            for (int i = 0; i < n; i++)
            {
                list.Add(MakeDuelist(
                    DuelSide.Top, i, VolleySpot(i, n), true,
                    VolleyLook(leader, i), $"{title} {i + 1}", RightHalf, Key.None));
            }
            return list;
        }

        /// <summary>Front (index 0) is shot first. Bootstrap y is a depth stagger, see <see cref="DuelistView"/>.</summary>
        static Vector2 VolleySpot(int index, int count)
        {
            if (count <= 2)
                return index == 0 ? new Vector2(4.6f, -2.2f) : new Vector2(5.8f, 2.2f);
            if (count >= 4)
            {
                switch (index)
                {
                    case 0: return new Vector2(4.0f, -3.4f);
                    case 1: return new Vector2(5.1f, -1.0f);
                    case 2: return new Vector2(6.0f, 1.4f);
                    default: return new Vector2(6.9f, 3.8f);
                }
            }
            switch (index)
            {
                case 0: return new Vector2(4.2f, -2.8f);
                case 1: return new Vector2(5.4f, 0.2f);
                default: return new Vector2(6.4f, 3.2f);
            }
        }

        /// <summary>Leader keeps the stage look. Each further foe steps hat/chest and, from the third, a different illustration.</summary>
        static CowboyLook VolleyLook(CowboyLook leader, int index)
        {
            var look = leader ?? CowboyLook.Enemy();
            for (int i = 0; i < index; i++)
                look = CowboyLook.Partner(look);
            if (index == 2) look.CharacterId = "dusty_hart";
            else if (index >= 3) look.CharacterId = "rio_vela";
            return look;
        }

        /// <summary>
        /// One human and an even line of foes. Left pistol shoots the even slots (0, then 2),
        /// right pistol the odd slots (1, then 3). PvE reads the stage; a direct scene uses
        /// <see cref="manualSyncOpponents"/>.
        /// </summary>
        List<Duelist> BuildSync(bool pve)
        {
            int n = SyncRules.FoeCount(pve ? Campaign.CurrentStage.Opponents : manualSyncOpponents);
            string title = pve ? Campaign.CurrentStage.Title : "GUNMAN";
            var leader = pve ? (Campaign.CurrentStage.Look ?? CowboyLook.Enemy()) : CowboyLook.Enemy();
            var list = new List<Duelist>
            {
                MakeDuelist(DuelSide.Bottom, 0, SoloLeft, false, CowboyLook.Player(), "YOU", LeftHalf, Key.S),
            };
            for (int i = 0; i < n; i++)
            {
                list.Add(MakeDuelist(
                    DuelSide.Top, i, VolleySpot(i, n), true,
                    VolleyLook(leader, i), $"{title} {i + 1}", RightHalf, Key.None));
            }
            return list;
        }

        List<Duelist> BuildPve()
        {
            string opponent = Campaign.CurrentStage.Title;
            var oppLook = Campaign.CurrentStage.Look ?? CowboyLook.Enemy();
            var partnerLook = CowboyLook.Partner(oppLook);

            // Two players share the LEFT; two bots (the stage opponents) hold the RIGHT.
            if (MatchSettings.Players == PvPPlayers.TwoPlayers)
            {
                return new List<Duelist>
                {
                    MakeDuelist(DuelSide.Bottom, 0, PairLeftA,  false, CowboyLook.Player(),  "P1",             LeftUpper, Key.A),
                    MakeDuelist(DuelSide.Bottom, 1, PairLeftB,  false, CowboyLook.Player2(), "P2",             LeftLower, Key.D),
                    MakeDuelist(DuelSide.Top,    0, PairRightA, true,  oppLook,              opponent + " 1", RightHalf, Key.None),
                    MakeDuelist(DuelSide.Top,    1, PairRightB, true,  partnerLook,         opponent + " 2", RightHalf, Key.None),
                };
            }

            // Solo: you (left) vs one bot opponent (right).
            return new List<Duelist>
            {
                MakeDuelist(DuelSide.Bottom, 0, SoloLeft,  false, CowboyLook.Player(), "YOU",    LeftHalf,  Key.S),
                MakeDuelist(DuelSide.Top,    0, SoloRight, true,  oppLook,             opponent, RightHalf, Key.None),
            };
        }

        List<Duelist> BuildPvp()
        {
            bool botBottom, botTop;
            if (useMatchSettings)
            {
                botBottom = false;
                botTop = MatchSettings.Players == PvPPlayers.OnePlayer;
            }
            else
            {
                botBottom = bottomIsBot;
                botTop = topIsBot;
            }

            return new List<Duelist>
            {
                MakeDuelist(DuelSide.Bottom, 0, SoloLeft,  botBottom, botBottom ? CowboyLook.Enemy() : CowboyLook.Player(),  botBottom ? "BOT" : "PLAYER 1", LeftHalf,  Key.S),
                MakeDuelist(DuelSide.Top,    0, SoloRight, botTop,    botTop    ? CowboyLook.Enemy() : CowboyLook.Player2(), botTop    ? "BOT" : "PLAYER 2", RightHalf, Key.W),
            };
        }

        List<Duelist> BuildCoop()
        {
            // Two players share the LEFT (upper/lower); two bots hold the RIGHT.
            var botLook = CowboyLook.Enemy();
            return new List<Duelist>
            {
                MakeDuelist(DuelSide.Bottom, 0, PairLeftA,  false, CowboyLook.Player(),  "P1",    LeftUpper, Key.A),
                MakeDuelist(DuelSide.Bottom, 1, PairLeftB,  false, CowboyLook.Player2(), "P2",    LeftLower, Key.D),
                MakeDuelist(DuelSide.Top,    0, PairRightA, true,  botLook,              "BOT 1", RightHalf, Key.None),
                MakeDuelist(DuelSide.Top,    1, PairRightB, true,  CowboyLook.Partner(botLook), "BOT 2", RightHalf, Key.None),
            };
        }

        static Sprite PortraitOf(List<Duelist> list, DuelSide side)
        {
            foreach (var d in list)
                if (d.Side == side) return d.View.IdlePortrait;
            return null;
        }

        Duelist MakeDuelist(DuelSide side, int lane, Vector2 pos, bool isBot, CowboyLook look, string label, Rect zone, Key key)
        {
            var go = new GameObject($"{side}_{lane}_Cowboy");
            go.transform.position = new Vector3(pos.x, pos.y, 0f);
            go.transform.localScale = new Vector3(1.8f, 1.8f, 1f);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 10;

            var weapon = isBot ? Weapons.Default : Weapons.Selected;

            var view = go.AddComponent<DuelistView>();
            view.Setup(sr, look, rightSide: side == DuelSide.Top);
            view.SetWeapon(weapon); // skeletal rig mounts the gun in-hand; no-op for other modes

            IDuelInput input = isBot
                ? (IDuelInput)new BotDuelInput(botConfig)
                : new HumanDuelInput(zone, key);

            return new Duelist
            {
                Side = side,
                Lane = lane,
                HomeLane = lane,
                Kind = isBot ? DuelistKind.Bot : DuelistKind.Human,
                Input = input,
                View = view,
                Label = label,
                Weapon = weapon,
            };
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
            cam.orthographicSize = 4.4f; // landscape framing — gunslingers stand at x≈±5 across the street
            cam.transform.position = new Vector3(0f, 0f, -10f);
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = _arena != null ? _arena.CameraFill : new Color(0.85f, 0.72f, 0.45f);

            _shake = cam.GetComponent<CameraShake>();
            if (_shake == null) _shake = cam.gameObject.AddComponent<CameraShake>();

            AppInit.EnsureAudioListener(cam); // without a listener the duel is silent on device
        }

        void SetupEventSystem()
        {
            if (FindFirstObjectByType<EventSystem>() != null) return;
            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<InputSystemUIInputModule>().AssignDefaultActions();
        }

        static DuelConfig RuntimeDuelConfig()
        {
            if (_runtimeDuel == null)
            {
                _runtimeDuel = ScriptableObject.CreateInstance<DuelConfig>();
                _runtimeDuel.hideFlags = HideFlags.HideAndDontSave;
            }
            return _runtimeDuel;
        }

        static BotConfig RuntimeBotConfig()
        {
            if (_runtimeBot == null)
            {
                _runtimeBot = ScriptableObject.CreateInstance<BotConfig>();
                _runtimeBot.hideFlags = HideFlags.HideAndDontSave;
            }
            return _runtimeBot;
        }
    }
}
