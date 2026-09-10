using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace HighNoon
{
    /// <summary>
    /// A code-built cut-out ("paper-doll") skeleton for a cowboy: eight separate part sprites
    /// (see <c>Resources/Art/Cowboys/hero_rig</c> — torso/head/hat/arm_*/leg_*, no gun) assembled
    /// into a bone hierarchy so the duel animations — draw, recoil, topple, walk — are pure
    /// transform rotations with no per-frame art. The weapon is NOT part of the art: a slot in the
    /// gun hand (<see cref="SetWeapon"/>) takes the selected weapon's sprite so guns stay swappable.
    /// An optional <c>hand.png</c> renders OVER the gun (fingers gripping it); omit it and the hand
    /// simply stays baked in arm_fore (behind the gun).
    ///
    /// The trick: every part is drawn on the SAME full canvas, in place. Each sprite pivots at its
    /// joint (normalized within that shared canvas), so placing its bone at the joint's offset from
    /// the parent joint reproduces the drawn pose exactly, and rotating the bone spins the art about
    /// the joint. Assembly is therefore automatic — no per-part positioning by hand.
    ///
    /// Joint coordinates below are authored in the placeholder art's logical 120x170 canvas (y-down,
    /// feet at 60,161). A different character reuses this class as long as its parts share a canvas of
    /// the same proportions and roughly the same joint layout.
    /// </summary>
    public class CowboyRig
    {
        struct Part
        {
            public string Name, Parent;
            public float Jx, Jy;   // joint (attach point) in logical canvas pixels
            public int Sort;       // relative draw order inside the sorting group
            public Part(string n, string p, float x, float y, int s) { Name = n; Parent = p; Jx = x; Jy = y; Sort = s; }
        }

        const float CanvasW = 120f, CanvasH = 170f;
        static readonly Vector2 Feet = new Vector2(60f, 161f); // rig root anchor (plants on the street)
        const float HatTopY = 2f;                              // character spans HatTopY..Feet.y logically

        static readonly Part[] Layout =
        {
            //         name        parent       Jx     Jy   sort  (back -> front)
            new Part("leg_back",  "root",      54f, 112f, 0),
            new Part("arm_back",  "torso",     52f,  69f, 1),
            new Part("torso",     "root",      60f, 112f, 2),
            new Part("leg_front", "root",      66f, 112f, 3),
            new Part("head",      "torso",     57f,  62f, 4),
            new Part("arm_upper", "torso",     70f,  70f, 5),
            new Part("arm_fore",  "arm_upper", 72f,  97f, 6),
            // gun mounts here at sort 7 (created after the loop)
            new Part("hand",      "arm_fore",  72f,  97f, 8), // optional: fingers drawn OVER the gun (grips it)
            new Part("hat",       "head",      57f,  22f, 9),
        };

        public Transform Root, Torso, Head, Hat, ArmUpper, ArmFore, Gun, ArmBack, LegFront, LegBack;
        public SpriteRenderer GunSprite; // the swappable weapon, mounted in the gun hand
        public WeaponMount Mount;        // live-tunable fit (scale / offset / rest angle) — see WeaponMount
        public Sprite Portrait;          // head sprite, used for dialog portraits
        public Vector3 HatHome;          // hat local position on the head (for re-attach after a topple)

        bool _hatOff;

        /// <summary>World point at the gun's muzzle (follows the arm as it aims) for the flash.</summary>
        public Vector3 MuzzleWorld => (Gun != null && Mount != null)
            ? Gun.TransformPoint(Mount.MuzzleLocal)
            : (Root != null ? Root.position : Vector3.zero);

        /// <summary>Builds the rig under <paramref name="parent"/>. Returns null if any part is missing
        /// (the caller then falls back to sheet / static / procedural).</summary>
        public static CowboyRig Build(Transform parent, string resBase, float charHeight, int sortingOrder, bool mirror)
        {
            float u = charHeight / (Feet.y - HatTopY); // world units per logical pixel

            var rig = new CowboyRig();
            var rootGo = new GameObject("rig");
            var root = rootGo.transform;
            root.SetParent(parent, false);
            root.localPosition = Vector3.zero;
            root.localRotation = Quaternion.identity;
            root.localScale = new Vector3(mirror ? -1f : 1f, 1f, 1f); // art faces right → mirror the right-side duelist
            var group = rootGo.AddComponent<SortingGroup>();
            group.sortingOrder = sortingOrder;
            rig.Root = root;

            var bones = new Dictionary<string, Transform> { { "root", root } };
            float ppu = 0f;

            // create in a parent-before-child order
            string[] order = { "torso", "head", "hat", "arm_back", "arm_upper", "arm_fore", "hand", "leg_front", "leg_back" };
            foreach (var name in order)
            {
                if (!TryPart(name, out var part)) continue;

                var tex = Resources.Load<Texture2D>($"{resBase}/{name}");
                if (tex == null)
                {
                    if (name == "hand") continue; // optional overlay — fine to omit (hand stays baked in arm_fore)
                    Debug.LogWarning($"[CowboyRig] missing part Resources/{resBase}/{name} — falling back.");
                    Object.Destroy(rootGo);
                    return null;
                }
                tex.filterMode = FilterMode.Point;
                if (ppu == 0f) ppu = tex.height / (CanvasH * u); // shared canvas → one ppu for all parts

                var pivot = new Vector2(part.Jx / CanvasW, 1f - part.Jy / CanvasH); // logical y-down → sprite v-up
                var spr = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), pivot, ppu, 0, SpriteMeshType.FullRect);
                spr.name = name;
                spr.hideFlags = HideFlags.HideAndDontSave;

                var go = new GameObject(name);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = spr;
                sr.sortingOrder = part.Sort;

                var parentBone = bones[part.Parent];
                go.transform.SetParent(parentBone, false);
                Vector2 pj = JointOf(part.Parent);
                go.transform.localPosition = new Vector3((part.Jx - pj.x) * u, (pj.y - part.Jy) * u, 0f);
                go.transform.localRotation = Quaternion.identity;

                bones[name] = go.transform;
                rig.Assign(name, go.transform, spr);
            }

            // weapon slot in the gun hand — the selected weapon sprite plugs in here (SetWeapon).
            // The anchor is pre-rotated so a right-pointing weapon icon hangs barrel-DOWN at rest
            // (like a lowered gun); the draw animation then swings it up to point at the opponent.
            if (rig.ArmFore != null)
            {
                var wGo = new GameObject("weapon");
                var gsr = wGo.AddComponent<SpriteRenderer>();
                gsr.sortingOrder = 7; // above the forearm/hand (6), below the hat (8)
                wGo.transform.SetParent(rig.ArmFore, false);
                rig.Gun = wGo.transform;
                rig.GunSprite = gsr;
                rig.Mount = wGo.AddComponent<WeaponMount>(); // owns the gun's position/rotation/scale, live-tunable
            }

            if (rig.Hat != null) rig.HatHome = rig.Hat.localPosition;
            return rig;
        }

        /// <summary>Mount a weapon sprite (from <see cref="WeaponArt"/>, drawn pointing right) in the
        /// gun hand: grip at the hand, muzzle recorded for the flash. Pass null to clear.</summary>
        public void SetWeapon(Sprite weapon)
        {
            if (GunSprite == null) return;
            GunSprite.sprite = weapon;
            if (Mount != null) Mount.Init(GunSprite); // positions / scales from the inspector fields
        }

        static bool TryPart(string name, out Part part)
        {
            foreach (var p in Layout) if (p.Name == name) { part = p; return true; }
            part = default;
            return false;
        }

        static Vector2 JointOf(string name)
        {
            if (name == "root") return Feet;
            foreach (var p in Layout) if (p.Name == name) return new Vector2(p.Jx, p.Jy);
            return Feet;
        }

        void Assign(string name, Transform t, Sprite spr)
        {
            switch (name)
            {
                case "torso": Torso = t; break;
                case "head": Head = t; Portrait = spr; break;
                case "hat": Hat = t; break;
                case "arm_back": ArmBack = t; break;
                case "arm_upper": ArmUpper = t; break;
                case "arm_fore": ArmFore = t; break;
                case "leg_front": LegFront = t; break;
                case "leg_back": LegBack = t; break;
            }
        }

        /// <summary>Pop the hat off (reparent so it stops riding the toppling body).</summary>
        public void DetachHat(Transform newParent)
        {
            if (Hat == null || _hatOff) return;
            Hat.SetParent(newParent, true);
            _hatOff = true;
        }

        /// <summary>Put the hat back on the head (for a fresh round on a reused view).</summary>
        public void ReattachHat()
        {
            if (Hat == null || !_hatOff) return;
            Hat.SetParent(Head, false);
            Hat.localPosition = HatHome;
            Hat.localRotation = Quaternion.identity;
            _hatOff = false;
        }
    }
}
