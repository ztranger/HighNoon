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

        [Header("Configs (optional — runtime defaults created if empty)")]
        public DuelConfig duelConfig;
        public BotConfig botConfig;

        CameraShake _shake;
        ArenaDef _arena;

        static readonly Color ColP1 = new Color(0.82f, 0.62f, 0.36f);
        static readonly Color ColP2 = new Color(0.45f, 0.62f, 0.85f);
        static readonly Color ColEnemy = new Color(0.78f, 0.32f, 0.30f);

        static readonly Rect BottomHalf = new Rect(0f, 0f, 1f, 0.5f);
        static readonly Rect TopHalf = new Rect(0f, 0.5f, 1f, 0.5f);
        static readonly Rect BottomLeft = new Rect(0f, 0f, 0.5f, 0.5f);
        static readonly Rect BottomRight = new Rect(0.5f, 0f, 0.5f, 0.5f);
        static readonly Rect TopLeft = new Rect(0f, 0.5f, 0.5f, 0.5f);
        static readonly Rect TopRight = new Rect(0.5f, 0.5f, 0.5f, 0.5f);

        void Start()
        {
            // Keep ticking while the window is unfocused (also in PlayerSettings for builds).
            Application.runInBackground = true;
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

            if (duelConfig == null) duelConfig = ScriptableObject.CreateInstance<DuelConfig>();
            if (useMatchSettings)
            {
                botConfig = ScriptableObject.CreateInstance<BotConfig>();
                MatchSettings.ApplyDifficulty(botConfig);
            }
            else if (botConfig == null)
            {
                botConfig = ScriptableObject.CreateInstance<BotConfig>();
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
                dialog.Play(stage.Intro,
                    "YOU", ColP1, CowboyArt.Build(ColP1).Idle[0],
                    stage.Title, ColEnemy, CowboyArt.Build(ColEnemy).Idle[0],
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

            // Two players share the bottom; two bots (the stage opponents) hold the top.
            if (MatchSettings.Players == PvPPlayers.TwoPlayers)
            {
                return new List<Duelist>
                {
                    MakeDuelist(DuelSide.Bottom, 0, -2f, false, ColP1,    "P1",     BottomLeft,  Key.A),
                    MakeDuelist(DuelSide.Bottom, 1,  2f, false, ColP2,    "P2",     BottomRight, Key.D),
                    MakeDuelist(DuelSide.Top,    0, -2f, true,  ColEnemy, opponent, TopLeft,     Key.None),
                    MakeDuelist(DuelSide.Top,    1,  2f, true,  ColEnemy, opponent, TopRight,    Key.None),
                };
            }

            // Solo: you (bottom) vs one bot opponent (top).
            return new List<Duelist>
            {
                MakeDuelist(DuelSide.Bottom, 0, 0f, false, ColP1,    "YOU",    BottomHalf, Key.S),
                MakeDuelist(DuelSide.Top,    0, 0f, true,  ColEnemy, opponent, TopHalf,    Key.None),
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
                MakeDuelist(DuelSide.Bottom, 0, 0f, botBottom, ColP1,    botBottom ? "BOT" : "PLAYER 1", BottomHalf, Key.S),
                MakeDuelist(DuelSide.Top,    0, 0f, botTop,    ColEnemy, botTop    ? "BOT" : "PLAYER 2", TopHalf,    Key.W),
            };
        }

        List<Duelist> BuildCoop()
        {
            // Two players share the bottom (left/right); two bots hold the top.
            return new List<Duelist>
            {
                MakeDuelist(DuelSide.Bottom, 0, -2f, false, ColP1,    "P1",  BottomLeft,  Key.A),
                MakeDuelist(DuelSide.Bottom, 1,  2f, false, ColP2,    "P2",  BottomRight, Key.D),
                MakeDuelist(DuelSide.Top,    0, -2f, true,  ColEnemy, "BOT", TopLeft,     Key.None),
                MakeDuelist(DuelSide.Top,    1,  2f, true,  ColEnemy, "BOT", TopRight,    Key.None),
            };
        }

        Duelist MakeDuelist(DuelSide side, int lane, float x, bool isBot, Color color, string label, Rect zone, Key key)
        {
            var go = new GameObject($"{side}_{lane}_Cowboy");
            float y = side == DuelSide.Bottom ? -2.6f : 2.6f;
            go.transform.position = new Vector3(x, y, 0f);
            go.transform.localScale = new Vector3(1.8f, 1.8f, 1f);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 10;

            var view = go.AddComponent<DuelistView>();
            view.Setup(sr, color, faceDown: side == DuelSide.Top);

            IDuelInput input = isBot
                ? (IDuelInput)new BotDuelInput(botConfig)
                : new HumanDuelInput(zone, key);

            return new Duelist
            {
                Side = side,
                Lane = lane,
                Kind = isBot ? DuelistKind.Bot : DuelistKind.Human,
                Input = input,
                View = view,
                Label = label,
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
            cam.orthographicSize = 6f;
            cam.transform.position = new Vector3(0f, 0f, -10f);
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = _arena != null ? _arena.CameraFill : new Color(0.85f, 0.72f, 0.45f);

            _shake = cam.GetComponent<CameraShake>();
            if (_shake == null) _shake = cam.gameObject.AddComponent<CameraShake>();
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
