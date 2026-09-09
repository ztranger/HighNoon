using UnityEngine;

namespace HighNoon
{
    /// <summary>
    /// Lightweight haptic feedback. On Android it drives the system Vibrator with amplitude
    /// control (API 26+ <c>VibrationEffect.createOneShot</c>), falling back to a duration-only
    /// vibrate on older devices. No-ops in the editor / on non-Android and when the player
    /// disabled haptics. Referencing <see cref="Handheld.Vibrate"/> makes Unity add the VIBRATE
    /// permission — that call is a ~500 ms buzz, so it is never used on a gunshot tick.
    /// </summary>
    public static class Haptics
    {
        static bool _init;
        static int _sdk = -1;
        static AndroidJavaObject _vibrator;
        static AndroidJavaClass _effectClass;

        // One VibrationEffect per (duration, amplitude) — constructing these via JNI
        // on the shot frame allocates and can hitch the BANG window.
        static AndroidJavaObject _fxLight;
        static AndroidJavaObject _fxMedium;
        static AndroidJavaObject _fxHeavy;
        static AndroidJavaObject _fxSuccess;

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
                if (_sdk >= 26)
                    _effectClass = new AndroidJavaClass("android.os.VibrationEffect");
            }
            catch { _vibrator = null; }
        }

        static AndroidJavaObject CachedEffect(long ms, int amplitude)
        {
            if (ms == 18)  return _fxLight   ??= CreateOneShot(ms, amplitude);
            if (ms == 35)  return _fxMedium  ??= CreateOneShot(ms, amplitude);
            if (ms == 90)  return _fxHeavy   ??= CreateOneShot(ms, amplitude);
            if (ms == 28)  return _fxSuccess ??= CreateOneShot(ms, amplitude);
            return CreateOneShot(ms, amplitude);
        }

        static AndroidJavaObject CreateOneShot(long ms, int amplitude)
        {
            return _effectClass.CallStatic<AndroidJavaObject>(
                "createOneShot", ms, Mathf.Clamp(amplitude, 1, 255));
        }

        static void Buzz(long ms, int amplitude, bool allowLongFallback)
        {
            if (!GameSettings.HapticsEnabled) return;
            if (Application.platform != RuntimePlatform.Android) return;
            Init();
            if (_vibrator == null)
            {
                // Handheld.Vibrate is a fixed ~500 ms buzz — never on a gunshot tick.
                if (allowLongFallback) Handheld.Vibrate();
                return;
            }
            try
            {
                if (_sdk >= 26 && _effectClass != null)
                    _vibrator.Call("vibrate", CachedEffect(ms, amplitude));
                else
                    _vibrator.Call("vibrate", ms);
            }
            catch { }
        }

        public static void Light()   => Buzz(18, 90,  allowLongFallback: false); // tap / gunshot
        public static void Medium()  => Buzz(35, 160, allowLongFallback: true);  // UI confirm
        public static void Heavy()    => Buzz(90, 255, allowLongFallback: true);  // death / big hit
        public static void Success() => Buzz(28, 150, allowLongFallback: true);  // win beat
    }
}
