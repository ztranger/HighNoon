using System.Collections;
using UnityEngine;

namespace HighNoon
{
    /// <summary>
    /// Visual representation of a duelist. Landscape showdown: the two gunslingers stand at
    /// opposite ends of the street and walk in from their own edge; the one facing the wrong
    /// way is mirrored (flipX) so the pair faces the centre.
    ///
    /// Two render modes:
    ///  • <b>Real</b> — a single hand-drawn illustration (<see cref="CowboyCatalog"/>) planted on
    ///    the street by its feet; walk / shoot / death are driven procedurally (slide, recoil +
    ///    muzzle flash, topple).
    ///  • <b>Procedural</b> (fallback when art is missing) — frame-by-frame pixel poses from
    ///    <see cref="CowboyArt"/>.
    /// </summary>
    public class DuelistView : MonoBehaviour
    {
        const float GroundY = -1.35f; // street line where a real cowboy's feet sit
        const float DepthToY = 0.30f; // 2v2 lane vertical stagger (from the pos.y the bootstrap passes)

        SpriteRenderer _sr;
        FrameAnimator _anim;
        Vector3 _homePos;
        Vector3 _offscreenPos;
        bool _rightSide;

        // Real-sprite mode
        bool _real;
        Sprite _realSprite;
        CowboyCharacter _char;

        // Procedural fallback
        CowboyFrames _frames;

        static Sprite _flash; // shared muzzle-flash sprite

        /// <summary>Idle sprite for UI portraits — the real illustration, or the procedural idle frame.</summary>
        public Sprite IdlePortrait => _real
            ? _realSprite
            : (_frames != null && _frames.Idle != null && _frames.Idle.Length > 0 ? _frames.Idle[0] : null);

        /// <summary>World point a reaction/accuracy popup should pin to (above the head in either mode).</summary>
        public Vector3 PopupAnchor => _real
            ? transform.position + Vector3.up * (_char.Height + 0.35f)
            : transform.position + Vector3.up * 1.6f;

        /// <summary><paramref name="rightSide"/> = this duelist stands on the right of the street
        /// (walks in from the right, sprite mirrored to face left toward the centre).</summary>
        public void Setup(SpriteRenderer sr, CowboyLook look, bool rightSide)
        {
            _sr = sr;
            _rightSide = rightSide;

            _char = ResolveCharacter(look);
            _realSprite = _char != null ? CowboySprites.Load(_char) : null;
            _real = _realSprite != null;

            _anim = gameObject.AddComponent<FrameAnimator>();
            _anim.Init(sr);

            if (_real)
            {
                _anim.ShowStatic(_realSprite);
                // Face the centre: left-of-street must point right, right-of-street must point left.
                _sr.flipX = rightSide ? _char.FacesRight : !_char.FacesRight;

                // Plant feet on the street. Normalization is baked into the sprite (PPU), so scale = 1;
                // the pos.y the bootstrap passes becomes a small depth stagger for 2v2 lanes.
                var p = transform.position;
                transform.localScale = Vector3.one;
                transform.position = new Vector3(p.x, GroundY + p.y * DepthToY, 0f);
            }
            else
            {
                _frames = CowboyArt.Build(look);
                _sr.sprite = _frames.Idle[0];
                _sr.flipX = rightSide;
            }

            _homePos = transform.position;
            _offscreenPos = _homePos + (rightSide ? Vector3.right : Vector3.left) * 9f;
        }

        static CowboyCharacter ResolveCharacter(CowboyLook look)
        {
            if (look == null) return null;
            return CowboyCatalog.Get(look.CharacterId) ?? CowboyCatalog.VillainFor(look.CacheKey());
        }

        public void SetIdleOffscreen()
        {
            StopAllCoroutines();
            transform.rotation = Quaternion.identity;
            transform.position = _offscreenPos;
            if (_real) _anim.ShowStatic(_realSprite);
            else _anim.Play(_frames.Idle, 3f, loop: true);
        }

        /// <summary>Walk-in runs on this view so <see cref="SetIdleOffscreen"/> can stop it.</summary>
        public void BeginWalkIn(float duration)
        {
            StopAllCoroutines();
            transform.rotation = Quaternion.identity;
            StartCoroutine(WalkIn(duration));
        }

        IEnumerator WalkIn(float duration)
        {
            if (_real) _anim.ShowStatic(_realSprite);
            else _anim.Play(_frames.Idle, 6f, loop: true); // brisker "walking" bob

            float t = 0f;
            Vector3 start = _offscreenPos;
            while (t < duration)
            {
                t += Time.deltaTime;
                Vector3 pos = Vector3.Lerp(start, _homePos, Mathf.Clamp01(t / duration));
                if (_real) pos.y += Mathf.Abs(Mathf.Sin(t * 10f)) * 0.06f; // footstep bob
                transform.position = pos;
                yield return null;
            }
            transform.position = _homePos;
        }

        public void Stance()
        {
            if (!_real) { _anim.Play(_frames.Ready, 4f, loop: true); return; }
            StopAllCoroutines();
            transform.rotation = Quaternion.identity;
            transform.position = _homePos;
            _anim.ShowStatic(_realSprite);
            StartCoroutine(Breathe());
        }

        public void PlayShoot()
        {
            if (!_real) { _anim.Play(_frames.Shoot, 12f, loop: false); return; }
            StopAllCoroutines();
            SpawnMuzzleFlash();
            StartCoroutine(Recoil());
        }

        public void PlayDeath()
        {
            if (!_real) { _anim.Play(_frames.Death, 6f, loop: false); return; }
            StopAllCoroutines();
            StartCoroutine(Topple());
        }

        // ---- real-sprite code animation ----

        IEnumerator Breathe()
        {
            while (true)
            {
                float y = Mathf.Sin(Time.time * 2.2f) * 0.03f;
                transform.position = _homePos + new Vector3(0f, y, 0f);
                yield return null;
            }
        }

        IEnumerator Recoil()
        {
            float dir = _rightSide ? 1f : -1f; // kick back, away from the centre they fire toward
            const float amp = 0.16f, up = 0.05f;
            float t = 0f, d1 = 0.05f;
            while (t < d1) { t += Time.deltaTime; float k = t / d1; transform.position = _homePos + new Vector3(dir * amp * k, up * k, 0f); yield return null; }
            t = 0f; float d2 = 0.16f;
            while (t < d2) { t += Time.deltaTime; float k = 1f - t / d2; transform.position = _homePos + new Vector3(dir * amp * k, up * k, 0f); yield return null; }
            transform.position = _homePos;
        }

        IEnumerator Topple()
        {
            float sign = _rightSide ? -1f : 1f; // rotate about the feet, falling backward from the centre
            const float angle = 84f;
            Vector3 home = transform.position;
            float t = 0f, dur = 0.55f;
            while (t < dur)
            {
                t += Time.deltaTime;
                float k = Mathf.Clamp01(t / dur);
                float e = k * k; // ease-in: hangs, then drops
                transform.rotation = Quaternion.Euler(0f, 0f, sign * angle * e);
                transform.position = home + new Vector3(sign * 0.35f * e, -0.10f * e, 0f);
                yield return null;
            }
            transform.rotation = Quaternion.Euler(0f, 0f, sign * angle);
        }

        void SpawnMuzzleFlash()
        {
            if (_flash == null) _flash = BuildFlashSprite();
            var go = new GameObject("MuzzleFlash");
            var fsr = go.AddComponent<SpriteRenderer>();
            fsr.sprite = _flash;
            fsr.sortingOrder = _sr.sortingOrder + 1;

            float faceDir = _rightSide ? -1f : 1f; // muzzle points toward the centre
            float h = _char.Height;
            go.transform.position = _homePos + new Vector3(faceDir * h * _char.Muzzle.x, h * _char.Muzzle.y, 0f);
            go.transform.localScale = Vector3.one * 0.65f;
            Destroy(go, 0.06f); // survives StopAllCoroutines (timed Destroy, not a coroutine)
        }

        static Sprite BuildFlashSprite()
        {
            const int S = 32;
            var px = new Color32[S * S];
            float c = (S - 1) / 2f;
            for (int y = 0; y < S; y++)
                for (int x = 0; x < S; x++)
                {
                    float dx = (x - c) / c, dy = (y - c) / c;
                    float core = Mathf.Clamp01(1f - Mathf.Sqrt(dx * dx + dy * dy));
                    float spikes = Mathf.Max(0f, 1f - Mathf.Abs(dy) * 5f) * Mathf.Max(0f, 1f - Mathf.Abs(dx))
                                 + Mathf.Max(0f, 1f - Mathf.Abs(dx) * 5f) * Mathf.Max(0f, 1f - Mathf.Abs(dy));
                    float a = Mathf.Clamp01(core * 1.1f + spikes * 0.6f);
                    px[y * S + x] = new Color32(255, (byte)(200 + 55 * a), 110, (byte)(a * 255));
                }
            var s = ProcSprites.Make(px, S, S, new Vector2(0.5f, 0.5f), S); // ppu=S → 1 world unit
            s.hideFlags = HideFlags.HideAndDontSave;
            return s;
        }
    }
}
