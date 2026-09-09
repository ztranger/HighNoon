using System.Collections;
using UnityEngine;

namespace HighNoon
{
    /// <summary>
    /// Visual representation of a duelist. Landscape showdown: the two gunslingers stand at
    /// opposite ends of the street and walk in from their own edge; the one facing the wrong
    /// way is mirrored (flipX) so the pair faces the centre.
    ///
    /// Three render modes, chosen per <see cref="CowboyLook.CharacterId"/> (see <see cref="CowboyCatalog"/>):
    ///  • <b>Sheet</b> — a real 6x4 sprite sheet: frame-by-frame idle / shoot / death, planted by
    ///    the feet. Recoil, muzzle flash and the fall are baked into the frames.
    ///  • <b>Static</b> — a single hand-drawn pose: walk / shoot / death are driven procedurally
    ///    (slide, recoil + muzzle flash, topple).
    ///  • <b>Procedural</b> (fallback when art is missing) — pixel frames from <see cref="CowboyArt"/>.
    /// </summary>
    public class DuelistView : MonoBehaviour
    {
        const float GroundY = -1.35f; // street line where a real cowboy's feet sit
        const float DepthToY = 0.30f; // 2v2 lane vertical stagger (from the pos.y the bootstrap passes)

        // Frame rates for frame-based modes (sheet / procedural).
        const float FpsWalk = 8f, FpsIdle = 7f, FpsShoot = 12f, FpsDeath = 10f;

        SpriteRenderer _sr;
        FrameAnimator _anim;
        Vector3 _homePos;
        Vector3 _offscreenPos;
        bool _rightSide;
        float _popupUp = 1.6f; // world height of the popup anchor above transform.position

        bool _static;          // single-pose code-animation mode
        Sprite _staticSprite;
        CowboyCharacter _char;

        CowboyFrames _frames;  // sheet or procedural

        static Sprite _flash;  // shared muzzle-flash sprite (static mode only)

        /// <summary>Idle sprite for UI portraits.</summary>
        public Sprite IdlePortrait => _static
            ? _staticSprite
            : (_frames != null && _frames.Idle != null && _frames.Idle.Length > 0 ? _frames.Idle[0] : null);

        /// <summary>World point a reaction/accuracy popup pins to (above the head in every mode).</summary>
        public Vector3 PopupAnchor => transform.position + Vector3.up * _popupUp;

        /// <summary><paramref name="rightSide"/> = this duelist stands on the right of the street
        /// (walks in from the right, sprite mirrored to face left toward the centre).</summary>
        public void Setup(SpriteRenderer sr, CowboyLook look, bool rightSide)
        {
            _sr = sr;
            _rightSide = rightSide;
            _char = ResolveCharacter(look);

            _anim = gameObject.AddComponent<FrameAnimator>();
            _anim.Init(sr);

            // 1) Animated sprite sheet?
            if (_char != null && !string.IsNullOrEmpty(_char.SheetBase))
                _frames = CowboySheet.Load(_char);

            if (_frames != null)
            {
                _static = false;
                _anim.ShowStatic(_frames.Idle[0]);
                _sr.flipX = rightSide; // sheet faces right natively → mirror on the right
                PlantOnStreet();
                _popupUp = _char.Height * 0.95f;
                return;
            }

            // 2) Static single pose?
            _staticSprite = _char != null ? CowboySprites.Load(_char) : null;
            if (_staticSprite != null)
            {
                _static = true;
                _anim.ShowStatic(_staticSprite);
                _sr.flipX = rightSide ? _char.FacesRight : !_char.FacesRight;
                PlantOnStreet();
                _popupUp = _char.Height + 0.35f;
                return;
            }

            // 3) Procedural fallback (unchanged framing: centre pivot, bootstrap scale).
            _frames = CowboyArt.Build(look);
            _static = false;
            _sr.sprite = _frames.Idle[0];
            _sr.flipX = rightSide;
            _popupUp = 1.6f;
            _homePos = transform.position;
            _offscreenPos = _homePos + (rightSide ? Vector3.right : Vector3.left) * 9f;
        }

        /// <summary>Feet on the street; normalization is baked into the sprite (PPU) so scale = 1.
        /// The pos.y the bootstrap passes becomes a small depth stagger for 2v2 lanes.</summary>
        void PlantOnStreet()
        {
            var p = transform.position;
            transform.localScale = Vector3.one;
            transform.position = new Vector3(p.x, GroundY + p.y * DepthToY, 0f);
            _homePos = transform.position;
            _offscreenPos = _homePos + (_rightSide ? Vector3.right : Vector3.left) * 9f;
        }

        static CowboyCharacter ResolveCharacter(CowboyLook look)
        {
            if (look == null) return null;
            return CowboyCatalog.Get(look.CharacterId) ?? CowboyCatalog.VillainFor(look.CacheKey());
        }

        // ---- state transitions (called by DuelManager) ----

        public void SetIdleOffscreen()
        {
            StopAllCoroutines();
            transform.rotation = Quaternion.identity;
            transform.position = _offscreenPos;
            if (_static) _anim.ShowStatic(_staticSprite);
            else _anim.Play(_frames.Idle, FpsWalk, loop: true);
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
            if (_static) _anim.ShowStatic(_staticSprite);
            else _anim.Play(_frames.Idle, FpsWalk, loop: true);

            float t = 0f;
            Vector3 start = _offscreenPos;
            while (t < duration)
            {
                t += Time.deltaTime;
                Vector3 pos = Vector3.Lerp(start, _homePos, Mathf.Clamp01(t / duration));
                if (_static) pos.y += Mathf.Abs(Mathf.Sin(t * 10f)) * 0.06f; // procedural footstep bob
                transform.position = pos;
                yield return null;
            }
            transform.position = _homePos;
        }

        public void Stance()
        {
            if (!_static) { _anim.Play(_frames.Ready, FpsIdle, loop: true); return; }
            StopAllCoroutines();
            transform.rotation = Quaternion.identity;
            transform.position = _homePos;
            _anim.ShowStatic(_staticSprite);
            StartCoroutine(Breathe());
        }

        public void PlayShoot()
        {
            if (!_static) { _anim.Play(_frames.Shoot, FpsShoot, loop: false); return; }
            StopAllCoroutines();
            SpawnMuzzleFlash();
            StartCoroutine(Recoil());
        }

        public void PlayDeath()
        {
            if (!_static) { _anim.Play(_frames.Death, FpsDeath, loop: false); return; }
            StopAllCoroutines();
            StartCoroutine(Topple());
        }

        // ---- static-pose code animation ----

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
