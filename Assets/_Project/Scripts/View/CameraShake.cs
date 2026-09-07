using UnityEngine;

namespace HighNoon
{
    /// <summary>
    /// Positional camera shake. Uses unscaled time so it keeps shaking during a
    /// hit-stop (Time.timeScale == 0). Call <see cref="Shake"/> to trigger.
    /// </summary>
    public class CameraShake : MonoBehaviour
    {
        Vector3 _base;
        float _amplitude;
        float _duration;
        float _timeLeft;

        void Awake() => _base = transform.localPosition;

        public void Shake(float amplitude, float duration)
        {
            _amplitude = amplitude;
            _duration = Mathf.Max(0.01f, duration);
            _timeLeft = _duration;
        }

        void LateUpdate()
        {
            if (_timeLeft > 0f)
            {
                _timeLeft -= Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(_timeLeft / _duration);
                Vector2 o = Random.insideUnitCircle * (_amplitude * k);
                transform.localPosition = _base + new Vector3(o.x, o.y, 0f);
            }
            else
            {
                transform.localPosition = _base;
            }
        }
    }
}
