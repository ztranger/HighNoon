using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.InputSystem.EnhancedTouch;

namespace HighNoon
{
    /// <summary>
    /// Single entry point for the PvP duel scene. Builds the camera, background,
    /// duelists, HUD, audio, and the DuelManager at runtime so the scene needs only
    /// this one component. Refactor to prefabs/inspector as real art arrives.
    /// </summary>
    public class DuelBootstrap : MonoBehaviour
    {
        [Header("Match setup")]
        [Tooltip("Read mode/players/difficulty from the main menu (MatchSettings). Uncheck to use the manual flags below when opening this scene directly.")]
        public bool useMatchSettings = true;

        [Tooltip("Single player: leave the top slot as a bot. Used only when 'useMatchSettings' is off.")]
        public bool topIsBot = true;
        public bool bottomIsBot = false;

        [Header("Configs (optional — runtime defaults created if empty)")]
        public DuelConfig duelConfig;
        public BotConfig botConfig;

        void Start()
        {
            // Keep the loop ticking when the editor/app is unfocused (also set in
            // PlayerSettings for builds); without it play mode freezes in background.
            Application.runInBackground = true;
            EnhancedTouchSupport.Enable();

            if (duelConfig == null) duelConfig = ScriptableObject.CreateInstance<DuelConfig>();

            if (useMatchSettings)
            {
                bottomIsBot = false;
                topIsBot = MatchSettings.Players == PvPPlayers.OnePlayer;
                botConfig = ScriptableObject.CreateInstance<BotConfig>();
                MatchSettings.ApplyDifficulty(botConfig);
            }
            else if (botConfig == null)
            {
                botConfig = ScriptableObject.CreateInstance<BotConfig>();
            }

            SetupCamera();
            SetupBackground();
            SetupEventSystem();

            var hud = new GameObject("HUD").AddComponent<DuelHUD>();
            hud.Setup();

            var audio = new GameObject("DuelAudio").AddComponent<DuelAudio>();
            audio.Setup();

            var duelists = new List<Duelist>
            {
                MakeDuelist(DuelSide.Bottom, bottomIsBot, new Color(0.80f, 0.60f, 0.35f), bottomIsBot ? "BOT" : "PLAYER 1"),
                MakeDuelist(DuelSide.Top,    topIsBot,    new Color(0.75f, 0.30f, 0.30f), topIsBot ? "BOT" : "PLAYER 2"),
            };

            var manager = new GameObject("DuelManager").AddComponent<DuelManager>();
            manager.Config = duelConfig;
            manager.Hud = hud;
            manager.Audio = audio;
            manager.Duelists = duelists;
            manager.StartDuel();
        }

        Duelist MakeDuelist(DuelSide side, bool isBot, Color color, string label)
        {
            var go = new GameObject(side + "_Cowboy");
            float y = side == DuelSide.Bottom ? -2.6f : 2.6f;
            go.transform.position = new Vector3(0f, y, 0f);
            go.transform.localScale = new Vector3(1.8f, 1.8f, 1f);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 10;

            var view = go.AddComponent<DuelistView>();
            view.Setup(sr, color, faceDown: side == DuelSide.Top); // Setup builds + assigns the sprite

            IDuelInput input;
            if (isBot)
            {
                input = new BotDuelInput(botConfig);
            }
            else
            {
                Rect zone = side == DuelSide.Bottom
                    ? new Rect(0f, 0f, 1f, 0.5f)
                    : new Rect(0f, 0.5f, 1f, 0.5f);
                var key = side == DuelSide.Bottom
                    ? UnityEngine.InputSystem.Key.S
                    : UnityEngine.InputSystem.Key.W;
                input = new HumanDuelInput(zone, key);
            }

            return new Duelist
            {
                Side = side,
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
            cam.backgroundColor = new Color(0.85f, 0.72f, 0.45f);
        }

        void SetupBackground()
        {
            var bg = new GameObject("Background");
            var sr = bg.AddComponent<SpriteRenderer>();
            sr.sprite = PlaceholderArt.SolidSprite(new Color(0.82f, 0.70f, 0.42f), 4, 4, 1);
            sr.sortingOrder = -100;
            bg.transform.localScale = new Vector3(20f, 26f, 1f);

            var divider = new GameObject("Divider");
            var dsr = divider.AddComponent<SpriteRenderer>();
            dsr.sprite = PlaceholderArt.SolidSprite(new Color(0f, 0f, 0f, 0.18f), 4, 4, 1);
            dsr.sortingOrder = -50;
            divider.transform.localScale = new Vector3(20f, 0.12f, 1f);
        }

        void SetupEventSystem()
        {
            if (FindFirstObjectByType<EventSystem>() != null) return;
            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            var module = es.AddComponent<InputSystemUIInputModule>();
            module.AssignDefaultActions();
        }
    }
}
