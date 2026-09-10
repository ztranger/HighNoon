using System.Collections.Generic;
using UnityEngine;

namespace HighNoon
{
    /// <summary>Sprite frames for a cowboy's animation states, drawn for one look.</summary>
    public class CowboyFrames
    {
        public Sprite[] Idle;
        public Sprite[] Ready;
        public Sprite[] Shoot;
        public Sprite[] Death;
        public Sprite[] Walk; // optional; DuelistView falls back to Idle
    }

    /// <summary>
    /// Procedurally draws a front-view pixel cowboy in several poses from a
    /// <see cref="CowboyLook"/> (hat style, chest accessory, facial hair, colors).
    /// Point-filtered, transparent background. Deterministic — swap for hand-made
    /// Aseprite sheets later.
    /// </summary>
    public static class CowboyArt
    {
        const int W = 24, H = 32, PPU = 24;

        enum Pose { Idle0, Idle1, Ready, Shoot0, Shoot1, Dead0, Dead1 }

        struct Pal
        {
            public Color32 hat, band, skin, shirt, shirtD, pants, boots, gun, flash, dark, accent, accentD;
            public HatStyle hatType;
            public Accessory chest;
            public FacialHair facial;
        }

        static readonly Dictionary<string, CowboyFrames> Cache = new Dictionary<string, CowboyFrames>();

        public static CowboyFrames Build(CowboyLook look)
        {
            string key = look != null ? look.CacheKey() : "default";
            if (Cache.TryGetValue(key, out var frames)) return frames;

            var p = MakePalette(look ?? CowboyLook.Basic(Color.white));
            frames = new CowboyFrames
            {
                Idle  = new[] { Frame(p, Pose.Idle0), Frame(p, Pose.Idle1) },
                Ready = new[] { Frame(p, Pose.Ready) },
                Shoot = new[] { Frame(p, Pose.Shoot0), Frame(p, Pose.Shoot1) },
                Death = new[] { Frame(p, Pose.Dead0), Frame(p, Pose.Dead1) },
                Walk  = new[] { Frame(p, Pose.Idle0), Frame(p, Pose.Idle1) },
            };
            Cache[key] = frames;
            return frames;
        }

        /// <summary>Convenience: a plain cowboy in the given shirt color.</summary>
        public static CowboyFrames Build(Color shirt) => Build(CowboyLook.Basic(shirt));

        static Color32 Mul(Color c, float f) => new Color32((byte)(c.r * f * 255), (byte)(c.g * f * 255), (byte)(c.b * f * 255), 255);

        static Pal MakePalette(CowboyLook L) => new Pal
        {
            hat    = L.HatColor,
            band   = L.Accent,
            skin   = L.Skin,
            shirt  = L.Shirt,
            shirtD = Mul(L.Shirt, 0.62f),
            pants  = new Color32(74, 59, 42, 255),
            boots  = new Color32(40, 28, 18, 255),
            gun    = new Color32(64, 64, 72, 255),
            flash  = new Color32(255, 232, 130, 255),
            dark   = new Color32(28, 20, 14, 255),
            accent = L.Accent,
            accentD = Mul(L.Accent, 0.6f),
            hatType = L.HatType,
            chest = L.Chest,
            facial = L.Facial,
        };

        static Sprite Frame(Pal p, Pose pose)
        {
            var px = new Color32[W * H];
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

            return ProcSprites.Make(px, W, H, new Vector2(0.5f, 0.5f), PPU);
        }

        // ---- poses ----

        static void DrawStanding(Color32[] px, Pal p, int dy, bool spread, bool handOnGun, bool extendedRight)
        {
            int lx = spread ? 8 : 9;
            Fill(px, lx, 0, lx + 2, 2, p.boots);
            Fill(px, lx + 5, 0, lx + 7, 2, p.boots);
            Fill(px, lx, 3, lx + 2, 10, p.pants);
            Fill(px, lx + 5, 3, lx + 7, 10, p.pants);
            Fill(px, 7, 11, 16, 11, p.dark);
            Fill(px, 11, 11, 12, 11, p.band);

            int ty = 12 + dy;
            Fill(px, 7, ty, 16, ty + 9, p.shirt);
            Fill(px, 15, ty, 16, ty + 9, p.shirtD);
            Fill(px, 5, ty + 1, 6, ty + 7, p.shirt);
            Fill(px, 5, ty, 6, ty, p.skin);
            if (!extendedRight)
            {
                Fill(px, 17, ty + 1, 18, ty + 7, p.shirt);
                Fill(px, 17, ty, 18, ty, p.skin);
            }
            Fill(px, 16, 8, 18, 10, p.gun);
            if (handOnGun) Fill(px, 16, 11, 17, 11, p.skin);

            DrawAccessory(px, p, ty);

            Fill(px, 11, ty + 9, 12, ty + 9, p.skin); // neck
            int hy = ty + 10;
            Fill(px, 9, hy, 14, hy + 3, p.skin);      // head
            Set(px, 10, hy + 2, p.dark);              // eyes
            Set(px, 13, hy + 2, p.dark);

            DrawFacial(px, p, hy);
            DrawHat(px, p, hy);
        }

        static void DrawShoot(Color32[] px, Pal p, bool flash)
        {
            DrawStanding(px, p, 0, true, false, true);
            int ty = 12;
            int ay = ty + 5;
            Fill(px, 17, ay, 18, ay + 1, p.shirt);
            Fill(px, 19, ay, 20, ay + 1, p.skin);
            Fill(px, 21, ay, 22, ay + 1, p.gun);
            Set(px, 23, ay, p.gun);
            if (flash)
            {
                Set(px, 23, ay + 1, p.flash);
                Set(px, 23, ay - 1, p.flash);
                Set(px, 22, ay + 2, p.flash);
                Set(px, 22, ay - 2, p.flash);
            }
        }

        // ---- outfit details ----

        static void DrawHat(Color32[] px, Pal p, int hy)
        {
            switch (p.hatType)
            {
                case HatStyle.Cowboy:
                    Fill(px, 6, hy + 4, 17, hy + 4, p.hat);
                    Fill(px, 9, hy + 5, 14, hy + 5, p.band);
                    Fill(px, 9, hy + 6, 14, hy + 8, p.hat);
                    break;
                case HatStyle.Wide:
                    Fill(px, 3, hy + 4, 20, hy + 4, p.hat);
                    Fill(px, 9, hy + 5, 14, hy + 5, p.band);
                    Fill(px, 9, hy + 6, 14, hy + 8, p.hat);
                    break;
                case HatStyle.Sombrero:
                    Fill(px, 2, hy + 4, 21, hy + 4, p.hat);
                    Set(px, 2, hy + 3, p.hat); Set(px, 21, hy + 3, p.hat); // upturned edges
                    Fill(px, 9, hy + 5, 14, hy + 5, p.accent);
                    Fill(px, 9, hy + 6, 14, hy + 7, p.hat);
                    Fill(px, 10, hy + 8, 13, hy + 9, p.hat);
                    Fill(px, 11, hy + 10, 12, hy + 11, p.hat);   // tall point
                    break;
                case HatStyle.Bowler:
                    Fill(px, 7, hy + 4, 16, hy + 4, p.hat);
                    Fill(px, 8, hy + 5, 15, hy + 6, p.hat);
                    Fill(px, 9, hy + 7, 14, hy + 7, p.hat);      // rounded top
                    break;
            }
        }

        static void DrawFacial(Color32[] px, Pal p, int hy)
        {
            switch (p.facial)
            {
                case FacialHair.Mustache:
                    Fill(px, 9, hy + 1, 14, hy + 1, p.dark);
                    break;
                case FacialHair.Beard:
                    Fill(px, 9, hy, 14, hy + 1, p.dark);
                    Set(px, 8, hy + 1, p.dark); Set(px, 15, hy + 1, p.dark);
                    Set(px, 8, hy + 2, p.dark); Set(px, 15, hy + 2, p.dark);
                    break;
            }
        }

        static void DrawAccessory(Color32[] px, Pal p, int ty)
        {
            switch (p.chest)
            {
                case Accessory.Bandana:
                    Fill(px, 9, ty + 7, 14, ty + 8, p.accent);
                    Fill(px, 11, ty + 9, 12, ty + 9, p.accentD);
                    break;
                case Accessory.Vest:
                    Fill(px, 7, ty, 8, ty + 8, p.accentD);
                    Fill(px, 15, ty, 16, ty + 8, p.accentD);
                    break;
                case Accessory.Poncho:
                    Fill(px, 5, ty + 1, 18, ty + 8, p.accent);
                    Fill(px, 5, ty + 4, 18, ty + 4, p.accentD);   // stripe
                    Fill(px, 5, ty + 8, 18, ty + 8, p.accentD);   // fringe
                    break;
                case Accessory.Badge:
                    Set(px, 9, ty + 6, p.accent);
                    Set(px, 8, ty + 6, p.accent); Set(px, 10, ty + 6, p.accent);
                    Set(px, 9, ty + 5, p.accent); Set(px, 9, ty + 7, p.accent);
                    break;
            }
        }

        // ---- death frames (colors follow the look; poses are generic) ----

        static void DrawDeadStruck(Color32[] px, Pal p)
        {
            Fill(px, 8, 0, 10, 2, p.boots);
            Fill(px, 14, 0, 16, 2, p.boots);
            Fill(px, 8, 3, 10, 7, p.pants);
            Fill(px, 14, 3, 16, 7, p.pants);
            Fill(px, 7, 8, 16, 8, p.dark);
            Fill(px, 7, 9, 16, 16, p.shirt);
            Fill(px, 15, 9, 16, 16, p.shirtD);
            Fill(px, 4, 14, 6, 19, p.shirt); Set(px, 5, 20, p.skin);
            Fill(px, 17, 14, 19, 19, p.shirt); Set(px, 18, 20, p.skin);
            Fill(px, 10, 17, 15, 20, p.skin);
            Set(px, 11, 19, p.dark); Set(px, 12, 19, p.dark);
            Set(px, 13, 18, p.dark); Set(px, 14, 18, p.dark);
            Fill(px, 9, 25, 16, 25, p.hat);
            Fill(px, 11, 26, 14, 26, p.band);
            Fill(px, 11, 27, 14, 29, p.hat);
        }

        static void DrawDeadDown(Color32[] px, Pal p)
        {
            Fill(px, 1, 1, 5, 1, p.hat);
            Fill(px, 2, 2, 4, 3, p.hat);
            Set(px, 2, 2, p.band);
            Fill(px, 5, 2, 7, 4, p.boots);
            Fill(px, 7, 2, 11, 4, p.pants);
            Fill(px, 11, 1, 18, 5, p.shirt);
            Fill(px, 11, 1, 18, 2, p.shirtD);
            Fill(px, 13, 5, 15, 6, p.shirt);
            Fill(px, 18, 2, 21, 5, p.skin);
            Set(px, 19, 4, p.dark); Set(px, 20, 3, p.dark);
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
