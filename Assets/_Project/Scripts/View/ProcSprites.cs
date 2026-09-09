using UnityEngine;

namespace HighNoon
{
    /// <summary>
    /// Point-filtered runtime sprites. Cached art (cowboy looks, weapons, props)
    /// lives for the session via static dictionaries — these are not scene objects
    /// (<see cref="HideFlags.HideAndDontSave"/>). Uncached callers must
    /// <see cref="Destroy"/>.
    /// </summary>
    public static class ProcSprites
    {
        public static Sprite Make(Color32[] px, int w, int h, Vector2 pivot, float ppu,
            FilterMode filter = FilterMode.Point)
        {
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = filter };
            tex.hideFlags = HideFlags.HideAndDontSave;
            tex.SetPixels32(px);
            tex.Apply(false, false); // keep readable (IconArt samples cowboy pixels)
            var sprite = Sprite.Create(tex, new Rect(0, 0, w, h), pivot, ppu);
            sprite.hideFlags = HideFlags.HideAndDontSave;
            return sprite;
        }

        public static void Destroy(Sprite sprite)
        {
            if (sprite == null) return;
            var tex = sprite.texture;
            UnityEngine.Object.Destroy(sprite);
            if (tex != null) UnityEngine.Object.Destroy(tex);
        }
    }
}
