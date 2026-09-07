using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem.UI;

namespace HighNoon
{
    /// <summary>
    /// Builds the main menu in code (camera, EventSystem, UI). Lets the player pick
    /// mode / player count / bot difficulty, writes them to <see cref="MatchSettings"/>,
    /// and loads the Duel scene on PLAY.
    /// </summary>
    public class MainMenuBootstrap : MonoBehaviour
    {
        static readonly Color Selected = new Color(0.88f, 0.66f, 0.22f);
        static readonly Color Normal   = new Color(0.48f, 0.38f, 0.24f);
        static readonly Color Disabled = new Color(0.34f, 0.31f, 0.29f);
        static readonly Color Gold     = new Color(0.92f, 0.78f, 0.35f);

        Font _font;
        Image _pvpImg, _coopImg, _pveImg;
        Image _p1Img, _p2Img, _easyImg, _normalImg, _hardImg;

        void Start()
        {
            Application.runInBackground = true;
            _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (_font == null) _font = Resources.GetBuiltinResource<Font>("Arial.ttf");

            SetupCamera();
            SetupEventSystem();
            BuildUI();
            RefreshHighlights();
        }

        void BuildUI()
        {
            var canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;
            gameObject.AddComponent<GraphicRaycaster>();

            // Full-screen background.
            var bg = NewRect("BG", transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            bg.gameObject.AddComponent<Image>().color = new Color(0.16f, 0.11f, 0.09f);

            MakeText(transform, "Title", "HIGH NOON", 150, new Vector2(0.5f, 0.88f), Vector2.zero, 1000, 220, Gold, FontStyle.Bold);
            MakeText(transform, "Subtitle", "— PvP Duel —", 54, new Vector2(0.5f, 0.80f), Vector2.zero, 900, 100, new Color(0.8f, 0.7f, 0.5f), FontStyle.Normal);

            // Mode row (PvP + Coop active; PvE later).
            MakeText(transform, "ModeLabel", "MODE", 44, new Vector2(0.5f, 0.71f), Vector2.zero, 900, 70, new Color(0.7f, 0.62f, 0.5f), FontStyle.Normal);
            var pvp = MakeButton(transform, "ModePvP", "PvP", new Vector2(0.5f, 0.645f), new Vector2(-320, 0), 300, 110, 44, Normal, out _pvpImg, true);
            var coop = MakeButton(transform, "ModeCoop", "COOP", new Vector2(0.5f, 0.645f), new Vector2(0, 0), 300, 110, 40, Normal, out _coopImg, true);
            var pve = MakeButton(transform, "ModePvE", "PvE", new Vector2(0.5f, 0.645f), new Vector2(320, 0), 300, 110, 44, Normal, out _pveImg, true);
            pvp.onClick.AddListener(() => { MatchSettings.Mode = GameMode.PvP; RefreshHighlights(); });
            coop.onClick.AddListener(() => { MatchSettings.Mode = GameMode.Coop; RefreshHighlights(); });
            pve.onClick.AddListener(() => { MatchSettings.Mode = GameMode.PvE; RefreshHighlights(); });

            // Players.
            MakeText(transform, "PlayersLabel", "PLAYERS", 44, new Vector2(0.5f, 0.55f), Vector2.zero, 900, 70, new Color(0.7f, 0.62f, 0.5f), FontStyle.Normal);
            var p1 = MakeButton(transform, "OnePlayer", "1 PLAYER", new Vector2(0.5f, 0.485f), new Vector2(-260, 0), 480, 120, 44, Normal, out _p1Img, true);
            var p2 = MakeButton(transform, "TwoPlayers", "2 PLAYERS", new Vector2(0.5f, 0.485f), new Vector2(260, 0), 480, 120, 44, Normal, out _p2Img, true);
            p1.onClick.AddListener(() => { MatchSettings.Players = PvPPlayers.OnePlayer; RefreshHighlights(); });
            p2.onClick.AddListener(() => { MatchSettings.Players = PvPPlayers.TwoPlayers; RefreshHighlights(); });

            // Bot difficulty.
            MakeText(transform, "DiffLabel", "BOT DIFFICULTY", 44, new Vector2(0.5f, 0.39f), Vector2.zero, 900, 70, new Color(0.7f, 0.62f, 0.5f), FontStyle.Normal);
            var easy = MakeButton(transform, "Easy", "EASY", new Vector2(0.5f, 0.325f), new Vector2(-330, 0), 300, 120, 40, Normal, out _easyImg, true);
            var norm = MakeButton(transform, "Normal", "NORMAL", new Vector2(0.5f, 0.325f), new Vector2(0, 0), 300, 120, 40, Normal, out _normalImg, true);
            var hard = MakeButton(transform, "Hard", "HARD", new Vector2(0.5f, 0.325f), new Vector2(330, 0), 300, 120, 40, Normal, out _hardImg, true);
            easy.onClick.AddListener(() => { MatchSettings.BotDifficulty = Difficulty.Easy; RefreshHighlights(); });
            norm.onClick.AddListener(() => { MatchSettings.BotDifficulty = Difficulty.Normal; RefreshHighlights(); });
            hard.onClick.AddListener(() => { MatchSettings.BotDifficulty = Difficulty.Hard; RefreshHighlights(); });

            // Play.
            var play = MakeButton(transform, "Play", "PLAY", new Vector2(0.5f, 0.15f), Vector2.zero, 620, 170, 72, Gold, out _, true);
            play.onClick.AddListener(() =>
            {
                if (MatchSettings.Mode == GameMode.PvE)
                {
                    Campaign.StartRun();
                    Story.Kind = StoryKind.ChapterIntro;
                    SceneManager.LoadScene("Story");
                }
                else
                {
                    SceneManager.LoadScene("Duel");
                }
            });
        }

        void RefreshHighlights()
        {
            var mode = MatchSettings.Mode;
            _pvpImg.color  = mode == GameMode.PvP  ? Selected : Normal;
            _coopImg.color = mode == GameMode.Coop ? Selected : Normal;
            _pveImg.color  = mode == GameMode.PvE  ? Selected : Normal;

            _p1Img.color = MatchSettings.Players == PvPPlayers.OnePlayer ? Selected : Normal;
            _p2Img.color = MatchSettings.Players == PvPPlayers.TwoPlayers ? Selected : Normal;

            _easyImg.color   = MatchSettings.BotDifficulty == Difficulty.Easy   ? Selected : Normal;
            _normalImg.color = MatchSettings.BotDifficulty == Difficulty.Normal ? Selected : Normal;
            _hardImg.color   = MatchSettings.BotDifficulty == Difficulty.Hard   ? Selected : Normal;

            // Player count applies to PvP and PvE (Coop is always 2 players).
            float pa = mode != GameMode.Coop ? 1f : 0.4f;
            SetAlpha(_p1Img, pa); SetAlpha(_p2Img, pa);

            // Difficulty applies to menu-chosen bots (PvP single player or Coop). PvE sets it per stage.
            bool botUsed = (mode == GameMode.PvP && MatchSettings.Players == PvPPlayers.OnePlayer) || mode == GameMode.Coop;
            float da = botUsed ? 1f : 0.4f;
            SetAlpha(_easyImg, da); SetAlpha(_normalImg, da); SetAlpha(_hardImg, da);
        }

        static void SetAlpha(Image img, float a)
        {
            var c = img.color; c.a = a; img.color = c;
        }

        // ---- UI builders ----

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
