using UnityEngine;

namespace HighNoon
{
    /// <summary>App-wide runtime settings applied by every scene bootstrap.</summary>
    public static class AppInit
    {
        public static void Apply()
        {
            Application.runInBackground = true;   // keep ticking when unfocused (editor/MCP testing)

            // Whole app is landscape (menus, map, story, tutorial, duel). Lock left so
            // the street's left/right tap sides stay consistent on a phone.
            Screen.autorotateToPortrait = false;
            Screen.autorotateToPortraitUpsideDown = false;
            Screen.autorotateToLandscapeRight = false;
            Screen.autorotateToLandscapeLeft = true;
            Screen.orientation = ScreenOrientation.LandscapeLeft;

            // One frame is gameplay. QualitySettings "Very Low" (current) has vSyncCount=0;
            // cap at 60 and keep vSync off so they don't fight. 120 later if we detect high-refresh.
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 60;

            MusicPlayer.Ensure();                 // persistent background music (starts once, survives scenes)
        }

        /// <summary>
        /// Guarantee exactly one <see cref="AudioListener"/> in the scene (on the given camera).
        /// Every scene builds its camera in code and has no AudioListener of its own, so without
        /// this NOTHING is audible — not in the editor, not on device.
        /// </summary>
        public static void EnsureAudioListener(Camera cam)
        {
            if (cam == null) return;
            if (UnityEngine.Object.FindFirstObjectByType<AudioListener>() != null) return; // avoid duplicates
            cam.gameObject.AddComponent<AudioListener>();
        }
    }
}
