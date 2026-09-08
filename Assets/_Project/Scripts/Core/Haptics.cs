using UnityEngine;

namespace HighNoon
{
    /// <summary>
    /// Lightweight haptic feedback. On Android it drives the system Vibrator with amplitude
    /// control (API 26+ <c>VibrationEffect.createOneShot</c>), falling back to a plain buzz on
    /// older devices. No-ops in the editor / on non-Android and when the player disabled haptics.
    /// Referencing <see cref="Handheld.Vibrate"/> makes Unity add the VIBRATE permission.
    /// </summary>
    public static class Haptics
    {
        static bool _init;
        static int _sdk = -1;
        static AndroidJavaObject _vibrator;

        static void Init()
        {
            if (_init) return;
            _init = true;
            if (Application.platform != RuntimePlatform.Android) return;
            try
            {
                using (var version = new AndroidJavaClass("android.os.Build$VERSION"))
                    _sdk = version.GetStatic<int>("SDK_INT");
                using (var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (var activity = player.GetStatic<AndroidJavaObject>("currentActivity"))
                    _vibrator = activity.Call<AndroidJavaObject>("getSystemService", "vibrator");
            }
            catch { _vibrator = null; }
        }

        static void Buzz(long ms, int amplitude)
        {
            if (!GameSettings.HapticsEnabled) return;
            if (Application.platform != RuntimePlatform.Android) return;
            Init();
            if (_vibrator == null) { Handheld.Vibrate(); return; } // fallback (fixed ~500ms buzz)
            try
            {
                if (_sdk >= 26)
                {
                    using (var fx = new AndroidJavaClass("android.os.VibrationEffect"))
                    using (var oneShot = fx.CallStatic<AndroidJavaObject>("createOneShot", ms, Mathf.Clamp(amplitude, 1, 255)))
                        _vibrator.Call("vibrate", oneShot);
                }
                else
                {
                    _vibrator.Call("vibrate", ms);
                }
            }
            catch { }
        }

        public static void Light()   => Buzz(18, 90);    // a tap / gunshot tick
        public static void Medium()  => Buzz(35, 160);   // UI confirm
        public static void Heavy()   => Buzz(90, 255);   // a death / big hit
        public static void Success() => Buzz(28, 150);   // a win beat
    }
}
