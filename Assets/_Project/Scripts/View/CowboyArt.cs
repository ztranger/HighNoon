using UnityEngine;

namespace HighNoon
{
    /// <summary>Sprite frames for a cowboy's animation states, drawn for one team color.</summary>
    public class CowboyFrames
    {
        public Sprite[] Idle;
        public Sprite[] Ready;
        public Sprite[] Shoot;
        public Sprite[] Death;
    }

    /// <summary>
    /// Procedurally draws a front-view pixel cowboy in several poses. Point-filtered,
    /// transparent background. Team color tints the shirt + hat band. Deterministic, so
    /// frames are perfectly consistent — swap for hand-made Aseprite sheets later.
    /// </summary>
    public static class CowboyArt
    {
        const int W = 24, H = 32, PPU = 24;

        enum Pose { Idle0, Idle1, Ready, Shoot0, Shoot1, Dead0, Dead1 }

        struct Pal
        {
            public Color32 hat, band, skin, shirt, shirtD, pants, boots, gun, flash, dark;
        }

        public static CowboyFrames Build(Color team)
        {
            var p = MakePalette(team);
            return new CowboyFrames
            {
                Idle  = new[] { Frame(p, Pose.Idle0), Frame(p, Pose.Idle1) },
                Ready = new[] { Frame(p, Pose.Ready) },
                Shoot = new[] { Frame(p, Pose.Shoot0), Frame(p, Pose.Shoot1) },
                Death = new[] { Frame(p, Pose.Dead0), Frame(p, Pose.Dead1) },
            };
        }

        static Pal MakePalette(Color t) => new Pal
        {
            hat   = new Color32(58, 42, 28, 255),
            band  = new Color32((byte)(t.r * 255), (byte)(t.g * 255), (byte)(t.b * 255), 255),
            skin  = new Color32(222, 168, 112, 255),
            shirt = t,
            shirtD = new Color(t.r * 0.62f, t.g * 0.62f, t.b * 0.62f, 1f),
            pants = new Color32(74, 59, 42, 255),
            boots = new Color32(40, 28, 18, 255),
            gun   = new Color32(64, 64, 72, 255),
            flash = new Color32(255, 232, 130, 255),
            dark  = new Color32(28, 20, 14, 255),
        };

        static Sprite Frame(Pal p, Pose pose)
        {
            var px = new Color32[W * H];
            // transparent clear
            for (int i = 0; i < px.Length; i++) px[i] = new Color32(0, 0, 0, 0);

            switch (pose)
            {
                case Pose.Idle0: DrawStanding(px, p, 0, false, false, false); break;
                case Pose.Idle1: DrawStanding(px, p, 1, false, false, false); break;
                case Pose.Ready: DrawStanding(px, p, 0, true, true, false); break;
                case Pose.Shoot0: DrawShoot(px, p, true); break;
                case Pose.Shoot1: DrawShoot(px, p, false); break;
                case Pose.Dead0: DrawDeadStruck(px, p); break;
                case Pose.Dead1: DrawDeadDown(px, p); break;
            }

            var tex = new Texture2D(W, H, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            tex.SetPixels32(px);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, W, H), new Vector2(0.5f, 0.5f), PPU);
        }

        // ---- poses ----

        static void DrawStanding(Color32[] px, Pal p, int dy, bool spread, bool handOnGun, bool extendedRight)
        {
            int lx = spread ? 8 : 9;              // left boot start
            // boots
            Fill(px, lx, 0, lx + 2, 2, p.boots);
            Fill(px, lx + 5, 0, lx + 7, 2, p.boots);
            // pants
            Fill(px, lx, 3, lx + 2, 10, p.pants);
            Fill(px, lx + 5, 3, lx + 7, 10, p.pants);
            // belt + buckle
            Fill(px, 7, 11, 16, 11, p.dark);
            Fill(px, 11, 11, 12, 11, p.band);

            int ty = 12 + dy;                     // torso bottom
            // torso + right-side shading
            Fill(px, 7, ty, 16, ty + 9, p.shirt);
            Fill(px, 15, ty, 16, ty + 9, p.shirtD);
            // left arm + hand
            Fill(px, 5, ty + 1, 6, ty + 7, p.shirt);
            Fill(px, 5, ty, 6, ty, p.skin);
            // right arm + hand (normal)
            if (!extendedRight)
            {
                Fill(px, 17, ty + 1, 18, ty + 7, p.shirt);
                Fill(px, 17, ty, 18, ty, p.skin);
            }
            // holstered gun at right hip
            Fill(px, 16, 8, 18, 10, p.gun);
            if (handOnGun) Fill(px, 16, 11, 17, 11, p.skin);

            // neck
            Fill(px, 11, ty + 9, 12, ty + 9, p.skin);
            // head
            int hy = ty + 10;
            Fill(px, 9, hy, 14, hy + 3, p.skin);
            Set(px, 10, hy + 2, p.dark);          // eyes
            Set(px, 13, hy + 2, p.dark);
            // hat: brim, band, crown
            Fill(px, 6, hy + 4, 17, hy + 4, p.hat);
            Fill(px, 9, hy + 5, 14, hy + 5, p.band);
            Fill(px, 9, hy + 6, 14, hy + 8, p.hat);
        }

        static void DrawShoot(Color32[] px, Pal p, bool flash)
        {
            DrawStanding(px, p, 0, true, false, true);   // base with right arm free
            int ty = 12;
            int ay = ty + 5;                             // extended-arm row
            // outstretched right arm holding revolver
            Fill(px, 17, ay, 18, ay + 1, p.shirt);       // upper arm
            Fill(px, 19, ay, 20, ay + 1, p.skin);        // forearm/hand
            Fill(px, 21, ay, 22, ay + 1, p.gun);         // revolver
            Set(px, 23, ay, p.gun);                      // barrel
            if (flash)
            {
                Set(px, 23, ay + 1, p.flash);
                Set(px, 23, ay - 1, p.flash);
                Set(px, 22, ay + 2, p.flash);
                Set(px, 22, ay - 2, p.flash);
            }
        }

        static void DrawDeadStruck(Color32[] px, Pal p)
        {
            // buckled legs
            Fill(px, 8, 0, 10, 2, p.boots);
            Fill(px, 14, 0, 16, 2, p.boots);
            Fill(px, 8, 3, 10, 7, p.pants);
            Fill(px, 14, 3, 16, 7, p.pants);
            Fill(px, 7, 8, 16, 8, p.dark);               // belt
            // torso leaning back
            Fill(px, 7, 9, 16, 16, p.shirt);
            Fill(px, 15, 9, 16, 16, p.shirtD);
            // arms flung up
            Fill(px, 4, 14, 6, 19, p.shirt); Set(px, 5, 20, p.skin);
            Fill(px, 17, 14, 19, 19, p.shirt); Set(px, 18, 20, p.skin);
            // dazed head
            Fill(px, 10, 17, 15, 20, p.skin);
            Set(px, 11, 19, p.dark); Set(px, 12, 19, p.dark);   // X eyes
            Set(px, 13, 18, p.dark); Set(px, 14, 18, p.dark);
            // hat knocked upward, off the head
            Fill(px, 9, 25, 16, 25, p.hat);
            Fill(px, 11, 26, 14, 26, p.band);
            Fill(px, 11, 27, 14, 29, p.hat);
        }

        static void DrawDeadDown(Color32[] px, Pal p)
        {
            // lying flat, head to the right, hat knocked to the left
            // hat (far left, on the ground)
            Fill(px, 1, 1, 5, 1, p.hat);                 // brim
            Fill(px, 2, 2, 4, 3, p.hat);                 // crown
            Set(px, 2, 2, p.band);
            // boots + legs pointing left
            Fill(px, 5, 2, 7, 4, p.boots);
            Fill(px, 7, 2, 11, 4, p.pants);
            // torso lying horizontally
            Fill(px, 11, 1, 18, 5, p.shirt);
            Fill(px, 11, 1, 18, 2, p.shirtD);            // ground-side shading
            // arm draped
            Fill(px, 13, 5, 15, 6, p.shirt);
            // head to the right
            Fill(px, 18, 2, 21, 5, p.skin);
            Set(px, 19, 4, p.dark); Set(px, 20, 3, p.dark);  // X eyes
            Set(px, 20, 4, p.dark); Set(px, 19, 3, p.dark);
        }

        // ---- raster helpers ----

        static void Set(Color32[] px, int x, int y, Color32 c)
        {
            if (x < 0 || y < 0 || x >= W || y >= H) return;
            px[y * W + x] = c;
        }

        static void Fill(Color32[] px, int x0, int y0, int x1, int y1, Color32 c)
        {
            for (int y = y0; y <= y1; y++)
                for (int x = x0; x <= x1; x++)
                    Set(px, x, y, c);
        }
    }
}
