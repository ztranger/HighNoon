using UnityEngine;

namespace HighNoon
{
    public enum ArenaStyle { Prairie, Town, Desert, Graveyard, Ranch }

    /// <summary>Describes one duel location (background palette + prop style).</summary>
    public class ArenaDef
    {
        public string Name;
        public ArenaStyle Style;
        public Color GroundBase;
        public Color GroundShade;
        public Color Speck;
        public Color Divider;
        public Color CameraFill;

        // Side-view sky (landscape showdown). Optional — BackgroundBuilder derives a
        // sensible sky from the ground palette when these are left null, so new arenas
        // still build without them.
        public Color? SkyTop;      // colour at the top of the sky gradient
        public Color? SkyHorizon;  // colour where the sky meets the land
        public Color? Sun;         // low sun / moon disc behind the standoff
        public Color? Hill;        // distant silhouette (hills / buttes / rooftops)

        /// <summary>
        /// Stable RNG seed for ground / silhouette. Do not use <c>Name.GetHashCode()</c> —
        /// that is not guaranteed equal in Editor vs IL2CPP, so a PvE mission would
        /// get a different ground on device.
        /// </summary>
        public int Seed;

        /// <summary>Seed used by <see cref="BackgroundBuilder"/>; falls back to a stable FNV of Name.</summary>
        public int RngSeed => Seed != 0 ? Seed : StableHash(Name);

        /// <summary>FNV-1a 32-bit — same value on Mono and IL2CPP.</summary>
        public static int StableHash(string s)
        {
            unchecked
            {
                int hash = (int)2166136261u;
                if (!string.IsNullOrEmpty(s))
                    for (int i = 0; i < s.Length; i++)
                        hash = (hash ^ s[i]) * 16777619;
                return hash == 0 ? 1 : hash;
            }
        }
    }

    /// <summary>
    /// Registry of arenas. Adding a location = one entry here (plus any new props in
    /// <see cref="PropArt"/> / a new case in <see cref="BackgroundBuilder"/>).
    /// </summary>
    public static class Arenas
    {
        public static readonly ArenaDef[] All =
        {
            new ArenaDef
            {
                Name = "Prairie", Seed = 1101, Style = ArenaStyle.Prairie,
                GroundBase = new Color(0.82f, 0.70f, 0.42f),
                GroundShade = new Color(0.72f, 0.60f, 0.34f),
                Speck = new Color(0.60f, 0.50f, 0.30f),
                Divider = new Color(0f, 0f, 0f, 0.12f),
                CameraFill = new Color(0.82f, 0.70f, 0.42f),
                SkyTop = new Color(0.42f, 0.66f, 0.86f), SkyHorizon = new Color(0.94f, 0.86f, 0.66f),
                Sun = new Color(1f, 0.92f, 0.66f), Hill = new Color(0.70f, 0.59f, 0.37f),
            },
            new ArenaDef
            {
                Name = "Dusty Town", Seed = 1102, Style = ArenaStyle.Town,
                GroundBase = new Color(0.64f, 0.52f, 0.38f),
                GroundShade = new Color(0.54f, 0.43f, 0.30f),
                Speck = new Color(0.42f, 0.33f, 0.22f),
                Divider = new Color(0.35f, 0.26f, 0.16f, 0.55f),
                CameraFill = new Color(0.64f, 0.52f, 0.38f),
                SkyTop = new Color(0.52f, 0.64f, 0.76f), SkyHorizon = new Color(0.92f, 0.83f, 0.66f),
                Sun = new Color(0.99f, 0.90f, 0.66f), Hill = new Color(0.44f, 0.36f, 0.27f),
            },
            new ArenaDef
            {
                Name = "Red Canyon", Seed = 1103, Style = ArenaStyle.Desert,
                GroundBase = new Color(0.80f, 0.52f, 0.36f),
                GroundShade = new Color(0.68f, 0.40f, 0.27f),
                Speck = new Color(0.52f, 0.30f, 0.22f),
                Divider = new Color(0.30f, 0.12f, 0.08f, 0.28f),
                CameraFill = new Color(0.80f, 0.52f, 0.36f),
                SkyTop = new Color(0.78f, 0.44f, 0.28f), SkyHorizon = new Color(0.96f, 0.78f, 0.52f),
                Sun = new Color(1f, 0.87f, 0.60f), Hill = new Color(0.49f, 0.24f, 0.16f),
            },
            new ArenaDef
            {
                Name = "Boot Hill", Seed = 1104, Style = ArenaStyle.Graveyard,
                GroundBase = new Color(0.44f, 0.46f, 0.42f),
                GroundShade = new Color(0.35f, 0.37f, 0.34f),
                Speck = new Color(0.27f, 0.29f, 0.27f),
                Divider = new Color(0f, 0f, 0f, 0.22f),
                CameraFill = new Color(0.44f, 0.46f, 0.42f),
                SkyTop = new Color(0.47f, 0.53f, 0.57f), SkyHorizon = new Color(0.80f, 0.79f, 0.71f),
                Sun = new Color(0.88f, 0.87f, 0.76f), Hill = new Color(0.30f, 0.32f, 0.30f),
            },
            new ArenaDef
            {
                Name = "Green Valley", Seed = 1105, Style = ArenaStyle.Ranch,
                GroundBase = new Color(0.52f, 0.62f, 0.34f),
                GroundShade = new Color(0.42f, 0.52f, 0.28f),
                Speck = new Color(0.34f, 0.44f, 0.24f),
                Divider = new Color(0.20f, 0.28f, 0.14f, 0.22f),
                CameraFill = new Color(0.52f, 0.62f, 0.34f),
                SkyTop = new Color(0.40f, 0.68f, 0.85f), SkyHorizon = new Color(0.87f, 0.90f, 0.72f),
                Sun = new Color(0.98f, 0.96f, 0.72f), Hill = new Color(0.30f, 0.42f, 0.22f),
            },
            new ArenaDef
            {
                Name = "Salt Flats", Seed = 1106, Style = ArenaStyle.Desert,
                GroundBase = new Color(0.86f, 0.84f, 0.74f),
                GroundShade = new Color(0.78f, 0.76f, 0.66f),
                Speck = new Color(0.66f, 0.64f, 0.56f),
                Divider = new Color(0f, 0f, 0f, 0.08f),
                CameraFill = new Color(0.86f, 0.84f, 0.74f),
                SkyTop = new Color(0.62f, 0.78f, 0.86f), SkyHorizon = new Color(0.94f, 0.92f, 0.84f),
                Sun = new Color(1f, 0.99f, 0.90f), Hill = new Color(0.72f, 0.70f, 0.62f),
            },
            new ArenaDef
            {
                Name = "Painted Hills", Seed = 1107, Style = ArenaStyle.Prairie,
                GroundBase = new Color(0.88f, 0.62f, 0.44f),
                GroundShade = new Color(0.78f, 0.48f, 0.40f),
                Speck = new Color(0.62f, 0.36f, 0.34f),
                Divider = new Color(0f, 0f, 0f, 0.12f),
                CameraFill = new Color(0.90f, 0.66f, 0.48f),
                SkyTop = new Color(0.55f, 0.44f, 0.62f), SkyHorizon = new Color(0.97f, 0.76f, 0.56f),
                Sun = new Color(1f, 0.86f, 0.62f), Hill = new Color(0.60f, 0.34f, 0.34f),
            },
            new ArenaDef
            {
                Name = "Ghost Town", Seed = 1108, Style = ArenaStyle.Town,
                GroundBase = new Color(0.58f, 0.54f, 0.50f),
                GroundShade = new Color(0.47f, 0.44f, 0.40f),
                Speck = new Color(0.35f, 0.33f, 0.30f),
                Divider = new Color(0.26f, 0.24f, 0.22f, 0.55f),
                CameraFill = new Color(0.56f, 0.53f, 0.50f),
                SkyTop = new Color(0.55f, 0.58f, 0.62f), SkyHorizon = new Color(0.86f, 0.81f, 0.71f),
                Sun = new Color(0.91f, 0.86f, 0.72f), Hill = new Color(0.34f, 0.32f, 0.30f),
            },
            new ArenaDef
            {
                Name = "Midnight Mesa", Seed = 1109, Style = ArenaStyle.Desert,
                GroundBase = new Color(0.28f, 0.30f, 0.44f),
                GroundShade = new Color(0.19f, 0.21f, 0.34f),
                Speck = new Color(0.13f, 0.14f, 0.24f),
                Divider = new Color(0f, 0f, 0f, 0.22f),
                CameraFill = new Color(0.22f, 0.24f, 0.38f),
                SkyTop = new Color(0.07f, 0.08f, 0.18f), SkyHorizon = new Color(0.26f, 0.28f, 0.46f),
                Sun = new Color(0.86f, 0.89f, 0.98f), Hill = new Color(0.10f, 0.11f, 0.20f),
            },
            new ArenaDef
            {
                Name = "Gallows Hill", Seed = 1110, Style = ArenaStyle.Graveyard,
                GroundBase = new Color(0.37f, 0.39f, 0.37f),
                GroundShade = new Color(0.28f, 0.30f, 0.29f),
                Speck = new Color(0.19f, 0.21f, 0.20f),
                Divider = new Color(0f, 0f, 0f, 0.28f),
                CameraFill = new Color(0.34f, 0.36f, 0.35f),
                SkyTop = new Color(0.24f, 0.27f, 0.26f), SkyHorizon = new Color(0.66f, 0.54f, 0.38f),
                Sun = new Color(0.82f, 0.64f, 0.42f), Hill = new Color(0.16f, 0.18f, 0.16f),
            },
            new ArenaDef
            {
                Name = "Devil's Crossroads", Seed = 1111, Style = ArenaStyle.Desert,
                GroundBase = new Color(0.44f, 0.23f, 0.20f),
                GroundShade = new Color(0.31f, 0.15f, 0.14f),
                Speck = new Color(0.21f, 0.10f, 0.10f),
                Divider = new Color(0f, 0f, 0f, 0.32f),
                CameraFill = new Color(0.36f, 0.19f, 0.17f),
                SkyTop = new Color(0.14f, 0.06f, 0.08f), SkyHorizon = new Color(0.62f, 0.18f, 0.13f),
                Sun = new Color(0.95f, 0.48f, 0.26f), Hill = new Color(0.13f, 0.06f, 0.06f),
            },
        };

        static int _last = -1;

        /// <summary>Returns the named arena, or a random one (avoiding an immediate repeat).</summary>
        public static ArenaDef Pick(string name)
        {
            if (!string.IsNullOrEmpty(name))
                foreach (var a in All)
                    if (a.Name == name) return a;

            int i = Random.Range(0, All.Length);
            if (All.Length > 1 && i == _last) i = (i + 1) % All.Length;
            _last = i;
            return All[i];
        }
    }
}
