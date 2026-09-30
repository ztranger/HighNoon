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

        /// <summary>
        /// Optional painted backdrop under Resources (no extension), e.g. <c>Art/Locations/dusty_town</c>.
        /// When set and the texture loads, <see cref="BackgroundBuilder"/> uses that sprite instead of
        /// the procedural sky / ground / props.
        /// </summary>
        public string Background;

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
                Background = "Art/Locations/prairie",
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
                Background = "Art/Locations/dusty_town",
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
                Background = "Art/Locations/red_canyon",
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
                Background = "Art/Locations/gallows_hill",
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
                Background = "Art/Locations/devils_crossroads",
                GroundBase = new Color(0.44f, 0.23f, 0.20f),
                GroundShade = new Color(0.31f, 0.15f, 0.14f),
                Speck = new Color(0.21f, 0.10f, 0.10f),
                Divider = new Color(0f, 0f, 0f, 0.32f),
                CameraFill = new Color(0.36f, 0.19f, 0.17f),
                SkyTop = new Color(0.14f, 0.06f, 0.08f), SkyHorizon = new Color(0.62f, 0.18f, 0.13f),
                Sun = new Color(0.95f, 0.48f, 0.26f), Hill = new Color(0.13f, 0.06f, 0.06f),
            },
            new ArenaDef
            {
                Name = "Night Cemetery", Seed = 1112, Style = ArenaStyle.Graveyard,
                Background = "Art/Locations/night_cemetery",
                GroundBase = new Color(0.42f, 0.32f, 0.24f),
                GroundShade = new Color(0.30f, 0.22f, 0.18f),
                Speck = new Color(0.22f, 0.18f, 0.16f),
                Divider = new Color(0f, 0f, 0f, 0.22f),
                CameraFill = new Color(0.36f, 0.26f, 0.20f),
                SkyTop = new Color(0.10f, 0.14f, 0.28f), SkyHorizon = new Color(0.28f, 0.32f, 0.46f),
                Sun = new Color(0.90f, 0.90f, 0.82f), Hill = new Color(0.12f, 0.14f, 0.22f),
            },
            new ArenaDef
            {
                Name = "Adobe Mission", Seed = 1113, Style = ArenaStyle.Town,
                Background = "Art/Locations/adobe_mission",
                GroundBase = new Color(0.76f, 0.58f, 0.38f),
                GroundShade = new Color(0.62f, 0.46f, 0.30f),
                Speck = new Color(0.50f, 0.36f, 0.22f),
                Divider = new Color(0.40f, 0.28f, 0.16f, 0.28f),
                CameraFill = new Color(0.72f, 0.54f, 0.36f),
                SkyTop = new Color(0.35f, 0.62f, 0.78f), SkyHorizon = new Color(0.93f, 0.78f, 0.55f),
                Sun = new Color(1f, 0.92f, 0.70f), Hill = new Color(0.55f, 0.38f, 0.28f),
            },

            // The Wire — procedural only (no Background). Final paintings: docs/CAMPAIGN_ART.md.
            new ArenaDef
            {
                Name = "Railhead", Seed = 1201, Style = ArenaStyle.Town,
                GroundBase = new Color(0.62f, 0.48f, 0.32f),
                GroundShade = new Color(0.48f, 0.36f, 0.24f),
                Speck = new Color(0.36f, 0.28f, 0.18f),
                Divider = new Color(0.28f, 0.20f, 0.12f, 0.40f),
                CameraFill = new Color(0.58f, 0.44f, 0.30f),
                SkyTop = new Color(0.45f, 0.62f, 0.78f), SkyHorizon = new Color(0.96f, 0.78f, 0.52f),
                Sun = new Color(1f, 0.82f, 0.48f), Hill = new Color(0.32f, 0.24f, 0.18f),
            },
            new ArenaDef
            {
                Name = "Dry Creek", Seed = 1202, Style = ArenaStyle.Town,
                GroundBase = new Color(0.38f, 0.32f, 0.36f),
                GroundShade = new Color(0.28f, 0.24f, 0.30f),
                Speck = new Color(0.18f, 0.16f, 0.22f),
                Divider = new Color(0.12f, 0.10f, 0.16f, 0.45f),
                CameraFill = new Color(0.32f, 0.26f, 0.32f),
                SkyTop = new Color(0.18f, 0.16f, 0.32f), SkyHorizon = new Color(0.62f, 0.40f, 0.36f),
                Sun = new Color(0.95f, 0.62f, 0.32f), Hill = new Color(0.16f, 0.12f, 0.18f),
            },
            new ArenaDef
            {
                Name = "Pole 12", Seed = 1203, Style = ArenaStyle.Prairie,
                GroundBase = new Color(0.72f, 0.64f, 0.40f),
                GroundShade = new Color(0.58f, 0.52f, 0.32f),
                Speck = new Color(0.42f, 0.40f, 0.28f),
                Divider = new Color(0f, 0f, 0f, 0.10f),
                CameraFill = new Color(0.70f, 0.62f, 0.40f),
                SkyTop = new Color(0.55f, 0.72f, 0.86f), SkyHorizon = new Color(0.92f, 0.88f, 0.70f),
                Sun = new Color(1f, 0.96f, 0.78f), Hill = new Color(0.48f, 0.46f, 0.36f),
            },
            new ArenaDef
            {
                Name = "Alkali Tank", Seed = 1204, Style = ArenaStyle.Prairie,
                GroundBase = new Color(0.82f, 0.80f, 0.70f),
                GroundShade = new Color(0.70f, 0.68f, 0.58f),
                Speck = new Color(0.55f, 0.54f, 0.46f),
                Divider = new Color(0f, 0f, 0f, 0.08f),
                CameraFill = new Color(0.80f, 0.78f, 0.68f),
                SkyTop = new Color(0.62f, 0.74f, 0.82f), SkyHorizon = new Color(0.94f, 0.90f, 0.78f),
                Sun = new Color(1f, 0.98f, 0.88f), Hill = new Color(0.62f, 0.60f, 0.52f),
            },
            new ArenaDef
            {
                Name = "The Cut", Seed = 1205, Style = ArenaStyle.Desert,
                GroundBase = new Color(0.62f, 0.36f, 0.26f),
                GroundShade = new Color(0.48f, 0.26f, 0.18f),
                Speck = new Color(0.32f, 0.18f, 0.14f),
                Divider = new Color(0.20f, 0.08f, 0.06f, 0.30f),
                CameraFill = new Color(0.58f, 0.32f, 0.24f),
                SkyTop = new Color(0.55f, 0.32f, 0.28f), SkyHorizon = new Color(0.92f, 0.62f, 0.42f),
                Sun = new Color(1f, 0.78f, 0.48f), Hill = new Color(0.36f, 0.16f, 0.14f),
            },
            new ArenaDef
            {
                Name = "Mile 70", Seed = 1206, Style = ArenaStyle.Prairie,
                GroundBase = new Color(0.55f, 0.40f, 0.28f),
                GroundShade = new Color(0.40f, 0.28f, 0.20f),
                Speck = new Color(0.28f, 0.20f, 0.14f),
                Divider = new Color(0f, 0f, 0f, 0.16f),
                CameraFill = new Color(0.48f, 0.34f, 0.24f),
                SkyTop = new Color(0.28f, 0.32f, 0.48f), SkyHorizon = new Color(0.92f, 0.55f, 0.32f),
                Sun = new Color(1f, 0.62f, 0.32f), Hill = new Color(0.28f, 0.18f, 0.16f),
            },
            new ArenaDef
            {
                Name = "Mile 90 Yard", Seed = 1207, Style = ArenaStyle.Town,
                GroundBase = new Color(0.28f, 0.30f, 0.34f),
                GroundShade = new Color(0.18f, 0.20f, 0.26f),
                Speck = new Color(0.12f, 0.14f, 0.18f),
                Divider = new Color(0.08f, 0.10f, 0.14f, 0.40f),
                CameraFill = new Color(0.22f, 0.24f, 0.30f),
                SkyTop = new Color(0.08f, 0.10f, 0.18f), SkyHorizon = new Color(0.28f, 0.32f, 0.42f),
                Sun = new Color(0.82f, 0.86f, 0.78f), Hill = new Color(0.10f, 0.12f, 0.16f),
            },
            new ArenaDef
            {
                Name = "Mile 90 Shed", Seed = 1208, Style = ArenaStyle.Town,
                GroundBase = new Color(0.24f, 0.28f, 0.26f),
                GroundShade = new Color(0.16f, 0.20f, 0.18f),
                Speck = new Color(0.10f, 0.14f, 0.12f),
                Divider = new Color(0.06f, 0.12f, 0.08f, 0.35f),
                CameraFill = new Color(0.18f, 0.24f, 0.22f),
                SkyTop = new Color(0.06f, 0.10f, 0.12f), SkyHorizon = new Color(0.16f, 0.28f, 0.22f),
                Sun = new Color(0.55f, 0.85f, 0.48f), Hill = new Color(0.08f, 0.12f, 0.10f),
            },
            new ArenaDef
            {
                Name = "Mile 90", Seed = 1209, Style = ArenaStyle.Town,
                GroundBase = new Color(0.16f, 0.16f, 0.20f),
                GroundShade = new Color(0.10f, 0.10f, 0.14f),
                Speck = new Color(0.06f, 0.06f, 0.10f),
                Divider = new Color(0f, 0f, 0f, 0.45f),
                CameraFill = new Color(0.10f, 0.10f, 0.14f),
                SkyTop = new Color(0.04f, 0.05f, 0.10f), SkyHorizon = new Color(0.20f, 0.18f, 0.28f),
                Sun = new Color(0.92f, 0.78f, 0.42f), Hill = new Color(0.06f, 0.06f, 0.08f),
            },

            // The Flood — procedural only. Water climbs from market to roof. Final paintings: docs/CAMPAIGN_ART.md.
            new ArenaDef
            {
                Name = "Lower Market", Seed = 1301, Style = ArenaStyle.Town,
                GroundBase = new Color(0.58f, 0.46f, 0.36f),
                GroundShade = new Color(0.42f, 0.40f, 0.36f),
                Speck = new Color(0.32f, 0.34f, 0.32f),
                Divider = new Color(0.22f, 0.24f, 0.22f, 0.35f),
                CameraFill = new Color(0.55f, 0.44f, 0.34f),
                SkyTop = new Color(0.48f, 0.64f, 0.78f), SkyHorizon = new Color(0.94f, 0.84f, 0.66f),
                Sun = new Color(1f, 0.90f, 0.62f), Hill = new Color(0.42f, 0.32f, 0.26f),
            },
            new ArenaDef
            {
                Name = "Cork Lane", Seed = 1302, Style = ArenaStyle.Town,
                GroundBase = new Color(0.40f, 0.46f, 0.44f),
                GroundShade = new Color(0.28f, 0.36f, 0.36f),
                Speck = new Color(0.20f, 0.28f, 0.28f),
                Divider = new Color(0.12f, 0.22f, 0.22f, 0.40f),
                CameraFill = new Color(0.36f, 0.42f, 0.42f),
                SkyTop = new Color(0.42f, 0.52f, 0.56f), SkyHorizon = new Color(0.72f, 0.74f, 0.68f),
                Sun = new Color(0.90f, 0.86f, 0.70f), Hill = new Color(0.28f, 0.32f, 0.30f),
            },
            new ArenaDef
            {
                Name = "Tanner's", Seed = 1303, Style = ArenaStyle.Town,
                GroundBase = new Color(0.42f, 0.32f, 0.24f),
                GroundShade = new Color(0.30f, 0.24f, 0.20f),
                Speck = new Color(0.22f, 0.16f, 0.14f),
                Divider = new Color(0.16f, 0.10f, 0.08f, 0.40f),
                CameraFill = new Color(0.38f, 0.28f, 0.22f),
                SkyTop = new Color(0.36f, 0.32f, 0.28f), SkyHorizon = new Color(0.70f, 0.58f, 0.42f),
                Sun = new Color(0.86f, 0.70f, 0.42f), Hill = new Color(0.26f, 0.18f, 0.14f),
            },
            new ArenaDef
            {
                Name = "Chapel Street", Seed = 1304, Style = ArenaStyle.Town,
                GroundBase = new Color(0.36f, 0.44f, 0.42f),
                GroundShade = new Color(0.24f, 0.34f, 0.34f),
                Speck = new Color(0.16f, 0.26f, 0.26f),
                Divider = new Color(0.70f, 0.72f, 0.68f, 0.25f),
                CameraFill = new Color(0.32f, 0.40f, 0.40f),
                SkyTop = new Color(0.40f, 0.48f, 0.52f), SkyHorizon = new Color(0.70f, 0.74f, 0.70f),
                Sun = new Color(0.82f, 0.84f, 0.74f), Hill = new Color(0.22f, 0.26f, 0.28f),
            },
            new ArenaDef
            {
                Name = "Bank Steps", Seed = 1305, Style = ArenaStyle.Town,
                GroundBase = new Color(0.55f, 0.54f, 0.50f),
                GroundShade = new Color(0.32f, 0.40f, 0.42f),
                Speck = new Color(0.24f, 0.30f, 0.32f),
                Divider = new Color(0.20f, 0.22f, 0.24f, 0.35f),
                CameraFill = new Color(0.50f, 0.52f, 0.50f),
                SkyTop = new Color(0.55f, 0.66f, 0.74f), SkyHorizon = new Color(0.88f, 0.90f, 0.84f),
                Sun = new Color(1f, 0.98f, 0.88f), Hill = new Color(0.34f, 0.36f, 0.38f),
            },
            new ArenaDef
            {
                Name = "Chapel Roof", Seed = 1306, Style = ArenaStyle.Town,
                GroundBase = new Color(0.48f, 0.40f, 0.34f),
                GroundShade = new Color(0.32f, 0.36f, 0.34f),
                Speck = new Color(0.24f, 0.26f, 0.24f),
                Divider = new Color(0.18f, 0.16f, 0.14f, 0.30f),
                CameraFill = new Color(0.46f, 0.40f, 0.34f),
                SkyTop = new Color(0.42f, 0.40f, 0.48f), SkyHorizon = new Color(0.90f, 0.62f, 0.40f),
                Sun = new Color(1f, 0.70f, 0.38f), Hill = new Color(0.30f, 0.28f, 0.26f),
            },
            new ArenaDef
            {
                Name = "Flood Street", Seed = 1307, Style = ArenaStyle.Town,
                GroundBase = new Color(0.22f, 0.30f, 0.34f),
                GroundShade = new Color(0.12f, 0.20f, 0.26f),
                Speck = new Color(0.08f, 0.14f, 0.18f),
                Divider = new Color(0.06f, 0.12f, 0.16f, 0.45f),
                CameraFill = new Color(0.18f, 0.26f, 0.30f),
                SkyTop = new Color(0.22f, 0.28f, 0.34f), SkyHorizon = new Color(0.48f, 0.54f, 0.56f),
                Sun = new Color(0.70f, 0.74f, 0.70f), Hill = new Color(0.12f, 0.16f, 0.18f),
            },
            new ArenaDef
            {
                Name = "Courthouse Steps", Seed = 1308, Style = ArenaStyle.Town,
                GroundBase = new Color(0.50f, 0.52f, 0.54f),
                GroundShade = new Color(0.28f, 0.36f, 0.40f),
                Speck = new Color(0.20f, 0.26f, 0.30f),
                Divider = new Color(0.16f, 0.18f, 0.20f, 0.35f),
                CameraFill = new Color(0.46f, 0.50f, 0.52f),
                SkyTop = new Color(0.55f, 0.60f, 0.64f), SkyHorizon = new Color(0.78f, 0.80f, 0.78f),
                Sun = new Color(0.92f, 0.90f, 0.82f), Hill = new Color(0.32f, 0.34f, 0.36f),
            },
            new ArenaDef
            {
                Name = "Courthouse Roof", Seed = 1309, Style = ArenaStyle.Town,
                GroundBase = new Color(0.62f, 0.58f, 0.50f),
                GroundShade = new Color(0.40f, 0.42f, 0.44f),
                Speck = new Color(0.30f, 0.32f, 0.34f),
                Divider = new Color(0.20f, 0.22f, 0.24f, 0.25f),
                CameraFill = new Color(0.58f, 0.60f, 0.58f),
                SkyTop = new Color(0.48f, 0.62f, 0.78f), SkyHorizon = new Color(0.90f, 0.88f, 0.78f),
                Sun = new Color(1f, 0.96f, 0.80f), Hill = new Color(0.36f, 0.40f, 0.44f),
            },

            // White Season — procedural only. One winter, bone to night-blue. Final paintings: docs/CAMPAIGN_ART.md.
            new ArenaDef
            {
                Name = "Elbow Gate", Seed = 1401, Style = ArenaStyle.Prairie,
                GroundBase = new Color(0.78f, 0.80f, 0.82f),
                GroundShade = new Color(0.62f, 0.66f, 0.70f),
                Speck = new Color(0.50f, 0.54f, 0.58f),
                Divider = new Color(0.36f, 0.40f, 0.44f, 0.28f),
                CameraFill = new Color(0.72f, 0.76f, 0.78f),
                SkyTop = new Color(0.62f, 0.72f, 0.80f), SkyHorizon = new Color(0.88f, 0.90f, 0.88f),
                Sun = new Color(0.96f, 0.94f, 0.84f), Hill = new Color(0.55f, 0.60f, 0.66f),
            },
            new ArenaDef
            {
                Name = "Switchback", Seed = 1402, Style = ArenaStyle.Desert,
                GroundBase = new Color(0.70f, 0.74f, 0.78f),
                GroundShade = new Color(0.48f, 0.54f, 0.62f),
                Speck = new Color(0.36f, 0.42f, 0.50f),
                Divider = new Color(0.24f, 0.30f, 0.38f, 0.35f),
                CameraFill = new Color(0.64f, 0.68f, 0.74f),
                SkyTop = new Color(0.48f, 0.58f, 0.70f), SkyHorizon = new Color(0.78f, 0.82f, 0.86f),
                Sun = new Color(0.90f, 0.92f, 0.88f), Hill = new Color(0.28f, 0.34f, 0.42f),
            },
            new ArenaDef
            {
                Name = "The Bowl", Seed = 1403, Style = ArenaStyle.Prairie,
                GroundBase = new Color(0.82f, 0.84f, 0.86f),
                GroundShade = new Color(0.70f, 0.74f, 0.76f),
                Speck = new Color(0.60f, 0.64f, 0.66f),
                Divider = new Color(0.50f, 0.54f, 0.56f, 0.20f),
                CameraFill = new Color(0.78f, 0.80f, 0.82f),
                SkyTop = new Color(0.74f, 0.78f, 0.80f), SkyHorizon = new Color(0.86f, 0.88f, 0.88f),
                Sun = new Color(0.90f, 0.90f, 0.88f), Hill = new Color(0.68f, 0.72f, 0.74f),
            },
            new ArenaDef
            {
                Name = "Cache Bowl", Seed = 1404, Style = ArenaStyle.Desert,
                GroundBase = new Color(0.58f, 0.62f, 0.66f),
                GroundShade = new Color(0.40f, 0.44f, 0.50f),
                Speck = new Color(0.48f, 0.28f, 0.24f),
                Divider = new Color(0.22f, 0.26f, 0.30f, 0.35f),
                CameraFill = new Color(0.52f, 0.56f, 0.60f),
                SkyTop = new Color(0.42f, 0.48f, 0.56f), SkyHorizon = new Color(0.70f, 0.74f, 0.76f),
                Sun = new Color(0.82f, 0.84f, 0.80f), Hill = new Color(0.32f, 0.34f, 0.38f),
            },
            new ArenaDef
            {
                Name = "Bowl Rim", Seed = 1405, Style = ArenaStyle.Prairie,
                GroundBase = new Color(0.62f, 0.64f, 0.70f),
                GroundShade = new Color(0.40f, 0.36f, 0.42f),
                Speck = new Color(0.28f, 0.30f, 0.36f),
                Divider = new Color(0.20f, 0.16f, 0.18f, 0.30f),
                CameraFill = new Color(0.56f, 0.50f, 0.52f),
                SkyTop = new Color(0.28f, 0.24f, 0.36f), SkyHorizon = new Color(0.86f, 0.48f, 0.32f),
                Sun = new Color(1f, 0.55f, 0.28f), Hill = new Color(0.22f, 0.18f, 0.24f),
            },
            new ArenaDef
            {
                Name = "Saddle Drift", Seed = 1406, Style = ArenaStyle.Prairie,
                GroundBase = new Color(0.55f, 0.64f, 0.74f),
                GroundShade = new Color(0.28f, 0.36f, 0.52f),
                Speck = new Color(0.18f, 0.24f, 0.38f),
                Divider = new Color(0.12f, 0.16f, 0.28f, 0.40f),
                CameraFill = new Color(0.42f, 0.52f, 0.64f),
                SkyTop = new Color(0.22f, 0.32f, 0.48f), SkyHorizon = new Color(0.62f, 0.72f, 0.82f),
                Sun = new Color(0.78f, 0.84f, 0.90f), Hill = new Color(0.16f, 0.22f, 0.34f),
            },
            new ArenaDef
            {
                Name = "Saddle Stair", Seed = 1407, Style = ArenaStyle.Desert,
                GroundBase = new Color(0.68f, 0.76f, 0.82f),
                GroundShade = new Color(0.36f, 0.48f, 0.60f),
                Speck = new Color(0.24f, 0.34f, 0.46f),
                Divider = new Color(0.16f, 0.22f, 0.32f, 0.40f),
                CameraFill = new Color(0.58f, 0.68f, 0.76f),
                SkyTop = new Color(0.30f, 0.42f, 0.58f), SkyHorizon = new Color(0.72f, 0.80f, 0.86f),
                Sun = new Color(0.86f, 0.90f, 0.92f), Hill = new Color(0.20f, 0.28f, 0.38f),
            },
            new ArenaDef
            {
                Name = "Saddle Cabin", Seed = 1408, Style = ArenaStyle.Prairie,
                GroundBase = new Color(0.28f, 0.34f, 0.46f),
                GroundShade = new Color(0.14f, 0.18f, 0.30f),
                Speck = new Color(0.10f, 0.12f, 0.20f),
                Divider = new Color(0.06f, 0.08f, 0.14f, 0.45f),
                CameraFill = new Color(0.16f, 0.20f, 0.32f),
                SkyTop = new Color(0.06f, 0.08f, 0.16f), SkyHorizon = new Color(0.18f, 0.24f, 0.38f),
                Sun = new Color(0.92f, 0.48f, 0.22f), Hill = new Color(0.08f, 0.10f, 0.16f),
            },

            // San Isidro — procedural only. Adobe and whitewash, oil once inside. Final paintings: docs/CAMPAIGN_ART.md.
            new ArenaDef
            {
                Name = "Mission Gate", Seed = 1501, Style = ArenaStyle.Town,
                GroundBase = new Color(0.72f, 0.58f, 0.38f),
                GroundShade = new Color(0.55f, 0.42f, 0.28f),
                Speck = new Color(0.42f, 0.32f, 0.20f),
                Divider = new Color(0.82f, 0.78f, 0.68f, 0.28f),
                CameraFill = new Color(0.68f, 0.54f, 0.36f),
                SkyTop = new Color(0.46f, 0.62f, 0.78f), SkyHorizon = new Color(0.96f, 0.82f, 0.58f),
                Sun = new Color(1f, 0.88f, 0.55f), Hill = new Color(0.55f, 0.40f, 0.26f),
            },
            new ArenaDef
            {
                Name = "Candle Court", Seed = 1502, Style = ArenaStyle.Town,
                GroundBase = new Color(0.62f, 0.50f, 0.36f),
                GroundShade = new Color(0.28f, 0.22f, 0.16f),
                Speck = new Color(0.18f, 0.14f, 0.10f),
                Divider = new Color(0.12f, 0.10f, 0.08f, 0.40f),
                CameraFill = new Color(0.55f, 0.44f, 0.32f),
                SkyTop = new Color(0.42f, 0.52f, 0.66f), SkyHorizon = new Color(0.92f, 0.74f, 0.46f),
                Sun = new Color(1f, 0.82f, 0.42f), Hill = new Color(0.42f, 0.32f, 0.22f),
            },
            new ArenaDef
            {
                Name = "West Cloister", Seed = 1503, Style = ArenaStyle.Town,
                GroundBase = new Color(0.70f, 0.66f, 0.58f),
                GroundShade = new Color(0.42f, 0.40f, 0.36f),
                Speck = new Color(0.32f, 0.28f, 0.24f),
                Divider = new Color(0.22f, 0.20f, 0.16f, 0.35f),
                CameraFill = new Color(0.64f, 0.60f, 0.52f),
                SkyTop = new Color(0.40f, 0.50f, 0.62f), SkyHorizon = new Color(0.86f, 0.78f, 0.62f),
                Sun = new Color(0.96f, 0.86f, 0.62f), Hill = new Color(0.48f, 0.42f, 0.34f),
            },
            new ArenaDef
            {
                Name = "The Nave", Seed = 1504, Style = ArenaStyle.Town,
                GroundBase = new Color(0.32f, 0.26f, 0.20f),
                GroundShade = new Color(0.18f, 0.14f, 0.12f),
                Speck = new Color(0.12f, 0.10f, 0.08f),
                Divider = new Color(0.08f, 0.06f, 0.05f, 0.45f),
                CameraFill = new Color(0.28f, 0.22f, 0.18f),
                SkyTop = new Color(0.16f, 0.14f, 0.16f), SkyHorizon = new Color(0.42f, 0.32f, 0.22f),
                Sun = new Color(0.72f, 0.52f, 0.28f), Hill = new Color(0.14f, 0.12f, 0.10f),
            },
            new ArenaDef
            {
                Name = "Vestry", Seed = 1505, Style = ArenaStyle.Town,
                GroundBase = new Color(0.66f, 0.62f, 0.54f),
                GroundShade = new Color(0.42f, 0.44f, 0.46f),
                Speck = new Color(0.30f, 0.32f, 0.34f),
                Divider = new Color(0.22f, 0.22f, 0.24f, 0.30f),
                CameraFill = new Color(0.58f, 0.54f, 0.48f),
                SkyTop = new Color(0.36f, 0.38f, 0.42f), SkyHorizon = new Color(0.78f, 0.74f, 0.64f),
                Sun = new Color(0.90f, 0.84f, 0.66f), Hill = new Color(0.36f, 0.34f, 0.30f),
            },
            new ArenaDef
            {
                Name = "Side Chapel", Seed = 1506, Style = ArenaStyle.Town,
                GroundBase = new Color(0.58f, 0.52f, 0.46f),
                GroundShade = new Color(0.36f, 0.32f, 0.28f),
                Speck = new Color(0.26f, 0.22f, 0.20f),
                Divider = new Color(0.18f, 0.16f, 0.14f, 0.32f),
                CameraFill = new Color(0.52f, 0.46f, 0.40f),
                SkyTop = new Color(0.38f, 0.42f, 0.50f), SkyHorizon = new Color(0.82f, 0.72f, 0.58f),
                Sun = new Color(0.94f, 0.80f, 0.55f), Hill = new Color(0.40f, 0.34f, 0.28f),
            },
            new ArenaDef
            {
                Name = "Confession Alley", Seed = 1507, Style = ArenaStyle.Town,
                GroundBase = new Color(0.36f, 0.30f, 0.26f),
                GroundShade = new Color(0.18f, 0.14f, 0.12f),
                Speck = new Color(0.12f, 0.10f, 0.08f),
                Divider = new Color(0.08f, 0.06f, 0.05f, 0.45f),
                CameraFill = new Color(0.32f, 0.26f, 0.22f),
                SkyTop = new Color(0.22f, 0.20f, 0.22f), SkyHorizon = new Color(0.96f, 0.88f, 0.70f),
                Sun = new Color(1f, 0.94f, 0.78f), Hill = new Color(0.20f, 0.16f, 0.14f),
            },
            new ArenaDef
            {
                Name = "Hollow Saint", Seed = 1508, Style = ArenaStyle.Town,
                GroundBase = new Color(0.78f, 0.64f, 0.42f),
                GroundShade = new Color(0.55f, 0.44f, 0.30f),
                Speck = new Color(0.40f, 0.32f, 0.22f),
                Divider = new Color(0.86f, 0.82f, 0.72f, 0.25f),
                CameraFill = new Color(0.74f, 0.60f, 0.40f),
                SkyTop = new Color(0.50f, 0.66f, 0.82f), SkyHorizon = new Color(0.98f, 0.88f, 0.66f),
                Sun = new Color(1f, 0.96f, 0.72f), Hill = new Color(0.58f, 0.44f, 0.28f),
            },
            new ArenaDef
            {
                Name = "The Cells", Seed = 1509, Style = ArenaStyle.Town,
                GroundBase = new Color(0.82f, 0.80f, 0.74f),
                GroundShade = new Color(0.62f, 0.60f, 0.56f),
                Speck = new Color(0.48f, 0.46f, 0.42f),
                Divider = new Color(0.30f, 0.28f, 0.26f, 0.22f),
                CameraFill = new Color(0.78f, 0.76f, 0.72f),
                SkyTop = new Color(0.62f, 0.68f, 0.74f), SkyHorizon = new Color(0.92f, 0.90f, 0.84f),
                Sun = new Color(0.98f, 0.96f, 0.90f), Hill = new Color(0.70f, 0.68f, 0.64f),
            },

            // Sunday Horses — procedural only. A bright fair that ends in mud. Final paintings: docs/CAMPAIGN_ART.md.
            new ArenaDef
            {
                Name = "Bright's Rail", Seed = 1601, Style = ArenaStyle.Ranch,
                GroundBase = new Color(0.62f, 0.52f, 0.32f),
                GroundShade = new Color(0.46f, 0.40f, 0.26f),
                Speck = new Color(0.72f, 0.28f, 0.22f),
                Divider = new Color(0.86f, 0.78f, 0.42f, 0.30f),
                CameraFill = new Color(0.58f, 0.48f, 0.30f),
                SkyTop = new Color(0.42f, 0.62f, 0.84f), SkyHorizon = new Color(0.96f, 0.86f, 0.62f),
                Sun = new Color(1f, 0.92f, 0.58f), Hill = new Color(0.40f, 0.52f, 0.28f),
            },
            new ArenaDef
            {
                Name = "The Paddock", Seed = 1602, Style = ArenaStyle.Ranch,
                GroundBase = new Color(0.55f, 0.46f, 0.28f),
                GroundShade = new Color(0.38f, 0.32f, 0.20f),
                Speck = new Color(0.28f, 0.48f, 0.22f),
                Divider = new Color(0.70f, 0.22f, 0.18f, 0.28f),
                CameraFill = new Color(0.52f, 0.44f, 0.28f),
                SkyTop = new Color(0.48f, 0.66f, 0.86f), SkyHorizon = new Color(0.94f, 0.82f, 0.55f),
                Sun = new Color(1f, 0.88f, 0.48f), Hill = new Color(0.36f, 0.46f, 0.24f),
            },
            new ArenaDef
            {
                Name = "Starting Gate", Seed = 1603, Style = ArenaStyle.Ranch,
                GroundBase = new Color(0.48f, 0.38f, 0.24f),
                GroundShade = new Color(0.32f, 0.26f, 0.16f),
                Speck = new Color(0.22f, 0.18f, 0.12f),
                Divider = new Color(0.18f, 0.14f, 0.10f, 0.35f),
                CameraFill = new Color(0.46f, 0.36f, 0.22f),
                SkyTop = new Color(0.50f, 0.64f, 0.80f), SkyHorizon = new Color(0.90f, 0.78f, 0.52f),
                Sun = new Color(0.98f, 0.86f, 0.50f), Hill = new Color(0.32f, 0.36f, 0.22f),
            },
            new ArenaDef
            {
                Name = "Betting Rail", Seed = 1604, Style = ArenaStyle.Town,
                GroundBase = new Color(0.70f, 0.62f, 0.42f),
                GroundShade = new Color(0.48f, 0.42f, 0.28f),
                Speck = new Color(0.90f, 0.86f, 0.70f),
                Divider = new Color(0.30f, 0.26f, 0.18f, 0.30f),
                CameraFill = new Color(0.66f, 0.58f, 0.40f),
                SkyTop = new Color(0.55f, 0.70f, 0.88f), SkyHorizon = new Color(0.98f, 0.92f, 0.72f),
                Sun = new Color(1f, 0.98f, 0.78f), Hill = new Color(0.42f, 0.38f, 0.28f),
            },
            new ArenaDef
            {
                Name = "Cade's Stable", Seed = 1605, Style = ArenaStyle.Ranch,
                GroundBase = new Color(0.42f, 0.32f, 0.20f),
                GroundShade = new Color(0.28f, 0.22f, 0.14f),
                Speck = new Color(0.55f, 0.42f, 0.18f),
                Divider = new Color(0.16f, 0.12f, 0.08f, 0.40f),
                CameraFill = new Color(0.40f, 0.30f, 0.18f),
                SkyTop = new Color(0.36f, 0.42f, 0.36f), SkyHorizon = new Color(0.82f, 0.70f, 0.42f),
                Sun = new Color(0.96f, 0.78f, 0.40f), Hill = new Color(0.28f, 0.22f, 0.14f),
            },
            new ArenaDef
            {
                Name = "Winners' Circle", Seed = 1606, Style = ArenaStyle.Ranch,
                GroundBase = new Color(0.50f, 0.40f, 0.26f),
                GroundShade = new Color(0.36f, 0.28f, 0.18f),
                Speck = new Color(0.62f, 0.28f, 0.32f),
                Divider = new Color(0.24f, 0.18f, 0.12f, 0.28f),
                CameraFill = new Color(0.48f, 0.38f, 0.24f),
                SkyTop = new Color(0.42f, 0.36f, 0.48f), SkyHorizon = new Color(0.96f, 0.62f, 0.36f),
                Sun = new Color(1f, 0.70f, 0.32f), Hill = new Color(0.34f, 0.28f, 0.20f),
            },
            new ArenaDef
            {
                Name = "Scale House", Seed = 1607, Style = ArenaStyle.Town,
                GroundBase = new Color(0.55f, 0.48f, 0.36f),
                GroundShade = new Color(0.22f, 0.18f, 0.14f),
                Speck = new Color(0.14f, 0.12f, 0.10f),
                Divider = new Color(0.62f, 0.52f, 0.28f, 0.30f),
                CameraFill = new Color(0.48f, 0.40f, 0.28f),
                SkyTop = new Color(0.32f, 0.36f, 0.32f), SkyHorizon = new Color(0.98f, 0.90f, 0.62f),
                Sun = new Color(1f, 0.96f, 0.70f), Hill = new Color(0.30f, 0.26f, 0.18f),
            },
            new ArenaDef
            {
                Name = "Owners' Box", Seed = 1608, Style = ArenaStyle.Town,
                GroundBase = new Color(0.58f, 0.46f, 0.30f),
                GroundShade = new Color(0.36f, 0.28f, 0.18f),
                Speck = new Color(0.72f, 0.22f, 0.28f),
                Divider = new Color(0.20f, 0.16f, 0.12f, 0.30f),
                CameraFill = new Color(0.54f, 0.42f, 0.28f),
                SkyTop = new Color(0.46f, 0.64f, 0.82f), SkyHorizon = new Color(0.92f, 0.80f, 0.55f),
                Sun = new Color(1f, 0.90f, 0.55f), Hill = new Color(0.32f, 0.46f, 0.24f),
            },
            new ArenaDef
            {
                Name = "The Gun", Seed = 1609, Style = ArenaStyle.Ranch,
                GroundBase = new Color(0.44f, 0.36f, 0.24f),
                GroundShade = new Color(0.28f, 0.22f, 0.16f),
                Speck = new Color(0.55f, 0.18f, 0.16f),
                Divider = new Color(0.16f, 0.12f, 0.10f, 0.35f),
                CameraFill = new Color(0.50f, 0.42f, 0.28f),
                SkyTop = new Color(0.48f, 0.68f, 0.88f), SkyHorizon = new Color(0.98f, 0.88f, 0.66f),
                Sun = new Color(1f, 0.96f, 0.72f), Hill = new Color(0.36f, 0.48f, 0.26f),
            },

            // The Circuit — procedural only. Five towns, then a bench with no clock. Final paintings: docs/CAMPAIGN_ART.md.
            new ArenaDef
            {
                Name = "Red Ankle", Seed = 1701, Style = ArenaStyle.Town,
                GroundBase = new Color(0.68f, 0.50f, 0.32f),
                GroundShade = new Color(0.48f, 0.34f, 0.22f),
                Speck = new Color(0.36f, 0.24f, 0.16f),
                Divider = new Color(0.24f, 0.16f, 0.10f, 0.32f),
                CameraFill = new Color(0.62f, 0.46f, 0.30f),
                SkyTop = new Color(0.48f, 0.62f, 0.78f), SkyHorizon = new Color(0.96f, 0.78f, 0.48f),
                Sun = new Color(1f, 0.84f, 0.46f), Hill = new Color(0.46f, 0.32f, 0.20f),
            },
            new ArenaDef
            {
                Name = "Red Ankle Yard", Seed = 1702, Style = ArenaStyle.Town,
                GroundBase = new Color(0.72f, 0.58f, 0.38f),
                GroundShade = new Color(0.40f, 0.30f, 0.20f),
                Speck = new Color(0.28f, 0.20f, 0.14f),
                Divider = new Color(0.18f, 0.14f, 0.10f, 0.35f),
                CameraFill = new Color(0.66f, 0.52f, 0.34f),
                SkyTop = new Color(0.52f, 0.68f, 0.86f), SkyHorizon = new Color(0.98f, 0.88f, 0.62f),
                Sun = new Color(1f, 0.96f, 0.70f), Hill = new Color(0.42f, 0.32f, 0.22f),
            },
            new ArenaDef
            {
                Name = "Red Ankle Square", Seed = 1703, Style = ArenaStyle.Town,
                GroundBase = new Color(0.64f, 0.56f, 0.42f),
                GroundShade = new Color(0.44f, 0.38f, 0.28f),
                Speck = new Color(0.32f, 0.28f, 0.20f),
                Divider = new Color(0.22f, 0.18f, 0.14f, 0.28f),
                CameraFill = new Color(0.60f, 0.52f, 0.38f),
                SkyTop = new Color(0.50f, 0.64f, 0.80f), SkyHorizon = new Color(0.94f, 0.82f, 0.58f),
                Sun = new Color(0.98f, 0.88f, 0.55f), Hill = new Color(0.40f, 0.34f, 0.24f),
            },
            new ArenaDef
            {
                Name = "Miller's Ford", Seed = 1704, Style = ArenaStyle.Town,
                GroundBase = new Color(0.42f, 0.36f, 0.28f),
                GroundShade = new Color(0.28f, 0.32f, 0.30f),
                Speck = new Color(0.20f, 0.24f, 0.22f),
                Divider = new Color(0.16f, 0.18f, 0.16f, 0.35f),
                CameraFill = new Color(0.40f, 0.38f, 0.34f),
                SkyTop = new Color(0.48f, 0.50f, 0.52f), SkyHorizon = new Color(0.70f, 0.68f, 0.62f),
                Sun = new Color(0.82f, 0.80f, 0.72f), Hill = new Color(0.32f, 0.30f, 0.26f),
            },
            new ArenaDef
            {
                Name = "Ford Office", Seed = 1705, Style = ArenaStyle.Town,
                GroundBase = new Color(0.28f, 0.30f, 0.28f),
                GroundShade = new Color(0.14f, 0.16f, 0.14f),
                Speck = new Color(0.10f, 0.12f, 0.10f),
                Divider = new Color(0.08f, 0.10f, 0.08f, 0.40f),
                CameraFill = new Color(0.18f, 0.22f, 0.18f),
                SkyTop = new Color(0.10f, 0.14f, 0.12f), SkyHorizon = new Color(0.16f, 0.28f, 0.18f),
                Sun = new Color(0.42f, 0.72f, 0.36f), Hill = new Color(0.08f, 0.12f, 0.10f),
            },
            new ArenaDef
            {
                Name = "Ford Chapel", Seed = 1706, Style = ArenaStyle.Town,
                GroundBase = new Color(0.32f, 0.26f, 0.20f),
                GroundShade = new Color(0.16f, 0.12f, 0.10f),
                Speck = new Color(0.10f, 0.08f, 0.06f),
                Divider = new Color(0.06f, 0.05f, 0.04f, 0.45f),
                CameraFill = new Color(0.22f, 0.16f, 0.12f),
                SkyTop = new Color(0.08f, 0.08f, 0.12f), SkyHorizon = new Color(0.28f, 0.18f, 0.16f),
                Sun = new Color(0.92f, 0.62f, 0.28f), Hill = new Color(0.10f, 0.08f, 0.08f),
            },
            new ArenaDef
            {
                Name = "Glass Hill", Seed = 1707, Style = ArenaStyle.Prairie,
                GroundBase = new Color(0.58f, 0.56f, 0.50f),
                GroundShade = new Color(0.36f, 0.40f, 0.44f),
                Speck = new Color(0.26f, 0.30f, 0.34f),
                Divider = new Color(0.18f, 0.20f, 0.22f, 0.32f),
                CameraFill = new Color(0.52f, 0.54f, 0.52f),
                SkyTop = new Color(0.46f, 0.58f, 0.72f), SkyHorizon = new Color(0.86f, 0.88f, 0.82f),
                Sun = new Color(0.98f, 0.94f, 0.78f), Hill = new Color(0.42f, 0.46f, 0.48f),
            },
            new ArenaDef
            {
                Name = "Hill Notary", Seed = 1708, Style = ArenaStyle.Prairie,
                GroundBase = new Color(0.52f, 0.40f, 0.30f),
                GroundShade = new Color(0.32f, 0.24f, 0.18f),
                Speck = new Color(0.48f, 0.22f, 0.18f),
                Divider = new Color(0.20f, 0.14f, 0.10f, 0.32f),
                CameraFill = new Color(0.48f, 0.36f, 0.26f),
                SkyTop = new Color(0.36f, 0.28f, 0.36f), SkyHorizon = new Color(0.90f, 0.52f, 0.32f),
                Sun = new Color(1f, 0.62f, 0.30f), Hill = new Color(0.28f, 0.22f, 0.20f),
            },
            new ArenaDef
            {
                Name = "Hanging Elm", Seed = 1709, Style = ArenaStyle.Graveyard,
                GroundBase = new Color(0.36f, 0.28f, 0.26f),
                GroundShade = new Color(0.20f, 0.14f, 0.16f),
                Speck = new Color(0.14f, 0.10f, 0.12f),
                Divider = new Color(0.10f, 0.06f, 0.08f, 0.40f),
                CameraFill = new Color(0.32f, 0.24f, 0.26f),
                SkyTop = new Color(0.22f, 0.16f, 0.24f), SkyHorizon = new Color(0.62f, 0.32f, 0.36f),
                Sun = new Color(0.90f, 0.48f, 0.32f), Hill = new Color(0.16f, 0.10f, 0.12f),
            },
            new ArenaDef
            {
                Name = "Elm Shade", Seed = 1710, Style = ArenaStyle.Graveyard,
                GroundBase = new Color(0.28f, 0.32f, 0.28f),
                GroundShade = new Color(0.14f, 0.18f, 0.16f),
                Speck = new Color(0.10f, 0.12f, 0.10f),
                Divider = new Color(0.08f, 0.10f, 0.08f, 0.40f),
                CameraFill = new Color(0.22f, 0.26f, 0.22f),
                SkyTop = new Color(0.12f, 0.16f, 0.18f), SkyHorizon = new Color(0.32f, 0.36f, 0.32f),
                Sun = new Color(0.55f, 0.62f, 0.48f), Hill = new Color(0.10f, 0.14f, 0.12f),
            },
            new ArenaDef
            {
                Name = "Judges' Seat", Seed = 1711, Style = ArenaStyle.Town,
                GroundBase = new Color(0.48f, 0.46f, 0.44f),
                GroundShade = new Color(0.30f, 0.28f, 0.26f),
                Speck = new Color(0.20f, 0.18f, 0.16f),
                Divider = new Color(0.12f, 0.12f, 0.12f, 0.35f),
                CameraFill = new Color(0.42f, 0.40f, 0.38f),
                SkyTop = new Color(0.36f, 0.38f, 0.42f), SkyHorizon = new Color(0.70f, 0.70f, 0.68f),
                Sun = new Color(0.86f, 0.84f, 0.78f), Hill = new Color(0.28f, 0.28f, 0.28f),
            },
            new ArenaDef
            {
                Name = "The Bench", Seed = 1712, Style = ArenaStyle.Town,
                GroundBase = new Color(0.24f, 0.22f, 0.20f),
                GroundShade = new Color(0.12f, 0.11f, 0.10f),
                Speck = new Color(0.08f, 0.07f, 0.06f),
                Divider = new Color(0.04f, 0.04f, 0.04f, 0.45f),
                CameraFill = new Color(0.16f, 0.15f, 0.14f),
                SkyTop = new Color(0.08f, 0.08f, 0.10f), SkyHorizon = new Color(0.22f, 0.20f, 0.18f),
                Sun = new Color(0.55f, 0.48f, 0.36f), Hill = new Color(0.08f, 0.08f, 0.08f),
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
