using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

namespace HighNoon
{
    /// <summary>
    /// Landscape menu shell: camera, canvas, and a navigator over the menu screens.
    /// Home is the default. PvE opens <see cref="CampaignSelectScreen"/>.
    /// </summary>
    public class MainMenuBootstrap : MonoBehaviour
    {
        HomeScreen _home;
        StatsScreen _stats;
        GunsScreen _guns;
        SetupScreen _setup;
        SettingsScreen _settings;
        CampaignSelectScreen _campaigns;
        MenuDiorama _diorama;
        GameObject _bg;

        void Start()
        {
            AppInit.Apply();
            if (!GameSettings.TutorialDone) { DuelFlow.Tutorial(); return; }

            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");

            Campaign.LoadSavedRun();
            SetupCamera();
            SetupEventSystem();
            _diorama = MenuDiorama.Create(); // living world backdrop behind the UI (needs Camera.main)
            BuildUI(new UiBuild(font));
            Show(MenuScreenId.Home);
        }

        void BuildUI(UiBuild ui)
        {
            UiCanvas.Overlay(gameObject);

            // Opaque backdrop for the non-Home panels; hidden on Home so the diorama shows through.
            var bg = ui.Stretch("BG", transform);
            bg.gameObject.AddComponent<Image>().color = new Color(0.16f, 0.11f, 0.09f);
            _bg = bg.gameObject;

            _home = new HomeScreen();
            _stats = new StatsScreen();
            _guns = new GunsScreen();
            _setup = new SetupScreen();
            _settings = new SettingsScreen();
            _campaigns = new CampaignSelectScreen();

            _home.Build(ui.Stretch("Home", transform), ui, Show, _diorama);
            _stats.Build(ui.Stretch("Stats", transform), ui, BackHome);
            _guns.Build(ui.Stretch("Guns", transform), ui, BackHome, () => _setup.Refresh());
            _setup.Build(ui.Stretch("Setup", transform), ui, BackHome, Show, () => _setup.Refresh());
            _settings.Build(ui.Stretch("Settings", transform), ui, BackHome);
            _campaigns.Build(ui.Stretch("Campaigns", transform), ui, BackHome);

            _guns.Refresh();
            _setup.Refresh();
        }

        void BackHome() => Show(MenuScreenId.Home);

        void Show(MenuScreenId id)
        {
            bool home = id == MenuScreenId.Home;
            if (_bg != null) _bg.SetActive(!home);          // reveal the diorama on Home, cover it elsewhere
            if (_diorama != null) _diorama.SetRunning(home); // tumbleweeds only while Home is visible

            if (_home.Root != null)
            {
                bool on = home;
                _home.Root.gameObject.SetActive(on);
                if (on) _home.Refresh();
            }
            if (_stats.Root != null) _stats.Root.gameObject.SetActive(id == MenuScreenId.Stats);
            if (_guns.Root != null) _guns.Root.gameObject.SetActive(id == MenuScreenId.Guns);
            if (_setup.Root != null)
            {
                bool on = id == MenuScreenId.Setup;
                _setup.Root.gameObject.SetActive(on);
                if (on) _setup.Refresh();
            }
            if (_settings.Root != null) _settings.Root.gameObject.SetActive(id == MenuScreenId.Settings);
            if (_campaigns.Root != null) _campaigns.Root.gameObject.SetActive(id == MenuScreenId.Campaigns);
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
