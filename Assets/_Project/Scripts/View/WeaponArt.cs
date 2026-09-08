using UnityEngine;

namespace HighNoon
{
    /// <summary>
    /// Procedural pixel weapon icons (Point-filtered), one distinct silhouette per weapon, all on
    /// a shared 64×32 canvas so they scale uniformly in the menu. <see cref="For"/> maps a weapon
    /// index (see <see cref="Weapons.All"/>) to its sprite. Guns point right.
    /// </summary>
    public static class WeaponArt
    {
        const int W = 64, H = 32;

        static readonly Color OL  = new Color(0.07f, 0.06f, 0.06f);
        static readonly Color ST  = new Color(0.50f, 0.53f, 0.60f); // steel
        static readonly Color STd = new Color(0.30f, 0.33f, 0.40f);
        static readonly Color STh = new Color(0.70f, 0.73f, 0.80f);
        static readonly Color BK  = new Color(0.17f, 0.17f, 0.19f); // black
        static readonly Color BKh = new Color(0.32f, 0.32f, 0.36f);
        static readonly Color WD  = new Color(0.52f, 0.33f, 0.18f); // wood
        static readonly Color WK  = new Color(0.37f, 0.22f, 0.12f);
        static readonly Color BR  = new Color(0.80f, 0.62f, 0.26f); // brass
        static readonly Color BRd = new Color(0.55f, 0.42f, 0.18f);
        static readonly Color BRh = new Color(0.93f, 0.80f, 0.42f);
        static readonly Color CP  = new Color(0.74f, 0.44f, 0.24f); // copper
        static readonly Color GL  = new Color(0.45f, 0.72f, 0.88f); // scope glass
        static readonly Color STM = new Color(1f, 1f, 1f, 0.55f);   // steam

        public static Sprite For(int index)
        {
            switch (index)
            {
                case 1: return DesertEagle();
                case 2: return Shotgun();
                case 3: return Sniper();
                case 4: return Steampunk();
                default: return Revolver();
            }
        }

        public static Sprite Revolver()
        {
            var px = New();
            // barrel
            Fill(px, 18, 19, 42, 23, ST); Fill(px, 18, 22, 42, 23, STh); Fill(px, 18, 19, 42, 20, STd);
            Fill(px, 41, 18, 43, 24, STd);              // muzzle
            Fill(px, 12, 17, 26, 19, ST);               // top frame
            Fill(px, 22, 13, 31, 24, STd);              // cylinder
            Fill(px, 23, 14, 30, 23, ST);
            Set(px, 26, 20, OL); Set(px, 27, 17, OL); Set(px, 25, 15, OL);
            Fill(px, 12, 14, 22, 19, ST);               // frame body
            Fill(px, 10, 22, 13, 26, STd);              // hammer
            Fill(px, 9, 10, 17, 16, WD); Fill(px, 7, 6, 16, 11, WD); Fill(px, 6, 3, 14, 7, WD); Fill(px, 6, 3, 8, 16, WK); // grip
            Ring(px, 14, 7, 22, 13, ST);                // trigger guard
            Fill(px, 16, 9, 18, 12, STd);               // trigger
            return Make(px);
        }

        public static Sprite DesertEagle()
        {
            var px = New();
            // long boxy slide
            Fill(px, 8, 20, 48, 27, BK); Fill(px, 8, 26, 48, 27, BKh); Fill(px, 8, 20, 48, 21, STd);
            for (int x = 11; x <= 18; x += 2) Fill(px, x, 22, x + 1, 26, BKh); // rear serrations
            Fill(px, 46, 19, 50, 27, BK); Fill(px, 48, 22, 50, 25, STd);       // squared muzzle
            Fill(px, 8, 16, 42, 20, BK);                // frame
            Fill(px, 14, 3, 26, 17, BK);                // big vertical grip
            Fill(px, 14, 3, 16, 17, BKh);
            for (int y = 6; y <= 13; y += 3) Fill(px, 16, y, 25, y + 1, STd); // grip checkering
            Ring(px, 24, 9, 34, 16, BK);                // trigger guard
            Fill(px, 28, 11, 30, 14, STd);              // trigger
            return Make(px);
        }

        public static Sprite Shotgun()
        {
            var px = New();
            // over/under double barrel
            Fill(px, 22, 22, 60, 25, STd); Fill(px, 22, 24, 60, 25, STh); // upper
            Fill(px, 22, 18, 60, 21, STd); Fill(px, 22, 20, 60, 21, STh); // lower
            Fill(px, 59, 17, 61, 26, ST);               // muzzles
            Fill(px, 14, 17, 24, 26, ST);               // breech/receiver
            Fill(px, 28, 15, 44, 18, WD); Fill(px, 28, 15, 44, 16, WK);   // wood forestock (pump)
            // wooden shoulder stock (angled down-back)
            Fill(px, 6, 12, 16, 18, WD); Fill(px, 2, 8, 12, 14, WD); Fill(px, 1, 5, 8, 10, WK); // butt
            Ring(px, 15, 11, 22, 17, ST);               // trigger guard
            Fill(px, 17, 13, 19, 16, STd);              // triggers
            return Make(px);
        }

        public static Sprite Sniper()
        {
            var px = New();
            // long thin barrel
            Fill(px, 26, 19, 61, 21, STd); Fill(px, 26, 20, 61, 21, STh);
            Fill(px, 60, 18, 62, 22, ST);               // muzzle
            Fill(px, 16, 16, 28, 23, BK);               // receiver
            // scope on top
            Fill(px, 20, 25, 40, 29, BK); Fill(px, 20, 28, 40, 29, BKh);
            Fill(px, 38, 25, 41, 29, GL);               // front lens
            Fill(px, 19, 25, 21, 29, GL);               // rear lens
            Fill(px, 24, 23, 26, 25, BK); Fill(px, 34, 23, 36, 25, BK);   // scope mounts
            Fill(px, 25, 21, 27, 24, STd);              // bolt handle
            // wooden stock (angled down-back)
            Fill(px, 6, 11, 18, 17, WD); Fill(px, 2, 7, 12, 13, WD); Fill(px, 1, 5, 7, 9, WK);
            Ring(px, 17, 11, 24, 17, BK);               // trigger guard
            Fill(px, 19, 13, 21, 16, STd);              // trigger
            return Make(px);
        }

        public static Sprite Steampunk()
        {
            var px = New();
            // flared bell muzzle (blunderbuss)
            Fill(px, 44, 17, 48, 25, BR);
            Fill(px, 48, 15, 52, 27, BR);
            Fill(px, 52, 12, 55, 30, BRh);
            Fill(px, 54, 12, 55, 30, BRd);
            // brass barrel
            Fill(px, 30, 19, 44, 23, BR); Fill(px, 30, 22, 44, 23, BRh);
            // copper boiler/tank
            Fill(px, 14, 9, 32, 25, CP); Fill(px, 15, 10, 31, 24, new Color(0.66f, 0.38f, 0.20f));
            for (int x = 16; x <= 30; x += 4) { Set(px, x, 11, BRd); Set(px, x, 23, BRd); } // rivets
            // gauge dial
            Fill(px, 19, 15, 27, 21, BRh); Fill(px, 20, 16, 26, 20, new Color(0.95f, 0.92f, 0.80f)); Set(px, 23, 18, OL); Set(px, 24, 19, OL);
            // copper pipe over the top
            Fill(px, 22, 25, 40, 27, CP);
            // wood grip
            Fill(px, 8, 4, 16, 14, WD); Fill(px, 8, 4, 10, 14, WK);
            Ring(px, 15, 8, 22, 14, BR);                // trigger guard
            Fill(px, 17, 10, 19, 13, BRd);              // trigger
            // steam puffs
            Set(px, 57, 30, STM); Set(px, 59, 31, STM); Set(px, 58, 28, STM); Set(px, 61, 29, STM);
            return Make(px);
        }

        // ---- helpers ----

        static Color32[] New() => new Color32[W * H]; // transparent

        static void Fill(Color32[] px, int x0, int y0, int x1, int y1, Color c)
        {
            for (int y = y0; y < y1; y++)
                for (int x = x0; x < x1; x++)
                    if (x >= 0 && x < W && y >= 0 && y < H) px[y * W + x] = c;
        }

        static void Set(Color32[] px, int x, int y, Color c)
        {
            if (x >= 0 && x < W && y >= 0 && y < H) px[y * W + x] = c;
        }

        /// <summary>A 1px-thick rectangle outline (for trigger guards) from (x0,y0) to (x1,y1) exclusive.</summary>
        static void Ring(Color32[] px, int x0, int y0, int x1, int y1, Color c)
        {
            Fill(px, x0, y0, x1, y0 + 1, c);      // bottom
            Fill(px, x0, y1 - 1, x1, y1, c);      // top
            Fill(px, x0, y0, x0 + 1, y1, c);      // left
            Fill(px, x1 - 1, y0, x1, y1, c);      // right
        }

        static Sprite Make(Color32[] px)
        {
            var tex = new Texture2D(W, H, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            tex.SetPixels32(px);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, W, H), new Vector2(0.5f, 0.5f), 100);
        }
    }
}
