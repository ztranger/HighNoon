using System.Collections.Generic;
using UnityEngine;

namespace HighNoon
{
    /// <summary>
    /// Loads a hand-drawn cowboy PNG from Resources and builds a normalized sprite:
    /// pivot at the feet (0.5, 0) so the figure plants on the street, and a pixels-per-unit
    /// chosen so every character ends up the same on-screen height regardless of source size.
    /// Session-cached. Returns <c>null</c> when the art is missing so the caller can fall back
    /// to the procedural <see cref="CowboyArt"/> cowboy.
    /// </summary>
    public static class CowboySprites
    {
        static readonly Dictionary<string, Sprite> Cache = new Dictionary<string, Sprite>();

        public static Sprite Load(CowboyCharacter c)
        {
            if (c == null) return null;
            if (Cache.TryGetValue(c.Id, out var cached)) return cached;

            var tex = Resources.Load<Texture2D>(c.ResourcePath);
            if (tex == null)
            {
                // Imported as Multiple-mode sprite? Grab the texture off the sub-asset.
                var spr = Resources.Load<Sprite>(c.ResourcePath);
                if (spr != null) tex = spr.texture;
            }
            if (tex == null)
            {
                Debug.LogWarning($"[CowboySprites] missing art at Resources/{c.ResourcePath} — using procedural cowboy.");
                Cache[c.Id] = null;
                return null;
            }

            tex.filterMode = FilterMode.Point; // keep the pixel edges crisp
            float ppu = tex.height / Mathf.Max(0.01f, c.Height);
            var sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height),
                new Vector2(0.5f, 0f), ppu, 0, SpriteMeshType.FullRect);
            sprite.name = c.Id;
            sprite.hideFlags = HideFlags.HideAndDontSave;
            Cache[c.Id] = sprite;
            return sprite;
        }
    }
}
