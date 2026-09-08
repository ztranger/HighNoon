using UnityEngine;

namespace HighNoon
{
    /// <summary>
    /// Scene-independent one-shot SFX (UI clicks, victory / defeat stings) usable from any menu.
    /// Lazily creates a persistent <see cref="AudioSource"/> host; heard through whichever scene's
    /// camera holds the AudioListener (every bootstrap adds one via <see cref="AppInit"/>). Clips
    /// come from <see cref="AudioBank"/> — a random variant each play, procedural fallback if none.
    /// </summary>
    public static class Sfx
    {
        static AudioSource _src;

        static void Ensure()
        {
            if (_src != null) return;
            var go = new GameObject("~Sfx");
            Object.DontDestroyOnLoad(go);
            _src = go.AddComponent<AudioSource>();
            _src.playOnAwake = false;
        }

        /// <summary>Play an arbitrary clip once (e.g. a weapon preview in the menu).</summary>
        public static void PlayClip(AudioClip clip) { if (clip == null) return; Ensure(); _src.PlayOneShot(clip, 0.95f * GameSettings.SfxVolume); }

        public static void Click()   { Ensure(); _src.PlayOneShot(AudioBank.GetRandom("click", ProcAudio.Click), 0.7f * GameSettings.SfxVolume); }
        public static void Victory() { Ensure(); _src.PlayOneShot(AudioBank.GetRandom("victory", ProcAudio.Victory), 0.9f * GameSettings.SfxVolume); }
        public static void Defeat()  { Ensure(); _src.PlayOneShot(AudioBank.GetRandom("defeat", ProcAudio.Defeat), 0.9f * GameSettings.SfxVolume); }
    }
}
