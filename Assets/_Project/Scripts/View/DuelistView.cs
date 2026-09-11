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

        bool _skeletal;        // cut-out bone rig mode
        CowboyRig _rig;

        Sprite[] WalkFrames =>
            _frames != null && _frames.Walk != null && _frames.Walk.Length > 0 ? _frames.Walk : _frames?.Idle;

        static Sprite _flash;  // shared muzzle-flash sprite (static mode only)

        /// <summary>Idle sprite for UI portraits.</summary>
        public Sprite IdlePortrait => _skeletal
            ? _rig?.Portrait
            : _static
                ? _staticSprite
                : (_frames != null && _frames.Idle != null && _frames.Idle.Length > 0 ? _frames.Idle[0] : null);

        /// <summary>World point a reaction/accuracy popup pins to (above the head in every mode).</summary>
        public Vector3 PopupAnchor => transform.position + Vector3.up * _popupUp;

        /// <summary>Mount the duelist's weapon in the gun hand (skeletal rig only; a no-op otherwise —
        /// sheet/procedural cowboys carry their gun in their own art).</summary>
        public void SetWeapon(WeaponDef weapon)
        {
            if (!_skeletal || _rig == null || weapon == null) return;
            _rig.SetWeapon(WeaponArt.For(Weapons.IndexOf(weapon)));
        }

        /// <summary><paramref name="rightSide"/> = this duelist stands on the right of the street
        /// (walks in from the right, sprite mirrored to face left toward the centre).</summary>
        public void Setup(SpriteRenderer sr, CowboyLook look, bool rightSide)
            => SetupInternal(sr, ResolveCharacter(look), look, rightSide);

        /// <summary>Set up with an explicit character (preview/debug tools bypass look-based resolution).</summary>
        public void SetupCharacter(SpriteRenderer sr, CowboyCharacter character, bool rightSide)
            => SetupInternal(sr, character, null, rightSide);

        void SetupInternal(SpriteRenderer sr, CowboyCharacter character, CowboyLook look, bool rightSide)
        {
            _sr = sr;
            _rightSide = rightSide;
            _char = character;

            _anim = gameObject.AddComponent<FrameAnimator>();
            _anim.Init(sr);

            // 0) Cut-out skeletal rig? (code-built bones from separate part PNGs)
            if (_char != null && !string.IsNullOrEmpty(_char.RigBase))
            {
                _rig = CowboyRig.Build(transform, _char.RigBase, _char.RigHeight, sr.sortingOrder, mirror: rightSide);
                if (_rig != null)
                {
                    _skeletal = true;
                    _sr.enabled = false; // the body is the child bones; the base renderer stays empty
                    ResetPose();
                    PlantOnStreet();
                    _popupUp = _char.RigHeight * 0.98f;
                    return;
                }
            }

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
            _frames = CowboyArt.Build(look ?? new CowboyLook());
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
            if (_skeletal) { ResetPose(); StartCoroutine(RigWalk()); return; }
            if (_static) _anim.ShowStatic(_staticSprite);
            else _anim.Play(WalkFrames, FpsWalk, loop: true);
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
            if (_skeletal) { /* legs/arms swing below */ }
            else if (_static) _anim.ShowStatic(_staticSprite);
            else _anim.Play(WalkFrames, FpsWalk, loop: true);

            float t = 0f, wt = 0f;
            Vector3 start = _offscreenPos;
            while (t < duration)
            {
                t += Time.deltaTime;
                Vector3 pos = Vector3.Lerp(start, _homePos, Mathf.Clamp01(t / duration));
                if (_skeletal) { wt += Time.deltaTime; RigWalkStep(wt); pos.y += Mathf.Abs(Mathf.Sin(wt * 12f)) * 0.05f; }
                else if (_static) pos.y += Mathf.Abs(Mathf.Sin(t * 10f)) * 0.06f; // procedural footstep bob
                transform.position = pos;
                yield return null;
            }
            transform.position = _homePos;
            if (_skeletal) ResetPose();
        }

        public void Stance()
        {
            if (_skeletal)
            {
                StopAllCoroutines();
                transform.rotation = Quaternion.identity;
                transform.position = _homePos;
                ResetPose();
                StartCoroutine(RigBreathe());
                return;
            }
            if (!_static) { _anim.Play(_frames.Ready, FpsIdle, loop: true); return; }
            StopAllCoroutines();
            transform.rotation = Quaternion.identity;
            transform.position = _homePos;
            _anim.ShowStatic(_staticSprite);
            StartCoroutine(Breathe());
        }

        public void PlayShoot()
        {
            if (_skeletal) { StopAllCoroutines(); StartCoroutine(RigShoot()); return; }
            if (!_static) { _anim.Play(_frames.Shoot, FpsShoot, loop: false); return; }
            StopAllCoroutines();
            SpawnMuzzleFlash();
            StartCoroutine(Recoil());
        }

        public void PlayDeath()
        {
            if (_skeletal) { StopAllCoroutines(); StartCoroutine(RigTopple()); return; }
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

        // ---- skeletal (cut-out) code animation ----
        const float ArmAim = 76f, ForeAim = 8f; // degrees to raise the gun arm from hanging to aiming right

        static void SetZ(Transform t, float z) { if (t) t.localRotation = Quaternion.Euler(0f, 0f, z); }

        void ResetPose()
        {
            if (_rig == null) return;
            SetZ(_rig.Root, 0f); SetZ(_rig.Torso, 0f); SetZ(_rig.Head, 0f);
            SetZ(_rig.ArmUpper, 0f); SetZ(_rig.ArmFore, 0f); SetZ(_rig.ArmBack, 0f);
            SetZ(_rig.LegFront, 0f); SetZ(_rig.LegBack, 0f);
            // note: _rig.Gun keeps its baked -90° (barrel-down) baseline — do not reset it
            _rig.ReattachHat();
        }

        void RigWalkStep(float t)
        {
            float s = Mathf.Sin(t * 9f);
            SetZ(_rig.LegFront, s * 22f);
            SetZ(_rig.LegBack, -s * 22f);
            SetZ(_rig.ArmUpper, -s * 14f);
            SetZ(_rig.ArmBack, s * 16f);
            SetZ(_rig.Torso, Mathf.Sin(t * 18f) * 1.6f);
        }

        IEnumerator RigWalk()
        {
            float t = 0f;
            while (true) { t += Time.deltaTime; RigWalkStep(t); yield return null; }
        }

        IEnumerator RigBreathe()
        {
            while (true)
            {
                float b = Mathf.Sin(Time.time * 2.2f);
                SetZ(_rig.Torso, b * 1.6f);
                SetZ(_rig.Head, -b * 1.1f);
                SetZ(_rig.ArmUpper, b * 2.2f);
                SetZ(_rig.ArmBack, -b * 2.2f);
                yield return null;
            }
        }

        IEnumerator RigShoot()
        {
            Quaternion u0 = _rig.ArmUpper.localRotation, f0 = _rig.ArmFore.localRotation;
            Quaternion uAim = Quaternion.Euler(0f, 0f, ArmAim), fAim = Quaternion.Euler(0f, 0f, ForeAim);

            // whip the arm up to aim
            float t = 0f, d = 0.06f;
            while (t < d) { t += Time.deltaTime; float k = t / d; _rig.ArmUpper.localRotation = Quaternion.Slerp(u0, uAim, k); _rig.ArmFore.localRotation = Quaternion.Slerp(f0, fAim, k); yield return null; }
            _rig.ArmUpper.localRotation = uAim; _rig.ArmFore.localRotation = fAim;

            SpawnMuzzleFlashSkeletal();

            // recoil kick, then settle back to the aim hold
            Quaternion uKick = Quaternion.Euler(0f, 0f, ArmAim - 12f);
            t = 0f; d = 0.05f;
            while (t < d) { t += Time.deltaTime; float k = t / d; _rig.ArmUpper.localRotation = Quaternion.Slerp(uAim, uKick, k); SetZ(_rig.Torso, -3f * k); yield return null; }
            t = 0f; d = 0.12f;
            while (t < d) { t += Time.deltaTime; float k = t / d; _rig.ArmUpper.localRotation = Quaternion.Slerp(uKick, uAim, k); SetZ(_rig.Torso, -3f * (1f - k)); yield return null; }
            _rig.ArmUpper.localRotation = uAim; SetZ(_rig.Torso, 0f);
        }

        IEnumerator RigTopple()
        {
            _rig.DetachHat(transform); // hat rides its own arc, not the falling body
            Vector3 hatStart = _rig.Hat != null ? _rig.Hat.position : Vector3.zero;
            Quaternion hatQ0 = _rig.Hat != null ? _rig.Hat.rotation : Quaternion.identity;
            float back = _rightSide ? 1f : -1f; // fall backward, away from the centre (mirror handles the root)

            const float angle = 80f;
            float t = 0f, dur = 0.55f;
            while (t < dur)
            {
                t += Time.deltaTime;
                float k = Mathf.Clamp01(t / dur), e = k * k; // ease-in: hangs, then drops
                SetZ(_rig.Root, angle * e);
                if (_rig.Hat != null)
                {
                    float hk = Mathf.Clamp01(t / 0.5f);
                    _rig.Hat.position = hatStart + new Vector3(back * 0.55f * hk, 0.4f * Mathf.Sin(hk * Mathf.PI), 0f);
                    _rig.Hat.rotation = hatQ0 * Quaternion.Euler(0f, 0f, -back * 240f * hk);
                }
                yield return null;
            }
            SetZ(_rig.Root, angle);
        }

        void SpawnMuzzleFlashSkeletal()
        {
            if (_flash == null) _flash = BuildFlashSprite();
            var go = new GameObject("MuzzleFlash");
            var fsr = go.AddComponent<SpriteRenderer>();
            fsr.sprite = _flash;
            fsr.sortingOrder = 60;
            go.transform.position = _rig.MuzzleWorld;
            go.transform.localScale = Vector3.one * 0.5f;
            Destroy(go, 0.06f);
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
