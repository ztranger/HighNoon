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
    /// duelists for the chosen mode (PvP = 1 lane, Coop = 2v2 across two lanes).
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
                MatchSettings.ForcedArena = null; // PvP/Coop use random arenas
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
            var duelists = pve ? BuildPve() : coop ? BuildCoop() : BuildPvp();

            var manager = new GameObject("DuelManager").AddComponent<DuelManager>();
            manager.Config = duelConfig;
            manager.Hud = hud;
            manager.Audio = audio;
            manager.Shake = _shake;
            manager.PveMode = pve;
            manager.Type = useMatchSettings ? MatchSettings.Type : manualDuelType;
            manager.Duelists = duelists;
            if (pve)
            {
                string tag = MatchSettings.Players == PvPPlayers.TwoPlayers ? "CO-OP" : "SOLO";
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

            var view = go.AddComponent<DuelistView>();
            view.Setup(sr, look, rightSide: side == DuelSide.Top);

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
                Weapon = isBot ? Weapons.Default : Weapons.Selected,
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
            Screen.orientation = ScreenOrientation.LandscapeLeft; // the duel plays in landscape (menus force portrait back)
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
