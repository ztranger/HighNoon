using UnityEngine;

namespace HighNoon
{
    /// <summary>
    /// Persisted player preferences (PlayerPrefs). Currently audio + haptics toggles and a
    /// master volume; read via the static properties from anywhere. Lazily loaded on first use.
    /// </summary>
    public static class GameSettings
    {
        const string KSfx = "hn_sfx";
        const string KMusic = "hn_music";
        const string KHaptics = "hn_haptics";
        const string KVolume = "hn_volume";
        const string KTutorial = "hn_tutorial_done";
        const string KWeapon = "hn_weapon";

        static bool _loaded;
        static bool _sfx = true;
        static bool _music = true;
        static bool _haptics = true;
        static float _volume = 1f;
        static bool _tutorial;
        static int _weapon;

        static void Load()
        {
            if (_loaded) return;
            _loaded = true;
            _sfx = PlayerPrefs.GetInt(KSfx, 1) == 1;
            _music = PlayerPrefs.GetInt(KMusic, 1) == 1;
            _haptics = PlayerPrefs.GetInt(KHaptics, 1) == 1;
            _volume = PlayerPrefs.GetFloat(KVolume, 1f);
            _tutorial = PlayerPrefs.GetInt(KTutorial, 0) == 1;
            _weapon = PlayerPrefs.GetInt(KWeapon, 0);
        }

        /// <summary>Index into <see cref="Weapons.All"/> of the player's chosen weapon.</summary>
        public static int SelectedWeapon
        {
            get { Load(); return _weapon; }
            set { Load(); _weapon = value; PlayerPrefs.SetInt(KWeapon, value); PlayerPrefs.Save(); }
        }

        public static bool MusicEnabled
        {
            get { Load(); return _music; }
            set { Load(); _music = value; PlayerPrefs.SetInt(KMusic, value ? 1 : 0); PlayerPrefs.Save(); }
        }

        /// <summary>True once the player has seen (or skipped) the tutorial. Auto-shown on first launch.</summary>
        public static bool TutorialDone
        {
            get { Load(); return _tutorial; }
            set { Load(); _tutorial = value; PlayerPrefs.SetInt(KTutorial, value ? 1 : 0); PlayerPrefs.Save(); }
        }

        public static bool SfxEnabled
        {
            get { Load(); return _sfx; }
            set { Load(); _sfx = value; PlayerPrefs.SetInt(KSfx, value ? 1 : 0); PlayerPrefs.Save(); }
        }

        public static bool HapticsEnabled
        {
            get { Load(); return _haptics; }
            set { Load(); _haptics = value; PlayerPrefs.SetInt(KHaptics, value ? 1 : 0); PlayerPrefs.Save(); }
        }

        public static float MasterVolume
        {
            get { Load(); return _volume; }
            set { Load(); _volume = Mathf.Clamp01(value); PlayerPrefs.SetFloat(KVolume, _volume); PlayerPrefs.Save(); }
        }

        /// <summary>Effective SFX gain: master volume, or 0 when SFX are muted.</summary>
        public static float SfxVolume => SfxEnabled ? MasterVolume : 0f;
    }
}
