using UnityEngine;

namespace HighNoon
{
    /// <summary>
    /// Live-tunable mount for the in-hand weapon sprite (built by <see cref="CowboyRig"/>). Sits on the
    /// "weapon" object in the gun hand (a child of the forearm bone) and drives its Transform from the
    /// fields below every LateUpdate — so you can select it in Play mode, drag the values, and watch the
    /// gun move/rotate/scale live. Because it's parented to the forearm, one placement holds across the
    /// whole draw: the gun swings up with the arm. Play-mode edits reset on Stop — bake good numbers here.
    /// </summary>
    public class WeaponMount : MonoBehaviour
    {
        [Header("Weapon fit — tune live in Play, then bake the numbers here")]
        [Tooltip("Position of the gun in the hand, relative to the forearm bone.")]
        public Vector2 localPos = new Vector2(0.31f, -0.84f);

        [Range(-180f, 180f)]
        [Tooltip("Barrel angle at rest. -90 = straight down along the leg; the draw swings it up from here.")]
        public float restAngle = -90f;

        [Tooltip("Overall weapon size.")]
        public float scale = 2.0f;

        [Range(0f, 1f)]
        [Tooltip("Where the muzzle (flash origin) sits across the weapon icon. 0.5 = its centre.")]
        public float muzzleFrac = 0.66f;

        SpriteRenderer _sprite;

        public void Init(SpriteRenderer sprite) { _sprite = sprite; Apply(); }

        void LateUpdate() { Apply(); } // live: inspector edits show immediately

        void Apply()
        {
            transform.localPosition = new Vector3(localPos.x, localPos.y, 0f);
            transform.localRotation = Quaternion.Euler(0f, 0f, restAngle);
            transform.localScale = Vector3.one * scale;
        }

        /// <summary>Muzzle point in the weapon's local space (for the flash). The icon pivot is its centre,
        /// so the muzzle is offset from centre by (muzzleFrac - 0.5) of the icon width.</summary>
        public Vector3 MuzzleLocal
        {
            get
            {
                if (_sprite == null || _sprite.sprite == null) return Vector3.zero;
                return new Vector3((muzzleFrac - 0.5f) * _sprite.sprite.bounds.size.x, 0f, 0f);
            }
        }
    }
}
