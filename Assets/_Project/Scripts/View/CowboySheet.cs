using System.Collections.Generic;
using UnityEngine;

namespace HighNoon
{
    /// <summary>
    /// Builds a <see cref="CowboyFrames"/> set from a hand-drawn 6x4 sprite sheet
    /// (see HIGH_NOON_INTEGRATION.md): row 0 idle, row 1 shoot (muzzle flash baked on
    /// frame 3), row 2 death, row 3 idle-left (unused — we mirror with flipX). Frames come
    /// from the pre-cut, uniform-cell PNGs "{SheetBase}_00".."_NN" so ragged padding on the
    /// master sheet can't shift the grid. Sprites pivot at the feet (bottom-centre of the cell)
    /// and are normalized to <see cref="CowboyCharacter.Height"/>. Session-cached; returns
    /// <c>null</c> when frames are missing so the caller can fall back to a static pose.
    /// </summary>
    public static class CowboySheet
    {
        static readonly Dictionary<string, CowboyFrames> Cache = new Dictionary<string, CowboyFrames>();

        public static CowboyFrames Load(CowboyCharacter c)
        {
            if (c == null || string.IsNullOrEmpty(c.SheetBase)) return null;
            if (Cache.TryGetValue(c.Id, out var cached)) return cached;

            int cols = c.SheetCols, rows = c.SheetRows;
            var cells = new Sprite[cols * rows];
            float ppu = 0f;

            for (int i = 0; i < cells.Length; i++)
            {
                var tex = Resources.Load<Texture2D>($"{c.SheetBase}_{i:00}");
                if (tex == null)
                {
                    Debug.LogWarning($"[CowboySheet] missing frame Resources/{c.SheetBase}_{i:00} — falling back for '{c.Id}'.");
                    Cache[c.Id] = null;
                    return null;
                }
                tex.filterMode = FilterMode.Point;
                if (ppu == 0f) ppu = tex.height / Mathf.Max(0.01f, c.Height);
                var s = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height),
                    new Vector2(0.5f, c.FeetInset), ppu, 0, SpriteMeshType.FullRect);
                s.name = $"{c.Id}_{i:00}";
                s.hideFlags = HideFlags.HideAndDontSave;
                cells[i] = s;
            }

            var frames = new CowboyFrames
            {
                Idle  = Row(cells, cols, 0),
                Ready = Row(cells, cols, 0), // hold in idle until the shot
                Shoot = Row(cells, cols, 1),
                Death = Row(cells, cols, 2),
            };
            Cache[c.Id] = frames;
            return frames;
        }

        static Sprite[] Row(Sprite[] cells, int cols, int row)
        {
            var a = new Sprite[cols];
            for (int col = 0; col < cols; col++) a[col] = cells[row * cols + col];
            return a;
        }
    }
}
