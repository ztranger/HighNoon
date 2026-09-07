using UnityEngine;

namespace HighNoon
{
    /// <summary>
    /// Procedurally drawn pixel props for arena backgrounds. Point-filtered,
    /// transparent, pivoted near the base so they sit on the ground.
    /// </summary>
    public static class PropArt
    {
        static Texture2D NewTex(int w, int h)
        {
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            return tex;
        }

        static Sprite Finish(Texture2D tex, Color32[] px, int ppu)
        {
            tex.SetPixels32(px);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.08f), ppu);
        }

        static Color32[] Clear(int w, int h)
        {
            var px = new Color32[w * h];
            for (int i = 0; i < px.Length; i++) px[i] = new Color32(0, 0, 0, 0);
            return px;
        }

        public static Sprite Cactus(int ppu = 16)
        {
            int w = 16, h = 28; var px = Clear(w, h);
            Color32 g = new Color32(74, 112, 60, 255), gd = new Color32(50, 82, 42, 255), gl = new Color32(98, 136, 80, 255);
            void S(int x, int y, Color32 c) { if (x >= 0 && y >= 0 && x < w && y < h) px[y * w + x] = c; }
            void F(int x0, int y0, int x1, int y1, Color32 c) { for (int y = y0; y <= y1; y++) for (int x = x0; x <= x1; x++) S(x, y, c); }
            F(6, 0, 9, 25, g); F(9, 0, 9, 25, gd); F(6, 0, 6, 25, gl);   // trunk
            F(2, 12, 6, 14, g); F(2, 14, 4, 20, g); F(2, 14, 2, 20, gl); // left arm
            F(9, 16, 14, 18, g); F(12, 18, 14, 24, g); F(14, 18, 14, 24, gd); // right arm
            for (int y = 3; y < 24; y += 4) { S(7, y, gd); S(8, y, gl); }        // spines
            return Finish(NewTex(w, h), px, ppu);
        }

        public static Sprite Rock(int ppu = 16)
        {
            int w = 16, h = 10; var px = Clear(w, h);
            Color32 b = new Color32(122, 116, 108, 255), l = new Color32(154, 148, 140, 255), d = new Color32(86, 80, 74, 255);
            void S(int x, int y, Color32 c) { if (x >= 0 && y >= 0 && x < w && y < h) px[y * w + x] = c; }
            void F(int x0, int y0, int x1, int y1, Color32 c) { for (int y = y0; y <= y1; y++) for (int x = x0; x <= x1; x++) S(x, y, c); }
            F(3, 0, 12, 1, d); F(1, 2, 14, 5, b); F(2, 6, 13, 7, b); F(4, 8, 11, 8, l); F(6, 9, 9, 9, l);
            F(2, 5, 6, 6, l);   // top-left highlight
            return Finish(NewTex(w, h), px, ppu);
        }

        public static Sprite Barrel(int ppu = 16)
        {
            int w = 14, h = 18; var px = Clear(w, h);
            Color32 wood = new Color32(142, 98, 58, 255), woodD = new Color32(104, 70, 40, 255), hoop = new Color32(70, 50, 30, 255), top = new Color32(122, 84, 50, 255);
            void S(int x, int y, Color32 c) { if (x >= 0 && y >= 0 && x < w && y < h) px[y * w + x] = c; }
            void F(int x0, int y0, int x1, int y1, Color32 c) { for (int y = y0; y <= y1; y++) for (int x = x0; x <= x1; x++) S(x, y, c); }
            F(2, 1, 11, 16, wood); F(9, 1, 11, 16, woodD);
            F(2, 3, 11, 3, hoop); F(2, 8, 11, 8, hoop); F(2, 13, 11, 13, hoop);
            F(3, 16, 10, 17, top);
            return Finish(NewTex(w, h), px, ppu);
        }

        public static Sprite Skull(int ppu = 16)
        {
            int w = 18, h = 14; var px = Clear(w, h);
            Color32 bone = new Color32(226, 216, 198, 255), boneD = new Color32(182, 170, 152, 255), dark = new Color32(40, 34, 28, 255);
            void S(int x, int y, Color32 c) { if (x >= 0 && y >= 0 && x < w && y < h) px[y * w + x] = c; }
            void F(int x0, int y0, int x1, int y1, Color32 c) { for (int y = y0; y <= y1; y++) for (int x = x0; x <= x1; x++) S(x, y, c); }
            // horns
            F(0, 9, 3, 11, boneD); F(0, 11, 1, 13, boneD);
            F(14, 9, 17, 11, boneD); F(16, 11, 17, 13, boneD);
            // cranium + snout
            F(4, 3, 13, 11, bone); F(6, 0, 11, 3, bone);
            F(4, 3, 4, 11, boneD); F(13, 3, 13, 11, boneD);
            // eyes + nose
            F(5, 6, 7, 8, dark); F(10, 6, 12, 8, dark); F(8, 1, 9, 3, dark);
            return Finish(NewTex(w, h), px, ppu);
        }

        public static Sprite GrassTuft(int ppu = 16)
        {
            int w = 12, h = 10; var px = Clear(w, h);
            Color32 g = new Color32(98, 122, 58, 255), gd = new Color32(66, 92, 40, 255);
            void S(int x, int y, Color32 c) { if (x >= 0 && y >= 0 && x < w && y < h) px[y * w + x] = c; }
            void Blade(int x, int top, Color32 c) { for (int y = 0; y <= top; y++) S(x, y, c); }
            Blade(2, 6, gd); Blade(3, 8, g); Blade(5, 5, gd); Blade(6, 9, g); Blade(8, 7, gd); Blade(9, 6, g);
            return Finish(NewTex(w, h), px, ppu);
        }

        public static Sprite FencePost(int ppu = 16)
        {
            int w = 10, h = 20; var px = Clear(w, h);
            Color32 wood = new Color32(122, 92, 58, 255), woodD = new Color32(92, 66, 40, 255);
            void S(int x, int y, Color32 c) { if (x >= 0 && y >= 0 && x < w && y < h) px[y * w + x] = c; }
            void F(int x0, int y0, int x1, int y1, Color32 c) { for (int y = y0; y <= y1; y++) for (int x = x0; x <= x1; x++) S(x, y, c); }
            F(3, 0, 6, 17, wood); F(6, 0, 6, 17, woodD); F(4, 17, 5, 19, woodD); // post + cap
            F(0, 9, 9, 10, wood); F(0, 12, 9, 13, wood);                          // rails
            return Finish(NewTex(w, h), px, ppu);
        }

        public static Sprite Plank(int ppu = 16)
        {
            int w = 24, h = 8; var px = Clear(w, h);
            Color32 wood = new Color32(136, 100, 62, 255), grain = new Color32(104, 74, 44, 255), grainL = new Color32(152, 114, 74, 255);
            void S(int x, int y, Color32 c) { if (x >= 0 && y >= 0 && x < w && y < h) px[y * w + x] = c; }
            void F(int x0, int y0, int x1, int y1, Color32 c) { for (int y = y0; y <= y1; y++) for (int x = x0; x <= x1; x++) S(x, y, c); }
            F(0, 0, 23, 7, wood);
            F(0, 0, 23, 0, grain); F(0, 7, 23, 7, grain);
            F(0, 3, 23, 3, grainL); F(7, 1, 7, 6, grain); F(16, 1, 16, 6, grain); // seams
            return Finish(NewTex(w, h), px, ppu);
        }

        public static Sprite Tombstone(int ppu = 16)
        {
            int w = 14, h = 18; var px = Clear(w, h);
            Color32 b = new Color32(150, 150, 150, 255), d = new Color32(112, 112, 112, 255), l = new Color32(182, 182, 182, 255), dark = new Color32(70, 70, 70, 255);
            void S(int x, int y, Color32 c) { if (x >= 0 && y >= 0 && x < w && y < h) px[y * w + x] = c; }
            void F(int x0, int y0, int x1, int y1, Color32 c) { for (int y = y0; y <= y1; y++) for (int x = x0; x <= x1; x++) S(x, y, c); }
            F(2, 0, 11, 13, b);                                   // body
            F(3, 13, 10, 13, b); F(4, 14, 9, 14, b); F(5, 15, 8, 15, b); // rounded top
            F(11, 0, 11, 13, d); F(2, 0, 2, 13, l);               // shade + highlight
            F(6, 4, 7, 11, dark); F(4, 8, 9, 9, dark);            // cross engraving
            return Finish(NewTex(w, h), px, ppu);
        }

        public static Sprite DeadTree(int ppu = 16)
        {
            int w = 20, h = 30; var px = Clear(w, h);
            Color32 b = new Color32(96, 72, 50, 255), d = new Color32(68, 50, 34, 255);
            void S(int x, int y, Color32 c) { if (x >= 0 && y >= 0 && x < w && y < h) px[y * w + x] = c; }
            void F(int x0, int y0, int x1, int y1, Color32 c) { for (int y = y0; y <= y1; y++) for (int x = x0; x <= x1; x++) S(x, y, c); }
            void Line(int x0, int y0, int x1, int y1, Color32 c)
            {
                int dx = Mathf.Abs(x1 - x0), dy = Mathf.Abs(y1 - y0);
                int sx = x0 < x1 ? 1 : -1, sy = y0 < y1 ? 1 : -1, err = dx - dy, x = x0, y = y0;
                while (true)
                {
                    S(x, y, c); S(x + 1, y, c);
                    if (x == x1 && y == y1) break;
                    int e2 = 2 * err;
                    if (e2 > -dy) { err -= dy; x += sx; }
                    if (e2 < dx) { err += dx; y += sy; }
                }
            }
            F(8, 0, 11, 21, b); F(11, 0, 11, 21, d);   // trunk
            Line(9, 14, 3, 22, b); Line(3, 22, 2,27, b);   // left branch
            Line(11, 16, 16, 23, b); Line(16, 23, 17, 28, b); // right branch
            Line(10, 20, 10, 27, b);                        // top twig
            return Finish(NewTex(w, h), px, ppu);
        }

        public static Sprite HayBale(int ppu = 16)
        {
            int w = 18, h = 14; var px = Clear(w, h);
            Color32 hay = new Color32(206, 178, 96, 255), hd = new Color32(176, 148, 74, 255), bind = new Color32(150, 120, 60, 255);
            void S(int x, int y, Color32 c) { if (x >= 0 && y >= 0 && x < w && y < h) px[y * w + x] = c; }
            void F(int x0, int y0, int x1, int y1, Color32 c) { for (int y = y0; y <= y1; y++) for (int x = x0; x <= x1; x++) S(x, y, c); }
            F(1, 1, 16, 12, hay);
            F(1, 1, 16, 2, hd); F(1, 11, 16, 12, hd); // top/bottom shading
            F(5, 1, 5, 12, bind); F(12, 1, 12, 12, bind); // bindings
            return Finish(NewTex(w, h), px, ppu);
        }
    }
}
