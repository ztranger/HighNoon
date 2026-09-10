using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

namespace HighNoon
{
    /// <summary>
    /// Standalone preview scene (Scenes/AnimTest.unity): pick a cowboy character + weapon and play
    /// each rig state — idle / walk-in / draw / death — with buttons. Not part of the duel flow; a
    /// harness for iterating on the cut-out rig and art. Needs a Global Light2D (in the scene) like the duel.
    /// </summary>
    public class AnimTestBootstrap : MonoBehaviour
    {
        DuelistView _view;
        GameObject _cowboy;
        int _charIndex, _weaponIndex;
        bool _rightSide;

        Canvas _canvas;
        Font _font;
        Text _charLabel, _weaponLabel, _modeLabel;

        void Start()
        {
            AppInit.Apply();
            Screen.orientation = ScreenOrientation.LandscapeLeft; // the rig is a landscape side-view
            _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            SetupCamera();
            SetupEventSystem();
            BuildUI();
            Spawn();
        }

        void SetupCamera()
        {
            var cam = Camera.main;
            if (cam == null)
            {
                var go = new GameObject("Main Camera") { tag = "MainCamera" };
                cam = go.AddComponent<Camera>();
            }
            cam.orthographic = true;
            cam.orthographicSize = 4.4f;
            cam.transform.position = new Vector3(0f, 0f, -10f);
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.16f, 0.15f, 0.19f);
            AppInit.EnsureAudioListener(cam);
        }

        void SetupEventSystem()
        {
            if (FindFirstObjectByType<EventSystem>() != null) return;
            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<InputSystemUIInputModule>().AssignDefaultActions();
        }

        void Spawn()
        {
            if (_cowboy != null) Destroy(_cowboy);
            var c = CowboyCatalog.Roster[_charIndex];
            _cowboy = new GameObject("PreviewCowboy");
            _cowboy.transform.position = Vector3.zero;
            _cowboy.transform.localScale = Vector3.one * 1.8f; // procedural mode keeps this; rig/sheet reset to 1
            var sr = _cowboy.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 10;
            _view = _cowboy.AddComponent<DuelistView>();
            _view.Setup(sr, new CowboyLook { CharacterId = c.Id }, _rightSide);
            _view.SetWeapon(Weapons.Get(_weaponIndex));
            _view.Stance();
            UpdateLabels();
        }

        void UpdateLabels()
        {
            var c = CowboyCatalog.Roster[_charIndex];
            if (_charLabel) _charLabel.text = $"CHARACTER:  {c.Id}";
            if (_weaponLabel) _weaponLabel.text = $"WEAPON:  {Weapons.Get(_weaponIndex).Name}";
            if (_modeLabel) _modeLabel.text = _rightSide ? "FACING: LEFT (mirrored)" : "FACING: RIGHT";
        }

        void CycleChar(int d)
        {
            int n = CowboyCatalog.Roster.Length;
            _charIndex = ((_charIndex + d) % n + n) % n;
            Spawn();
        }

        void CycleWeapon(int d)
        {
            int n = Weapons.Count;
            _weaponIndex = ((_weaponIndex + d) % n + n) % n;
            if (_view != null) _view.SetWeapon(Weapons.Get(_weaponIndex));
            UpdateLabels();
        }

        void Flip() { _rightSide = !_rightSide; Spawn(); }

        // ---------- UI ----------

        void BuildUI()
        {
            var canGo = new GameObject("UI");
            _canvas = canGo.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            canGo.AddComponent<GraphicRaycaster>();

            var top = new Vector2(0.5f, 1f);

            _charLabel = MakeText("charLabel", top, new Vector2(0f, -46f), new Vector2(560f, 50f), 34, TextAnchor.MiddleCenter);
            MakeButton("<", top, new Vector2(-330f, -46f), new Vector2(70f, 50f), () => CycleChar(-1));
            MakeButton(">", top, new Vector2(330f, -46f), new Vector2(70f, 50f), () => CycleChar(1));

            _weaponLabel = MakeText("weaponLabel", top, new Vector2(0f, -104f), new Vector2(560f, 44f), 28, TextAnchor.MiddleCenter);
            MakeButton("<", top, new Vector2(-330f, -104f), new Vector2(70f, 44f), () => CycleWeapon(-1));
            MakeButton(">", top, new Vector2(330f, -104f), new Vector2(70f, 44f), () => CycleWeapon(1));

            _modeLabel = MakeText("modeLabel", top, new Vector2(0f, -152f), new Vector2(560f, 36f), 22, TextAnchor.MiddleCenter);

            var actions = new (string, UnityAction)[]
            {
                ("WALK-IN", () => { if (_view) _view.BeginWalkIn(1.2f); }),
                ("IDLE",    () => { if (_view) _view.Stance(); }),
                ("DRAW",    () => { if (_view) _view.PlayShoot(); }),
                ("DEATH",   () => { if (_view) _view.PlayDeath(); }),
                ("RESET",   () => Spawn()),
                ("FLIP",    () => Flip()),
            };
            const float bw = 210f, gap = 16f, h = 72f;
            float total = actions.Length * bw + (actions.Length - 1) * gap;
            float x0 = -total / 2f + bw / 2f;
            var bottom = new Vector2(0.5f, 0f);
            for (int i = 0; i < actions.Length; i++)
                MakeButton(actions[i].Item1, bottom, new Vector2(x0 + i * (bw + gap), 66f), new Vector2(bw, h), actions[i].Item2);

            UpdateLabels();
        }

        Text MakeText(string name, Vector2 anchor, Vector2 pos, Vector2 size, int fontSize, TextAnchor align)
        {
            var go = new GameObject(name);
            go.transform.SetParent(_canvas.transform, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = anchor;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = size;
            rt.anchoredPosition = pos;
            var t = go.AddComponent<Text>();
            t.font = _font;
            t.fontSize = fontSize;
            t.color = Color.white;
            t.alignment = align;
            t.raycastTarget = false;
            t.text = name;
            return t;
        }

        Button MakeButton(string label, Vector2 anchor, Vector2 pos, Vector2 size, UnityAction onClick)
        {
            var go = new GameObject("btn_" + label);
            go.transform.SetParent(_canvas.transform, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = anchor;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = size;
            rt.anchoredPosition = pos;

            var img = go.AddComponent<Image>();
            img.color = new Color(0.24f, 0.21f, 0.17f, 0.96f);
            var btn = go.AddComponent<Button>();
            btn.targetGraphic = img;
            btn.onClick.AddListener(onClick);

            var txtGo = new GameObject("txt");
            txtGo.transform.SetParent(go.transform, false);
            var trt = txtGo.AddComponent<RectTransform>();
            trt.anchorMin = Vector2.zero;
            trt.anchorMax = Vector2.one;
            trt.offsetMin = trt.offsetMax = Vector2.zero;
            var t = txtGo.AddComponent<Text>();
            t.font = _font;
            t.fontSize = 26;
            t.color = new Color(0.98f, 0.92f, 0.78f);
            t.alignment = TextAnchor.MiddleCenter;
            t.raycastTarget = false;
            t.text = label;
            return btn;
        }
    }
}
