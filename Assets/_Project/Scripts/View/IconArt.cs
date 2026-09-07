using UnityEngine;

namespace HighNoon
{
    /// <summary>
    /// Procedural app-icon concepts in the game's pixel style. Point-filtered, opaque,
    /// each with a thin black border so it reads on white or black. Mock-up / generator.
    /// </summary>
    public static class IconArt
    {
        static readonly Color32 Black = new Color32(0, 0, 0, 255);

        // ---- concepts ----

        public static Texture2D ShowdownSun()
        {
            const int n = 64;
            var px = Solid(n, new Color32(150, 110, 60, 255)); // ground
            for (int y = 18; y < n; y++)                       // sky gradient
            {
                float t = (y - 18) / (float)(n - 1 - 18);
                Color32 c = Lerp(new Color32(255, 236, 150, 255), new Color32(241, 158, 72, 255), t);
                for (int x = 0; x < n; x++) px[y * n + x] = c;
            }
            Fill(px, n, 0, 16, n - 1, 16, new Color32(214, 152, 82, 255)); // horizon glow
            Fill(px, n, 0, 17, n - 1, 17, new Color32(120, 86, 46, 255));  // horizon line

            int scx = 32, scy = 40;
            Rays(px, n, scx, scy, 16, 23, 16, new Color32(255, 232, 140, 255), false);
            Disc(px, n, scx, scy, 15, new Color32(255, 236, 140, 255));
            Disc(px, n, scx, scy, 11, new Color32(255, 246, 190, 255));
            Disc(px, n, scx, scy, 6, new Color32(255, 252, 224, 255));

            Fill(px, n, 25, 5, 39, 7, new Color32(112, 80, 44, 255));      // ground shadow
            Gunslinger(px, n, 32, new Color32(24, 17, 15, 255));
            Border(px, n, Black);
            return ToTex(px, n);
        }

        static void Gunslinger(Color32[] px, int n, int cx, Color32 c)
        {
            int o = cx - 32;
            Fill(px, n, o + 26, 7, o + 30, 10, c); Fill(px, n, o + 34, 7, o + 38, 10, c);  // boots
            Fill(px, n, o + 27, 10, o + 30, 19, c); Fill(px, n, o + 34, 10, o + 37, 19, c); // legs
            Fill(px, n, o + 24, 17, o + 40, 20, c);   // duster hem (flared)
            Fill(px, n, o + 25, 20, o + 39, 26, c);
            Fill(px, n, o + 26, 26, o + 38, 32, c);   // torso
            Fill(px, n, o + 22, 20, o + 25, 31, c); Fill(px, n, o + 22, 18, o + 26, 20, c); // left arm + hand
            Fill(px, n, o + 39, 20, o + 42, 31, c); Fill(px, n, o + 38, 18, o + 42, 20, c); // right arm + hand
            Fill(px, n, o + 25, 31, o + 39, 34, c);   // shoulders
            Fill(px, n, o + 30, 34, o + 33, 35, c);   // neck
            Fill(px, n, o + 29, 35, o + 34, 40, c);   // head
            Fill(px, n, o + 22, 40, o + 42, 41, c);   // hat brim
            Set(px, n, o + 21, 40, c); Set(px, n, o + 43, 40, c);
            Fill(px, n, o + 28, 41, o + 36, 46, c);   // crown
            Fill(px, n, o + 29, 46, o + 35, 47, c);
        }

        public static Texture2D SixShooter()
        {
            const int n = 48;
            var px = Solid(n, new Color32(198, 112, 44, 255));
            Rays(px, n, 24, 24, 20, 25, 12, new Color32(255, 214, 110, 255), false);
            Disc(px, n, 24, 24, 20, new Color32(255, 214, 110, 255));
            Disc(px, n, 24, 24, 16, new Color32(255, 232, 150, 255));
            MiniRevolver(px, n, 40, 24, -1, new Color32(28, 22, 20, 255)); // barrel points left
            Border(px, n, Black);
            return ToTex(px, n);
        }

        public static Texture2D SheriffStar()
        {
            const int n = 48;
            var px = Solid(n, new Color32(74, 48, 30, 255));
            Vignette(px, n, new Color32(40, 26, 16, 255));
            Star(px, n, 24, 24, 18f, 7.6f, new Color32(120, 80, 30, 255));
            Star(px, n, 24, 24, 16.5f, 6.8f, new Color32(242, 196, 74, 255));
            for (int i = 0; i < 5; i++)
            {
                float a = Mathf.PI / 2 + i * 2f * Mathf.PI / 5f;
                Disc(px, n, 24 + Mathf.RoundToInt(Mathf.Cos(a) * 17f), 24 + Mathf.RoundToInt(Mathf.Sin(a) * 17f), 1, new Color32(200, 150, 40, 255));
            }
            Disc(px, n, 24, 24, 3, new Color32(210, 160, 46, 255));
            Disc(px, n, 24, 24, 1, new Color32(120, 80, 30, 255));
            Border(px, n, Black);
            return ToTex(px, n);
        }

        public static Texture2D CowboyHat()
        {
            const int n = 48;
            var px = Solid(n, new Color32(248, 196, 92, 255));
            Rays(px, n, 24, 24, 0, 30, 12, new Color32(255, 214, 120, 255), false);
            Disc(px, n, 24, 24, 20, new Color32(255, 222, 128, 255));
            Color32 hat = new Color32(58, 40, 26, 255);
            Color32 band = new Color32(178, 60, 46, 255);
            Fill(px, n, 6, 18, 41, 20, hat);
            Fill(px, n, 9, 17, 38, 17, hat);
            Fill(px, n, 9, 21, 38, 21, hat);
            Fill(px, n, 17, 21, 30, 31, hat);
            Fill(px, n, 19, 31, 28, 33, hat);
            Fill(px, n, 17, 22, 30, 24, band);
            Border(px, n, Black);
            return ToTex(px, n);
        }

        public static Texture2D DuelSilhouettes()
        {
            const int n = 48;
            var px = Solid(n, new Color32(245, 175, 70, 255));
            Disc(px, n, 24, 24, 21, new Color32(255, 214, 110, 255));
            Disc(px, n, 24, 24, 16, new Color32(255, 232, 150, 255));
            Fill(px, n, 0, 23, n - 1, 24, new Color32(70, 44, 26, 200));
            var dark = new Color32(30, 20, 15, 255);
            StampCowboy(px, n, 24, 2, 0.62f, false, dark);   // bottom, upright
            StampCowboy(px, n, 24, 26, 0.62f, true, dark);   // top, upside-down (mirrors the game)
            Border(px, n, Black);
            return ToTex(px, n);
        }

        /// <summary>Custom: sheriff star + two revolvers aimed at the centre + a clock-ray sun above.</summary>
        public static Texture2D ClockStarGuns()
        {
            const int n = 64;
            var bg = new Color32(255, 140, 0, 255);   // #FF8C00
            var gold = new Color32(255, 215, 0, 255); // #FFD700
            var steel = new Color32(47, 27, 14, 255); // #2F1B0E
            var px = Solid(n, bg);

            Star(px, n, 32, 32, 28f, 12f, steel);     // dark outline
            Star(px, n, 32, 32, 26.5f, 11.2f, gold);  // gold star

            // clock-ray sun (above the guns)
            int sx = 32, sy = 46;
            for (int k = 0; k < 12; k++)
            {
                float a = k * Mathf.PI / 6f;
                for (int r = 5; r <= 7; r++)
                    Set(px, n, sx + Mathf.RoundToInt(Mathf.Cos(a) * r), sy + Mathf.RoundToInt(Mathf.Sin(a) * r), steel);
            }
            Disc(px, n, sx, sy, 4, new Color32(255, 236, 150, 255));
            Disc(px, n, sx, sy, 2, gold);

            // two revolvers, barrels meeting at the centre
            MiniRevolver(px, n, 30, 30, +1, steel);   // left gun, barrel points right
            MiniRevolver(px, n, 34, 30, -1, steel);   // right gun, barrel points left

            Border(px, n, Black);
            return ToTex(px, n);
        }

        // ---- drawing helpers ----

        static Color32[] Solid(int n, Color32 c)
        {
            var px = new Color32[n * n];
            for (int i = 0; i < px.Length; i++) px[i] = c;
            return px;
        }

        static void Set(Color32[] px, int n, int x, int y, Color32 c)
        {
            if (x < 0 || y < 0 || x >= n || y >= n) return;
            px[y * n + x] = c;
        }

        static void Fill(Color32[] px, int n, int x0, int y0, int x1, int y1, Color32 c)
        {
            for (int y = y0; y <= y1; y++)
                for (int x = x0; x <= x1; x++)
                    Set(px, n, x, y, c);
        }

        static void Disc(Color32[] px, int n, int cx, int cy, int r, Color32 c)
        {
            for (int y = cy - r; y <= cy + r; y++)
                for (int x = cx - r; x <= cx + r; x++)
                {
                    int dx = x - cx, dy = y - cy;
                    if (dx * dx + dy * dy <= r * r) Set(px, n, x, y, c);
                }
        }

        static void Rays(Color32[] px, int n, int cx, int cy, int r0, int r1, int count, Color32 c, bool upperOnly)
        {
            for (int k = 0; k < count; k++)
            {
                float a = k * 2f * Mathf.PI / count;
                if (upperOnly && Mathf.Sin(a) < -0.1f) continue;
                for (int r = r0; r <= r1; r++)
                {
                    int x = cx + Mathf.RoundToInt(Mathf.Cos(a) * r);
                    int y = cy + Mathf.RoundToInt(Mathf.Sin(a) * r);
                    Set(px, n, x, y, c);
                    Set(px, n, x + 1, y, c);
                }
            }
        }

        static void Vignette(Color32[] px, int n, Color32 edge)
        {
            float h = (n - 1) / 2f;
            for (int y = 0; y < n; y++)
                for (int x = 0; x < n; x++)
                {
                    float dx = (x - h) / h, dy = (y - h) / h;
                    if (Mathf.Sqrt(dx * dx + dy * dy) > 0.82f) px[y * n + x] = edge;
                }
        }

        static void Star(Color32[] px, int n, int cx, int cy, float outer, float inner, Color32 c)
        {
            var pts = new Vector2[10];
            for (int i = 0; i < 10; i++)
            {
                float r = (i % 2 == 0) ? outer : inner;
                float a = Mathf.PI / 2 + i * Mathf.PI / 5f;
                pts[i] = new Vector2(cx + Mathf.Cos(a) * r, cy + Mathf.Sin(a) * r);
            }
            for (int y = 0; y < n; y++)
                for (int x = 0; x < n; x++)
                    if (InPoly(pts, x + 0.5f, y + 0.5f)) px[y * n + x] = c;
        }

        static bool InPoly(Vector2[] p, float x, float y)
        {
            bool inside = false;
            for (int i = 0, j = p.Length - 1; i < p.Length; j = i++)
                if (((p[i].y > y) != (p[j].y > y)) &&
                    (x < (p[j].x - p[i].x) * (y - p[i].y) / (p[j].y - p[i].y) + p[i].x))
                    inside = !inside;
            return inside;
        }

        static void Border(Color32[] px, int n, Color32 c)
        {
            for (int i = 0; i < n; i++)
            {
                Set(px, n, i, 0, c); Set(px, n, i, n - 1, c);
                Set(px, n, 0, i, c); Set(px, n, n - 1, i, c);
            }
        }

        static void StampCowboy(Color32[] px, int n, int cx, int baseY, float scale, bool flip180, Color32 c)
        {
            var src = CowboyArt.Build(CowboyLook.Player()).Ready[0].texture.GetPixels32();
            const int sw = 24, sh = 32;
            int dw = Mathf.RoundToInt(sw * scale), dh = Mathf.RoundToInt(sh * scale);
            int ox = cx - dw / 2;
            for (int y = 0; y < dh; y++)
                for (int x = 0; x < dw; x++)
                {
                    int sx = Mathf.Clamp(Mathf.FloorToInt(x / scale), 0, sw - 1);
                    int sy = Mathf.Clamp(Mathf.FloorToInt(y / scale), 0, sh - 1);
                    if (flip180) { sx = sw - 1 - sx; sy = sh - 1 - sy; }
                    if (src[sy * sw + sx].a > 10) Set(px, n, ox + x, baseY + y, c);
                }
        }

        static void MiniRevolver(Color32[] px, int n, int tipX, int tipY, int dir, Color32 c)
        {
            for (int i = 0; i <= 9; i++) { int x = tipX - dir * i; Set(px, n, x, tipY, c); Set(px, n, x, tipY + 1, c); }       // barrel
            for (int i = 10; i <= 13; i++) for (int dyc = -1; dyc <= 2; dyc++) Set(px, n, tipX - dir * i, tipY + dyc, c);       // cylinder
            for (int i = 14; i <= 15; i++) { int x = tipX - dir * i; Set(px, n, x, tipY, c); Set(px, n, x, tipY + 1, c); }      // hammer/frame
            for (int i = 13; i <= 16; i++) for (int g = 1; g <= 6; g++) Set(px, n, tipX - dir * i, tipY - g, c);                // grip (downward)
            Set(px, n, tipX - dir * 11, tipY - 2, c);                                                                          // trigger
        }

        static Color32 Lerp(Color32 a, Color32 b, float t) => new Color32(
            (byte)Mathf.Lerp(a.r, b.r, t), (byte)Mathf.Lerp(a.g, b.g, t),
            (byte)Mathf.Lerp(a.b, b.b, t), 255);

        static Texture2D ToTex(Color32[] px, int n)
        {
            var tex = new Texture2D(n, n, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            tex.SetPixels32(px);
            tex.Apply();
            return tex;
        }
    }
}
