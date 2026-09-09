using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

namespace HighNoon
{
    /// <summary>
    /// Landscape menu shell: camera, canvas, and a navigator over five screen classes
    /// (<see cref="HomeScreen"/>, <see cref="StatsScreen"/>, <see cref="GunsScreen"/>,
    /// <see cref="SetupScreen"/>, <see cref="SettingsScreen"/>). Home is the default.
    /// </summary>
    public class MainMenuBootstrap : MonoBehaviour
    {
        HomeScreen _home;
        StatsScreen _stats;
        GunsScreen _guns;
        SetupScreen _setup;
        SettingsScreen _settings;

        void Start()
        {
            AppInit.Apply();
            if (!GameSettings.TutorialDone) { DuelFlow.Tutorial(); return; }

            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");

            Campaign.LoadSavedRun();
            SetupCamera();
            SetupEventSystem();
            BuildUI(new UiBuild(font));
            Show(MenuScreenId.Home);
        }

        void BuildUI(UiBuild ui)
        {
            UiCanvas.Overlay(gameObject);

            var bg = ui.Stretch("BG", transform);
            bg.gameObject.AddComponent<Image>().color = new Color(0.16f, 0.11f, 0.09f);

            _home = new HomeScreen();
            _stats = new StatsScreen();
            _guns = new GunsScreen();
            _setup = new SetupScreen();
            _settings = new SettingsScreen();

            _home.Build(ui.Stretch("Home", transform), ui, Show);
            _stats.Build(ui.Stretch("Stats", transform), ui, BackHome);
            _guns.Build(ui.Stretch("Guns", transform), ui, BackHome, () => _setup.Refresh());
            _setup.Build(ui.Stretch("Setup", transform), ui, BackHome, Show, () => _setup.Refresh());
            _settings.Build(ui.Stretch("Settings", transform), ui, BackHome);

            _guns.Refresh();
            _setup.Refresh();
        }

        void BackHome() => Show(MenuScreenId.Home);

        void Show(MenuScreenId id)
        {
            if (_home.Root != null) _home.Root.gameObject.SetActive(id == MenuScreenId.Home);
            if (_stats.Root != null) _stats.Root.gameObject.SetActive(id == MenuScreenId.Stats);
            if (_guns.Root != null) _guns.Root.gameObject.SetActive(id == MenuScreenId.Guns);
            if (_setup.Root != null) _setup.Root.gameObject.SetActive(id == MenuScreenId.Setup);
            if (_settings.Root != null) _settings.Root.gameObject.SetActive(id == MenuScreenId.Settings);
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
