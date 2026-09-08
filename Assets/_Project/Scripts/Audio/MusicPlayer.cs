using UnityEngine;
using UnityEngine.SceneManagement;

namespace HighNoon
{
    /// <summary>
    /// Persistent background audio with two layers:
    ///  • MUSIC — a shuffled playlist of <c>Resources/Audio/Ambient</c>, played in the menu / map.
    ///  • AMBIENCE — a looping wind bed from <c>Resources/Audio/Wind</c>, played during a duel /
    ///    tutorial (that's where the wind belongs — under the gunfight), while the music pauses.
    /// On the Story screen both are silent so the victory / defeat sting reads clean. Volume
    /// follows <see cref="GameSettings.MusicEnabled"/> + <see cref="GameSettings.MasterVolume"/>
    /// live. Created once via <see cref="Ensure"/> from <see cref="AppInit.Apply"/>.
    /// </summary>
    public class MusicPlayer : MonoBehaviour
    {
        const float MusicVolume = 0.45f;
        const float WindVolume = 0.35f;

        static MusicPlayer _instance;
        AudioSource _music, _wind;
        AudioClip[] _tracks, _winds;
        int _lastTrack = -1, _lastWind = -1;
        bool _wantMusic = true;
        bool _musicPaused;

        public static void Ensure()
        {
            if (_instance != null) return;
            var go = new GameObject("~Music");
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<MusicPlayer>();
        }

        void Awake()
        {
            _music = gameObject.AddComponent<AudioSource>();
            _music.playOnAwake = false; _music.loop = false; _music.volume = 0f;

            _wind = gameObject.AddComponent<AudioSource>();
            _wind.playOnAwake = false; _wind.loop = true; _wind.volume = 0f;

            _tracks = Resources.LoadAll<AudioClip>("Audio/Ambient");
            _winds = Resources.LoadAll<AudioClip>("Audio/Wind");

            SceneManager.sceneLoaded += OnSceneLoaded;
            ApplyScene(SceneManager.GetActiveScene().name);
            if (_wantMusic) NextTrack();
        }

        void OnDestroy() { SceneManager.sceneLoaded -= OnSceneLoaded; }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode) => ApplyScene(scene.name);

        void ApplyScene(string sceneName)
        {
            bool duel = sceneName == "Duel" || sceneName == "Tutorial";
            bool story = sceneName == "Story";
            _wantMusic = !duel && !story;

            if (duel && _winds != null && _winds.Length > 0)
            {
                _wind.clip = PickWind();
                if (!_wind.isPlaying) { _wind.volume = 0f; _wind.Play(); }
            }
            else
            {
                _wind.Stop();
            }
        }

        AudioClip PickWind()
        {
            if (_winds.Length == 1) return _winds[0];
            int i; do { i = Random.Range(0, _winds.Length); } while (i == _lastWind);
            _lastWind = i; return _winds[i];
        }

        void NextTrack()
        {
            if (_tracks == null || _tracks.Length == 0) return;
            int i = 0;
            if (_tracks.Length > 1) { do { i = Random.Range(0, _tracks.Length); } while (i == _lastTrack); }
            _lastTrack = i;
            _music.clip = _tracks[i];
            _music.Play();
        }

        void Update()
        {
            float master = GameSettings.MasterVolume;
            bool on = GameSettings.MusicEnabled;
            float step = Time.unscaledDeltaTime * 1.5f;

            // Music layer (menu / map): pause elsewhere so it resumes seamlessly.
            _music.volume = Mathf.MoveTowards(_music.volume, (_wantMusic && on) ? MusicVolume * master : 0f, step);
            if (!_wantMusic)
            {
                if (_music.isPlaying) { _music.Pause(); _musicPaused = true; }
            }
            else if (_musicPaused) { _music.UnPause(); _musicPaused = false; }
            else if (_tracks != null && _tracks.Length > 0 && !_music.isPlaying) NextTrack();

            // Ambience layer (duel wind): only audible while it's playing (set per scene).
            _wind.volume = Mathf.MoveTowards(_wind.volume, (_wind.isPlaying && on) ? WindVolume * master : 0f, step);
        }
    }
}
