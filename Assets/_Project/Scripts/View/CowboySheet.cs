using System;
using System.Collections.Generic;
using UnityEngine;

namespace HighNoon
{
    /// <summary>Per-cell pixel nudge for an atlas (same Resources path, <c>.json</c> sidecar).</summary>
    [Serializable]
    public class CowboyAtlasOffsets
    {
        public int cols;
        public int rows;
        public int[] dx;
        public int[] dy;

        public static CowboyAtlasOffsets Create(int cols, int rows)
        {
            int n = Mathf.Max(1, cols) * Mathf.Max(1, rows);
            return new CowboyAtlasOffsets { cols = cols, rows = rows, dx = new int[n], dy = new int[n] };
        }

        public void Ensure(int cols, int rows)
        {
            int n = Mathf.Max(1, cols) * Mathf.Max(1, rows);
            if (this.cols == cols && this.rows == rows && dx != null && dy != null && dx.Length == n && dy.Length == n)
                return;
            var ndx = new int[n];
            var ndy = new int[n];
            if (dx != null && dy != null)
            {
                int copy = Mathf.Min(Mathf.Min(dx.Length, dy.Length), n);
                for (int i = 0; i < copy; i++) { ndx[i] = dx[i]; ndy[i] = dy[i]; }
            }
            this.cols = cols;
            this.rows = rows;
            dx = ndx;
            dy = ndy;
        }

        public Vector2Int Get(int i)
        {
            int x = dx != null && i >= 0 && i < dx.Length ? dx[i] : 0;
            int y = dy != null && i >= 0 && i < dy.Length ? dy[i] : 0;
            return new Vector2Int(x, y);
        }

        public void Set(int i, int x, int y)
        {
            if (dx == null || dy == null || i < 0 || i >= dx.Length) return;
            dx[i] = x;
            dy[i] = y;
        }
    }

    /// <summary>
    /// Builds a <see cref="CowboyFrames"/> set from sprite sheets.
    /// Preferred: one atlas PNG (<see cref="CowboyCharacter.Atlas"/>) sliced on a uniform
    /// grid — each frame is a Sprite rect on the same texture (UV window, no extra files,
    /// no RenderTexture). Packed 6×4 file sequences and per-anim strips still work.
    /// </summary>
    public static class CowboySheet
    {
        static readonly Dictionary<string, CowboyFrames> Cache = new Dictionary<string, CowboyFrames>();

        /// <summary>Uniform grid over an atlas texture (shared by the loader and the editor tool).</summary>
        public struct AtlasGrid
        {
            public Texture2D Tex;
            public int Cols, Rows, CellW, CellH, X0, Top0;
            public int Count => Cols * Rows;

            public bool CellOf(int i, out int col, out int row)
            {
                col = Cols > 0 ? i % Cols : 0;
                row = Cols > 0 ? i / Cols : 0;
                return i >= 0 && i < Count;
            }

            /// <summary>Sprite rect — Unity origin at the texture's bottom-left.</summary>
            public Rect SpriteRect(int i)
            {
                CellOf(i, out int col, out int row);
                float unityY = Tex.height - (Top0 + row * CellH) - CellH;
                return new Rect(X0 + col * CellW, unityY, CellW, CellH);
            }

            /// <summary>Cell rect in image/GUI space — origin at the texture's top-left.</summary>
            public Rect GuiCell(int i)
            {
                CellOf(i, out int col, out int row);
                return new Rect(X0 + col * CellW, Top0 + row * CellH, CellW, CellH);
            }

            public Rect Uv(int i)
            {
                var r = SpriteRect(i);
                return new Rect(r.x / Tex.width, r.y / Tex.height, r.width / Tex.width, r.height / Tex.height);
            }
        }

        public static CowboyFrames Load(CowboyCharacter c)
        {
            if (c == null) return null;
            if (Cache.TryGetValue(c.Id, out var cached)) return cached;

            if (!string.IsNullOrEmpty(c.Atlas))
                return LoadAtlas(c);

            if (!string.IsNullOrEmpty(c.SheetBase))
                return LoadPacked(c);

            return LoadSequences(c);
        }

        public static void Invalidate(string id)
        {
            if (!string.IsNullOrEmpty(id)) Cache.Remove(id);
        }

        public struct AtlasClip
        {
            public string Name;
            public string Path;
            public int Cols, Rows;
        }

        /// <summary>Walk / idle / shoot / death atlases set on a character (editor tool + loader).</summary>
        public static List<AtlasClip> Clips(CowboyCharacter c)
        {
            var list = new List<AtlasClip>();
            if (c == null) return list;
            int cols = Mathf.Max(1, c.SheetCols);
            int rows = Mathf.Max(1, c.SheetRows);
            int deathCols = c.DeathCols > 0 ? c.DeathCols : cols;
            int deathRows = c.DeathRows > 0 ? c.DeathRows : rows;
            void add(string name, string path, int clipCols, int clipRows)
            {
                if (!string.IsNullOrEmpty(path))
                    list.Add(new AtlasClip { Name = name, Path = path, Cols = clipCols, Rows = clipRows });
            }
            add("Walk", c.Atlas, cols, rows);
            add("Idle", c.IdleAtlas, cols, rows);
            add("Shoot", c.ShootAtlas, cols, rows);
            add("Death", c.DeathAtlas, deathCols, deathRows);
            return list;
        }

        public static bool TryGrid(CowboyCharacter c, out AtlasGrid g)
        {
            if (c == null) { g = default; return false; }
            return TryGrid(c.Atlas, c.SheetCols, c.SheetRows, out g);
        }

        public static bool TryGrid(string atlas, int cols, int rows, out AtlasGrid g)
        {
            g = default;
            if (string.IsNullOrEmpty(atlas)) return false;
            var tex = Resources.Load<Texture2D>(atlas);
            if (tex == null) return false;
            tex.filterMode = FilterMode.Point;
            cols = Mathf.Max(1, cols);
            rows = Mathf.Max(1, rows);
            int cellW = tex.width / cols;
            int cellH = tex.height / rows;
            if (cellW < 1 || cellH < 1) return false;
            g = new AtlasGrid
            {
                Tex = tex, Cols = cols, Rows = rows,
                CellW = cellW, CellH = cellH,
                X0 = (tex.width % cols) / 2,
                Top0 = (tex.height % rows) / 2,
            };
            return true;
        }

        public static CowboyAtlasOffsets LoadOffsets(string atlas)
        {
            if (string.IsNullOrEmpty(atlas)) return null;
            var ta = Resources.Load<TextAsset>(atlas);
            if (ta == null || string.IsNullOrEmpty(ta.text)) return null;
            try { return JsonUtility.FromJson<CowboyAtlasOffsets>(ta.text); }
            catch { return null; }
        }

        /// <summary>One texture, uniform cols×rows. Sprite.Create rects = UV cells on that atlas.</summary>
        static CowboyFrames LoadAtlas(CowboyCharacter c)
        {
            int cols = Mathf.Max(1, c.SheetCols);
            int rows = Mathf.Max(1, c.SheetRows);
            var walk = LoadClip(c.Atlas, cols, rows, c.Height, c.FeetInset, c.Id + "_walk");
            if (walk == null)
            {
                Debug.LogWarning($"[CowboySheet] missing atlas Resources/{c.Atlas} — falling back for '{c.Id}'.");
                Cache[c.Id] = null;
                return null;
            }
            var idle  = LoadClip(c.IdleAtlas,  cols, rows, c.Height, c.FeetInset, c.Id + "_idle")
                        ?? LastColumn(walk, cols, rows);
            var shoot = LoadClip(c.ShootAtlas, cols, rows, c.Height, c.FeetInset, c.Id + "_shoot") ?? idle;
            int deathCols = c.DeathCols > 0 ? c.DeathCols : cols;
            int deathRows = c.DeathRows > 0 ? c.DeathRows : rows;
            var death = LoadClip(c.DeathAtlas, deathCols, deathRows, c.Height, c.FeetInset, c.Id + "_death") ?? idle;
            var frames = new CowboyFrames
            {
                Idle  = idle,
                Ready = idle,
                Shoot = shoot,
                Death = death,
                Walk  = walk,
            };
            Cache[c.Id] = frames;
            return frames;
        }

        static Sprite[] LoadClip(string atlas, int cols, int rows, float height, float feetInset, string prefix)
        {
            if (!TryGrid(atlas, cols, rows, out var g)) return null;
            float ppu = g.CellH / Mathf.Max(0.01f, height);
            var off = LoadOffsets(atlas);
            if (off != null) off.Ensure(g.Cols, g.Rows);
            var cells = new Sprite[g.Count];
            for (int i = 0; i < g.Count; i++)
            {
                var n = off != null ? off.Get(i) : Vector2Int.zero;
                cells[i] = Make(g.Tex, g.SpriteRect(i), $"{prefix}_{i:00}", ppu, feetInset, n.x, n.y);
            }
            return cells;
        }

        static CowboyFrames LoadPacked(CowboyCharacter c)
        {
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
                cells[i] = Make(tex, $"{c.Id}_{i:00}", ppu, c.FeetInset);
            }

            var packedIdle = Row(cells, cols, 0);
            var frames = new CowboyFrames
            {
                Idle  = packedIdle,
                Ready = packedIdle,
                Shoot = Row(cells, cols, 1),
                Death = Row(cells, cols, 2),
                Walk  = LoadSeqN(c.WalkBase, ppu, c.FeetInset) ?? packedIdle,
            };
            Cache[c.Id] = frames;
            return frames;
        }

        static CowboyFrames LoadSequences(CowboyCharacter c)
        {
            string probe = FirstPresent(c.IdleBase, c.WalkBase, c.ShootBase, c.DeathBase);
            if (probe == null)
            {
                Cache[c.Id] = null;
                return null;
            }
            var probeTex = Resources.Load<Texture2D>($"{probe}_00");
            if (probeTex == null)
            {
                Debug.LogWarning($"[CowboySheet] missing Resources/{probe}_00 — falling back for '{c.Id}'.");
                Cache[c.Id] = null;
                return null;
            }
            probeTex.filterMode = FilterMode.Point;
            float ppu = probeTex.height / Mathf.Max(0.01f, c.Height);

            var idle  = LoadSeqN(c.IdleBase,  ppu, c.FeetInset);
            var walk  = LoadSeqN(c.WalkBase,  ppu, c.FeetInset);
            var shoot = LoadSeqN(c.ShootBase, ppu, c.FeetInset);
            var death = LoadSeqN(c.DeathBase, ppu, c.FeetInset);
            idle ??= walk;
            walk ??= idle;
            if (idle == null)
            {
                Cache[c.Id] = null;
                return null;
            }
            var frames = new CowboyFrames
            {
                Idle  = idle,
                Ready = idle,
                Shoot = shoot ?? idle,
                Death = death ?? idle,
                Walk  = walk,
            };
            Cache[c.Id] = frames;
            return frames;
        }

        static string FirstPresent(params string[] bases)
        {
            foreach (var b in bases)
            {
                if (string.IsNullOrEmpty(b)) continue;
                if (Resources.Load<Texture2D>($"{b}_00") != null) return b;
            }
            return null;
        }

        static Sprite Make(Texture2D tex, string name, float ppu, float feetInset)
            => Make(tex, new Rect(0, 0, tex.width, tex.height), name, ppu, feetInset);

        static Sprite Make(Texture2D tex, Rect rect, string name, float ppu, float feetInset, int dx = 0, int dy = 0)
        {
            tex.filterMode = FilterMode.Point;
            float px = 0.5f - (rect.width > 0f ? dx / rect.width : 0f);
            float py = feetInset - (rect.height > 0f ? dy / rect.height : 0f);
            var s = Sprite.Create(tex, rect, new Vector2(px, py), ppu, 0, SpriteMeshType.FullRect);
            s.name = name;
            s.hideFlags = HideFlags.HideAndDontSave;
            return s;
        }

        static Sprite[] LoadSeqN(string pathBase, float ppu, float feetInset, int max = 16)
        {
            if (string.IsNullOrEmpty(pathBase)) return null;
            var list = new List<Sprite>();
            for (int i = 0; i < max; i++)
            {
                var tex = Resources.Load<Texture2D>($"{pathBase}_{i:00}");
                if (tex == null) break;
                list.Add(Make(tex, $"{pathBase}_{i:00}", ppu, feetInset));
            }
            return list.Count > 0 ? list.ToArray() : null;
        }

        static Sprite[] Row(Sprite[] cells, int cols, int row)
        {
            var a = new Sprite[cols];
            for (int col = 0; col < cols; col++) a[col] = cells[row * cols + col];
            return a;
        }

        static Sprite[] LastColumn(Sprite[] cells, int cols, int rows)
        {
            var a = new Sprite[rows];
            for (int r = 0; r < rows; r++) a[r] = cells[r * cols + (cols - 1)];
            return a;
        }
    }
}
