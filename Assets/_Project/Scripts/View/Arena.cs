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
                Name = "Prairie", Style = ArenaStyle.Prairie,
                GroundBase = new Color(0.82f, 0.70f, 0.42f),
                GroundShade = new Color(0.72f, 0.60f, 0.34f),
                Speck = new Color(0.60f, 0.50f, 0.30f),
                Divider = new Color(0f, 0f, 0f, 0.12f),
                CameraFill = new Color(0.82f, 0.70f, 0.42f),
            },
            new ArenaDef
            {
                Name = "Dusty Town", Style = ArenaStyle.Town,
                GroundBase = new Color(0.64f, 0.52f, 0.38f),
                GroundShade = new Color(0.54f, 0.43f, 0.30f),
                Speck = new Color(0.42f, 0.33f, 0.22f),
                Divider = new Color(0.35f, 0.26f, 0.16f, 0.55f),
                CameraFill = new Color(0.64f, 0.52f, 0.38f),
            },
            new ArenaDef
            {
                Name = "Red Canyon", Style = ArenaStyle.Desert,
                GroundBase = new Color(0.80f, 0.52f, 0.36f),
                GroundShade = new Color(0.68f, 0.40f, 0.27f),
                Speck = new Color(0.52f, 0.30f, 0.22f),
                Divider = new Color(0.30f, 0.12f, 0.08f, 0.28f),
                CameraFill = new Color(0.80f, 0.52f, 0.36f),
            },
            new ArenaDef
            {
                Name = "Boot Hill", Style = ArenaStyle.Graveyard,
                GroundBase = new Color(0.44f, 0.46f, 0.42f),
                GroundShade = new Color(0.35f, 0.37f, 0.34f),
                Speck = new Color(0.27f, 0.29f, 0.27f),
                Divider = new Color(0f, 0f, 0f, 0.22f),
                CameraFill = new Color(0.44f, 0.46f, 0.42f),
            },
            new ArenaDef
            {
                Name = "Green Valley", Style = ArenaStyle.Ranch,
                GroundBase = new Color(0.52f, 0.62f, 0.34f),
                GroundShade = new Color(0.42f, 0.52f, 0.28f),
                Speck = new Color(0.34f, 0.44f, 0.24f),
                Divider = new Color(0.20f, 0.28f, 0.14f, 0.22f),
                CameraFill = new Color(0.52f, 0.62f, 0.34f),
            },
            new ArenaDef
            {
                Name = "Salt Flats", Style = ArenaStyle.Desert,
                GroundBase = new Color(0.86f, 0.84f, 0.74f),
                GroundShade = new Color(0.78f, 0.76f, 0.66f),
                Speck = new Color(0.66f, 0.64f, 0.56f),
                Divider = new Color(0f, 0f, 0f, 0.08f),
                CameraFill = new Color(0.86f, 0.84f, 0.74f),
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
