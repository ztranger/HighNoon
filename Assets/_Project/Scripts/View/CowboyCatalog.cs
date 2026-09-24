using UnityEngine;

namespace HighNoon
{
    /// <summary>
    /// One hand-drawn cowboy illustration (a single static pose in
    /// <c>Resources/Art/Cowboys</c>) plus the metadata needed to place and
    /// code-animate it in the landscape showdown.
    /// </summary>
    public class CowboyCharacter
    {
        public string Id;
        public string ResourcePath;                 // static single-pose PNG under Resources/, no extension
        public bool FacesRight;                      // direction the source art / sheet points
        public float Height = 3.0f;                  // static: figure world height. sheet: cell world height (feet→top of cell)
        public Vector2 Muzzle = new Vector2(0.36f, 0.46f); // static only: approx gun muzzle, normalized: x out from center, y up from feet

        // --- animated sprite sheet (optional) ---
        // Atlas: one PNG per clip, sliced at runtime on a uniform SheetCols×SheetRows grid
        // (UV rects on the same texture). Walk = Atlas; optional IdleAtlas / ShootAtlas / DeathAtlas.
        // Missing clips fall back (idle = last walk column; shoot/death = idle).
        // SheetBase: legacy packed 6×4 as "{SheetBase}_00".. files.
        public string Atlas;                          // walk (or the only clip)
        public string IdleAtlas;
        public string ShootAtlas;
        public string DeathAtlas;
        public int DeathCols;                        // 0 = SheetCols (death may be an 8×1 strip)
        public int DeathRows;
        public string SheetBase;
        public int SheetCols = 6, SheetRows = 4;
        public float FeetInset = 0.05f;              // fraction of cell height the feet sit above the cell bottom
        public string WalkBase;                      // optional "{path}/walk_00".. — in-place walk cycle
        public string IdleBase;                      // optional "{path}/idle_00"..
        public string ShootBase;                     // optional "{path}/shoot_00"..
        public string DeathBase;                     // optional "{path}/death_00"..

        // --- cut-out skeletal rig (optional) ---
        // When RigBase is set, DuelistView builds a CowboyRig from the eight part PNGs in that Resources
        // folder (torso/head/hat/arm_*/leg_*) and animates draw/recoil/topple/walk from code. The gun is
        // NOT baked in — the selected weapon sprite mounts in the hand. Takes priority over SheetBase / ResourcePath.
        public string RigBase;
        public float RigHeight = 3.4f;               // feet → hat-top world height
    }

    /// <summary>
    /// Registry of the real cowboy sprites. A <see cref="CowboyLook"/> may name one
    /// via <see cref="CowboyLook.CharacterId"/>; otherwise a stable pick is derived so
    /// every duelist still gets a real illustration (and the player never clones a foe).
    /// Missing files fall back to the procedural <see cref="CowboyArt"/> cowboy.
    /// </summary>
    public static class CowboyCatalog
    {
        public static readonly CowboyCharacter DustyHart = new CowboyCharacter
        {
            Id = "dusty_hart", ResourcePath = "Art/Cowboys/char_dusty_hart",
            FacesRight = true, Height = 2.9f, Muzzle = new Vector2(0.42f, 0.42f),
        };
        public static readonly CowboyCharacter RioVela = new CowboyCharacter
        {
            Id = "rio_vela", ResourcePath = "Art/Cowboys/char_rio_vela",
            FacesRight = true, Height = 3.0f, Muzzle = new Vector2(0.40f, 0.55f),
        };
        public static readonly CowboyCharacter BlackCalhoun = new CowboyCharacter
        {
            Id = "black_calhoun", ResourcePath = "Art/Cowboys/char_black_calhoun",
            FacesRight = false, Height = 3.0f, Muzzle = new Vector2(0.30f, 0.40f),
        };
        public static readonly CowboyCharacter DocGraves = new CowboyCharacter
        {
            Id = "doc_graves", ResourcePath = "Art/Cowboys/char_doc_graves",
            FacesRight = false, Height = 3.2f, Muzzle = new Vector2(0.34f, 0.42f),
        };

        /// <summary>Fully animated test cowboy (red poncho) — the 6x4 sprite-sheet pipeline.</summary>
        public static readonly CowboyCharacter Hero = new CowboyCharacter
        {
            Id = "hero", FacesRight = true, Height = 3.6f,
            SheetBase = "Art/Cowboys/high_noon_hero_sprites/frames/hero_sheet_4x6",
            WalkBase = "Art/Cowboys/high_noon_hero_sprites/frames/hero_walk",
            SheetCols = 6, SheetRows = 4, FeetInset = 11f / 196f,
        };

        /// <summary>Side-view gunslinger — one atlas per clip (walk / idle / shoot / death), all 4×2.</summary>
        public static readonly CowboyCharacter Gunslinger = new CowboyCharacter
        {
            Id = "gunslinger", FacesRight = true, Height = 3.6f,
            Atlas      = "Art/Cowboys/sheets/gunslinger/walk",
            IdleAtlas  = "Art/Cowboys/sheets/gunslinger/idle",
            ShootAtlas = "Art/Cowboys/sheets/gunslinger/shoot",
            DeathAtlas = "Art/Cowboys/sheets/gunslinger/death",
            SheetCols = 4, SheetRows = 2,
            DeathCols = 8, DeathRows = 1,
            FeetInset = 0f, // raw grid cells — feet alignment is done in the atlas tool
        };

        /// <summary>Saloon singer — 8-frame strips (walk / idle / shoot / death).</summary>
        public static readonly CowboyCharacter SaloonSinger = new CowboyCharacter
        {
            Id = "saloon_singer", FacesRight = true, Height = 3.2f,
            Atlas      = "Art/Cowboys/sheets/saloon_singer/walk",
            IdleAtlas  = "Art/Cowboys/sheets/saloon_singer/idle",
            ShootAtlas = "Art/Cowboys/sheets/saloon_singer/shoot",
            DeathAtlas = "Art/Cowboys/sheets/saloon_singer/death",
            SheetCols = 8, SheetRows = 1,
            FeetInset = 0f,
        };

        /// <summary>Cut-out skeletal cowboy (eight part PNGs; weapon mounts in the hand) — the code-built bone rig.</summary>
        public static readonly CowboyCharacter HeroRig = new CowboyCharacter
        {
            Id = "hero_rig", FacesRight = true, RigHeight = 3.4f,
            RigBase = "Art/Cowboys/hero_rig",
        };

        static readonly CowboyCharacter[] All = { Gunslinger, SaloonSinger, HeroRig, Hero, DustyHart, RioVela, BlackCalhoun, DocGraves };

        /// <summary>All characters, for preview/debug tools (e.g. the animation test scene).</summary>
        public static CowboyCharacter[] Roster => All;

        // Opponents cycle through these so a stage bot is never a copy of the player's look.
        static readonly CowboyCharacter[] Villains = { BlackCalhoun, DocGraves, RioVela, DustyHart };

        public static CowboyCharacter Get(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            foreach (var c in All) if (c.Id == id) return c;
            return null;
        }

        /// <summary>Stable villain pick from an arbitrary key (e.g. opponent name / look) so
        /// looks without an explicit <see cref="CowboyLook.CharacterId"/> still get real art.</summary>
        public static CowboyCharacter VillainFor(string key)
        {
            if (string.IsNullOrEmpty(key)) return BlackCalhoun;
            unchecked
            {
                int h = 17;
                foreach (char ch in key) h = h * 31 + ch;
                return Villains[(h & 0x7fffffff) % Villains.Length];
            }
        }
    }
}
