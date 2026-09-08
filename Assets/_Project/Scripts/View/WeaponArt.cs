using UnityEngine;

namespace HighNoon
{
    /// <summary>Procedural pixel weapon icons (Point-filtered) for the menu. One revolver for now;
    /// tint per weapon so each pick reads a little differently until bespoke icons exist.</summary>
    public static class WeaponArt
    {
        static readonly Color OL = new Color(0.08f, 0.07f, 0.07f, 1f);
        static readonly Color MT = new Color(0.46f, 0.49f, 0.56f, 1f);
        static readonly Color MD = new Color(0.27f, 0.29f, 0.35f, 1f);
        static readonly Color MH = new Color(0.64f, 0.67f, 0.74f, 1f);
        static readonly Color WD = new Color(0.50f, 0.31f, 0.17f, 1f);
        static readonly Color WK = new Color(0.36f, 0.21f, 0.11f, 1f);

        /// <summary>A side-view revolver pointing right. <paramref name="metal"/> tints the barrel/frame.</summary>
        public static Sprite Revolver(Color metal)
        {
            int W = 48, H = 30;
            var px = new Color32[W * H]; // transparent
            Color mt = metal, md = metal * 0.6f, mh = Color.Lerp(metal, Color.white, 0.35f);
            md.a = 1f; mh.a = 1f;

            // Barrel.
            Fill(px, W, 14, 18, 40, 23, mt);
            Fill(px, W, 14, 22, 40, 23, mh);       // top highlight
            Fill(px, W, 14, 18, 40, 19, md);       // bottom shade
            Fill(px, W, 39, 18, 41, 24, md);       // muzzle
            Fill(px, W, 37, 23, 39, 24, mt);       // front sight
            // Top frame.
            Fill(px, W, 10, 16, 24, 18, mt);
            // Cylinder.
            Fill(px, W, 21, 12, 30, 24, md);
            Fill(px, W, 22, 13, 29, 23, mt);
            Set(px, W, 25, 20, OL); Set(px, W, 26, 18, OL); Set(px, W, 24, 16, OL);
            // Frame body.
            Fill(px, W, 10, 13, 22, 18, mt);
            // Hammer.
            Fill(px, W, 8, 22, 11, 26, md);
            // Grip (leaning back-down).
            Fill(px, W, 8, 10, 16, 15, WD);
            Fill(px, W, 6, 6, 15, 11, WD);
            Fill(px, W, 4, 2, 13, 7, WD);
            Fill(px, W, 4, 2, 6, 15, WK);          // left shade
            // Trigger guard.
            Fill(px, W, 12, 11, 20, 12, mt);       // top
            Fill(px, W, 12, 6, 13, 12, mt);        // front
            Fill(px, W, 19, 6, 20, 12, mt);        // back
            Fill(px, W, 12, 6, 20, 7, mt);         // bottom
            Fill(px, W, 15, 8, 17, 11, md);        // trigger

            var tex = new Texture2D(W, H, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            tex.SetPixels32(px);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, W, H), new Vector2(0.5f, 0.5f), 100);
        }

        static void Fill(Color32[] px, int W, int x0, int y0, int x1, int y1, Color c)
        {
            for (int y = y0; y < y1; y++)
                for (int x = x0; x < x1; x++)
                    if (x >= 0 && x < W && y >= 0 && y * W + x < px.Length) px[y * W + x] = c;
        }

        static void Set(Color32[] px, int W, int x, int y, Color c)
        {
            if (x >= 0 && x < W && y >= 0 && y * W + x < px.Length) px[y * W + x] = c;
        }
    }
}
