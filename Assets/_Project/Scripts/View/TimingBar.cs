using UnityEngine;
using UnityEngine.UI;

namespace HighNoon
{
    /// <summary>
    /// A "sweet spot" timing bar for the <see cref="DuelType.Timing"/> duel: a track with a
    /// green target zone and a pointer that sweeps left↔right. The player taps to lock the
    /// pointer; landing on green is a clean shot. Built entirely in code on the HUD canvas.
    /// The top player's bar is rotated 180° so it reads upright from their end of the phone.
    /// </summary>
    public class TimingBar : MonoBehaviour
    {
        RectTransform _self;
        RectTransform _pointer;
        Image _pointerImg;
        Image _track;
        Image _frame;
        Vector2 _restPos;
        float _trackW;
        float _cur;

        public float GreenCenter { get; private set; } // 0..1 along the track
        public float GreenHalf { get; private set; }    // half-width, as a 0..1 fraction of the track
        public bool Locked { get; private set; }
        public float LockedX { get; private set; }

        static readonly Color FrameCol   = new Color(0.05f, 0.04f, 0.03f, 1f);
        static readonly Color TrackCol   = new Color(0.20f, 0.15f, 0.11f, 0.94f);
        static readonly Color GreenCol   = new Color(0.28f, 0.80f, 0.32f, 1f);
        static readonly Color GreenCore  = new Color(0.80f, 1f, 0.55f, 1f);
        static readonly Color PointerCol = new Color(1f, 0.96f, 0.85f, 1f);
        static readonly Color HitCol     = new Color(0.40f, 1f, 0.42f, 1f);
        static readonly Color MissCol    = new Color(1f, 0.42f, 0.36f, 1f);
        static readonly Color TrackHot   = new Color(0.72f, 0.08f, 0.06f, 0.96f);
        static readonly Color FrameHot   = new Color(0.42f, 0.04f, 0.03f, 1f);
        const float ShakeAmp = 7f;     // pixels, whole bar, so the green and the pointer stay aligned

        public void Build(Transform parent, Font font, bool topSide, Vector2 anchoredPos,
                          float width, float height, float greenCenter, float greenHalf, string label)
        {
            GreenCenter = Mathf.Clamp01(greenCenter);
            GreenHalf = greenHalf;
            _trackW = width;

            _self = gameObject.AddComponent<RectTransform>();
            _self.SetParent(parent, false);
            _self.anchorMin = _self.anchorMax = new Vector2(0.5f, 0.5f);
            _self.pivot = new Vector2(0.5f, 0.5f);
            _self.sizeDelta = new Vector2(width, height);
            _self.anchoredPosition = anchoredPos;
            _restPos = anchoredPos;
            if (topSide) _self.localRotation = Quaternion.Euler(0f, 0f, 180f);

            // Frame (slightly larger, dark) behind the track.
            _frame = NewImage("Frame", _self, FrameCol);
            Stretch(_frame.rectTransform, -6f, -6f);

            // Track.
            _track = NewImage("Track", _self, TrackCol);
            Stretch(_track.rectTransform, 0f, 0f);

            // Green target zone.
            float gx = (GreenCenter - 0.5f) * width;
            float gw = GreenHalf * 2f * width;
            var green = NewImage("Green", _track.rectTransform, GreenCol);
            Place(green.rectTransform, new Vector2(gx, 0f), new Vector2(gw, height));

            // Bright core line at the exact green centre.
            var core = NewImage("Core", green.rectTransform, GreenCore);
            Place(core.rectTransform, Vector2.zero, new Vector2(Mathf.Max(6f, gw * 0.14f), height));

            // Pointer (sticks out above/below the track).
            _pointerImg = NewImage("Pointer", _track.rectTransform, PointerCol);
            _pointer = _pointerImg.rectTransform;
            Place(_pointer, Vector2.zero, new Vector2(10f, height + 20f));
            SetSweepX(0f);

            if (!string.IsNullOrEmpty(label) && font != null)
            {
                var go = new GameObject("Label");
                var lrt = go.AddComponent<RectTransform>();
                lrt.SetParent(_self, false);
                lrt.anchorMin = lrt.anchorMax = new Vector2(0.5f, 0.5f);
                lrt.pivot = new Vector2(0.5f, 0.5f);
                lrt.sizeDelta = new Vector2(width, 48f);
                lrt.anchoredPosition = new Vector2(0f, height * 0.5f + 38f);
                var t = go.AddComponent<Text>();
                t.text = label; t.font = font; t.fontSize = 32; t.fontStyle = FontStyle.Bold;
                t.alignment = TextAnchor.MiddleCenter;
                t.color = new Color(1f, 0.94f, 0.78f);
                t.horizontalOverflow = HorizontalWrapMode.Overflow;
                t.verticalOverflow = VerticalWrapMode.Overflow;
                t.raycastTarget = false;
            }
        }

        /// <summary>Move the sweeping pointer (ignored once locked).</summary>
        public void SetSweepX(float x01)
        {
            if (Locked) return;
            _cur = Mathf.Clamp01(x01);
            _pointer.anchoredPosition = new Vector2((_cur - 0.5f) * _trackW, 0f);
        }

        /// <summary>
        /// Colour + shake the bar by how many pointer passes are left in the window (one edge-to-edge
        /// sweep is a pass). No reddening at <see cref="TimingRules.RedFromPasses"/>+ passes remaining;
        /// the red grows as the window runs out, and the final pass (under 1 remaining) also shakes the
        /// whole bar. The green zone rides with the pointer, so the shake never changes the hit.
        /// </summary>
        public void SetPassesLeft(float passesLeft)
        {
            if (Locked || _self == null) return;
            float heat = Mathf.Clamp01((TimingRules.RedFromPasses - passesLeft) / TimingRules.RedFromPasses);
            if (_track != null) _track.color = Color.Lerp(TrackCol, TrackHot, heat);
            if (_frame != null) _frame.color = Color.Lerp(FrameCol, FrameHot, heat);

            if (passesLeft >= 1f) // no shake until the final pass
            {
                _self.anchoredPosition = _restPos;
                return;
            }
            float shake = 1f - Mathf.Clamp01(passesLeft); // 0 at one pass left → 1 at the far end
            float amp = shake * shake * ShakeAmp;
            float wobble = Mathf.Sin(Time.time * 46f) * amp;
            float wobbleY = Mathf.Sin(Time.time * 63f) * amp * 0.4f;
            _self.anchoredPosition = _restPos + new Vector2(wobble, wobbleY);
        }

        /// <summary>Freeze the pointer at <paramref name="x01"/> and colour it hit/miss.</summary>
        public void Lock(float x01)
        {
            Locked = true;
            LockedX = Mathf.Clamp01(x01);
            _cur = LockedX;
            _pointer.anchoredPosition = new Vector2((_cur - 0.5f) * _trackW, 0f);
            _pointerImg.color = IsHit(LockedX) ? HitCol : MissCol;
            _self.anchoredPosition = _restPos;
        }

        public bool IsHit(float x01) => Mathf.Abs(x01 - GreenCenter) <= GreenHalf;
        public float Error(float x01) => Mathf.Abs(x01 - GreenCenter);

        // ---- builders ----

        static Image NewImage(string name, Transform parent, Color color)
        {
            var go = new GameObject(name);
            var rt = go.AddComponent<RectTransform>();
            rt.SetParent(parent, false);
            var img = go.AddComponent<Image>();
            img.color = color;
            img.raycastTarget = false; // never steal taps from HumanDuelInput
            return img;
        }

        static void Stretch(RectTransform rt, float padX, float padY)
        {
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(padX, padY);
            rt.offsetMax = new Vector2(-padX, -padY);
        }

        static void Place(RectTransform rt, Vector2 pos, Vector2 size)
        {
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = size;
            rt.anchoredPosition = pos;
        }
    }
}
