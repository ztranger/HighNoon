using UnityEngine;
using UnityEngine.InputSystem;
using ETouch = UnityEngine.InputSystem.EnhancedTouch;

namespace HighNoon
{
    /// <summary>
    /// A human duelist. Fires on the first touch (or mouse click) inside its
    /// normalized screen zone, with a keyboard fallback for in-editor testing.
    /// Multi-touch aware so two players on one phone are timed independently.
    /// </summary>
    public class HumanDuelInput : IDuelInput
    {
        readonly Rect _zone;          // normalized screen rect (0..1)
        readonly Key _fallbackKey;    // editor/keyboard testing
        bool _armed;
        bool _fired;
        double _fireTime;

        public HumanDuelInput(Rect normalizedZone, Key fallbackKey)
        {
            _zone = normalizedZone;
            _fallbackKey = fallbackKey;
        }

        public bool HasFired => _fired;
        public double FireTimeRealtime => _fireTime;

        public void Arm() { _armed = true; _fired = false; }
        public void OnBang(double bangTimeRealtime) { /* humans react to what they see/hear */ }
        public void ResetInput() { _armed = false; _fired = false; _fireTime = 0; }

        public void Tick(double nowRealtime)
        {
            if (!_armed || _fired) return;

            // Multi-touch (device)
            foreach (var t in ETouch.Touch.activeTouches)
            {
                if (t.phase == UnityEngine.InputSystem.TouchPhase.Began && InZone(t.screenPosition))
                {
                    Fire(nowRealtime);
                    return;
                }
            }

            // Mouse (editor)
            var mouse = Mouse.current;
            if (mouse != null && mouse.leftButton.wasPressedThisFrame && InZone(mouse.position.ReadValue()))
            {
                Fire(nowRealtime);
                return;
            }

            // Keyboard fallback (editor)
            var kb = Keyboard.current;
            if (kb != null && _fallbackKey != Key.None && kb[_fallbackKey].wasPressedThisFrame)
            {
                Fire(nowRealtime);
                return;
            }
        }

        void Fire(double now)
        {
            _fired = true;
            _fireTime = now;
        }

        bool InZone(Vector2 screenPos)
        {
            float nx = screenPos.x / Mathf.Max(1, Screen.width);
            float ny = screenPos.y / Mathf.Max(1, Screen.height);
            return _zone.Contains(new Vector2(nx, ny));
        }
    }
}
