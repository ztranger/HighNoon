using UnityEngine;

namespace HighNoon
{
    /// <summary>
    /// Code-drawn placeholder pixel sprites so the game is playable before real
    /// art exists. Point-filtered to keep the crisp pixel look. Swap for imported
    /// Aseprite sprite sheets later without touching gameplay code.
    /// </summary>
    public static class PlaceholderArt
    {
        public static Sprite SolidSprite(Color c, int w = 4, int h = 4, int ppu = 4)
        {
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            var px = new Color32[w * h];
            Color32 c32 = c;
            for (int i = 0; i < px.Length; i++) px[i] = c32;
            tex.SetPixels32(px);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), ppu);
        }

        /// <summary>Tiny top-down-ish cowboy silhouette (hat + body), pivot centered.</summary>
        public static Sprite Cowboy(Color body, int ppu = 16)
        {
            const int w = 16, h = 20;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            var px = new Color32[w * h];
            var clear = new Color32(0, 0, 0, 0);
            for (int i = 0; i < px.Length; i++) px[i] = clear;

            Color32 bc = body;
            Color32 dark = new Color32((byte)(body.r * 160), (byte)(body.g * 160), (byte)(body.b * 160), 255);

            void Set(int x, int y, Color32 c) { if (x >= 0 && y >= 0 && x < w && y < h) px[y * w + x] = c; }
            void Fill(int x0, int y0, int x1, int y1, Color32 c)
            {
                for (int y = y0; y <= y1; y++)
                    for (int x = x0; x <= x1; x++)
                        Set(x, y, c);
            }

            Fill(5, 2, 10, 12, bc);    // torso / legs
            Fill(3, 12, 12, 14, dark); // shoulders
            Fill(6, 14, 9, 16, bc);    // head
            Fill(4, 16, 11, 17, dark); // hat brim
            Fill(6, 17, 9, 19, dark);  // hat top

            tex.SetPixels32(px);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), ppu);
        }
    }
}
