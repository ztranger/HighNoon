using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace HighNoon
{
    /// <summary>
    /// A living, world-space backdrop for the main menu: a real arena, the player's
    /// selected gunslinger idling on the left, a rival idling on the right across the
    /// street, and a tumbleweed that rolls through now and then. It reuses the duel's
    /// own systems — <see cref="BackgroundBuilder"/>, <see cref="DuelistView"/>,
    /// <see cref="Tumbleweed"/> — so the menu shows the game, not a flat colour.
    ///
    /// It renders in world space BEHIND the overlay UI (a ScreenSpaceOverlay canvas
    /// always draws on top of what the camera renders), so the menu buttons sit over it.
    /// The menu scene ships no Global Light2D (its UI needs none), so this creates one at
    /// runtime — otherwise the lit sprites render black, exactly as in the duel scene.
    ///
    /// The Home char / arena pickers drive it live: <see cref="RefreshPlayer"/> swaps the
    /// hero, <see cref="RefreshArena"/> rebuilds the location.
    /// </summary>
    public sealed class MenuDiorama : MonoBehaviour
    {
        // Stand-off positions (world x). The two figures frame the UI: hero under the
        // char picker on the left, rival clear of the PvP/PvE buttons on the right.
        // Tune on device — world x maps to a different screen fraction per aspect ratio.
        const float PlayerX = -5.4f;
        const float RivalX  =  6.0f;

        GameObject _bgRoot;
        DuelistView _playerView, _rivalView;
        ArenaDef _arena;
        bool _running;
        float _nextWeed;

        public static MenuDiorama Create()
        {
            var go = new GameObject("MenuDiorama");
            var d = go.AddComponent<MenuDiorama>();
            d.Build();
            return d;
        }

        void Build()
        {
            EnsureLight();

            var cam = Camera.main;
            if (cam != null) cam.orthographicSize = 4.4f; // match the duel's landscape framing

            _arena = Arenas.Pick(MatchSettings.MenuArena);
            BuildArena();

            _playerView = BuildCowboy("Hero",  CowboyLook.Player(), PlayerX, rightSide: false);
            _rivalView  = BuildCowboy("Rival", CowboyLook.Enemy(),  RivalX,  rightSide: true);

            _running = true;
            _nextWeed = Random.Range(4f, 8f);
        }

        /// <summary>Global 2D light so the world sprites are lit (the menu scene has none).</summary>
        void EnsureLight()
        {
            if (FindFirstObjectByType<Light2D>() != null) return;
            var go = new GameObject("Global Light 2D");
            go.transform.SetParent(transform, false);
            go.SetActive(false); // configure before OnEnable so it registers already typed Global
            var light = go.AddComponent<Light2D>();
            light.lightType = Light2D.LightType.Global;
            light.intensity = 1f;
            light.color = Color.white;
            go.SetActive(true);
        }

        void BuildArena()
        {
            if (_bgRoot != null) Destroy(_bgRoot);
            _bgRoot = BackgroundBuilder.Build(_arena);
            _bgRoot.transform.SetParent(transform, false);
            var cam = Camera.main;
            if (cam != null) cam.backgroundColor = _arena.CameraFill;
        }

        DuelistView BuildCowboy(string name, CowboyLook look, float x, bool rightSide)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            go.transform.position = new Vector3(x, 0f, 0f);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 10;

            var view = go.AddComponent<DuelistView>();
            view.Setup(sr, look, rightSide);
            view.SetWeapon(Weapons.Selected); // skeletal rig mounts the gun; no-op for sheet/static art
            view.Stance();                    // idle loop (sheet) / breathing (static/rig)
            return view;
        }

        /// <summary>Rebuild the backdrop from the current menu arena pick (called by the Home picker).</summary>
        public void RefreshArena()
        {
            _arena = Arenas.Pick(MatchSettings.MenuArena);
            BuildArena();
        }

        /// <summary>Swap the hero to the currently selected character (called by the Home picker).</summary>
        public void RefreshPlayer()
        {
            if (_playerView != null) Destroy(_playerView.gameObject);
            _playerView = BuildCowboy("Hero", CowboyLook.Player(), PlayerX, rightSide: false);
        }

        /// <summary>Only spawn tumbleweeds while Home is the visible screen.</summary>
        public void SetRunning(bool onHome) => _running = onHome;

        void Update()
        {
            if (!_running) return;
            _nextWeed -= Time.deltaTime;
            if (_nextWeed <= 0f)
            {
                Tumbleweed.Spawn();
                _nextWeed = Random.Range(8f, 15f);
            }
        }
    }
}
