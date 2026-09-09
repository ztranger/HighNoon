using System.Collections.Generic;
using UnityEngine;

namespace HighNoon
{
    /// <summary>
    /// Builds a duel arena as a landscape *side-view* showdown: a gradient sky, a low
    /// sun/moon sitting on the horizon behind the standoff, a distant silhouette
    /// (hills / buttes / rooftops by style), a textured ground plane, a packed-dirt
    /// street where the gunslingers stand, and a few foreground props.
    /// Each arena keeps its own palette so the eleven locations stay distinct.
    /// </summary>
    public static class BackgroundBuilder
    {
        /// <summary>World Y where the sky meets the land. Cowboys stand at y≈0 (feet just
        /// below this), so they read as standing on the near street. Tumbleweed uses it too.</summary>
        public const float Horizon = -0.9f;

        static readonly Dictionary<string, Sprite> Grounds = new Dictionary<string, Sprite>();
        static readonly Dictionary<string, Sprite> Silhouettes = new Dictionary<string, Sprite>();

        public static GameObject Build(ArenaDef def)
        {
            var root = new GameObject("BackgroundRoot");

            var cam = Camera.main;
            float halfH = cam != null ? cam.orthographicSize : 4.4f;
            float halfW = cam != null ? halfH * Mathf.Max(1f, cam.aspect) : 7.82f;
            float top = halfH + 1f;
            float bottom = -halfH - 1f;
            float coverW = halfW * 2f + 2f;

            // Palette — explicit per arena, with sensible fallbacks derived from the ground.
            Color skyTop = def.SkyTop ?? Dim(def.GroundBase, 0.55f);
            Color skyHor = def.SkyHorizon ?? Color.Lerp(def.GroundBase, Color.white, 0.45f);
            Color sunCol = def.Sun ?? Color.Lerp(skyHor, Color.white, 0.5f);
            Color hillCol = def.Hill ?? def.GroundShade;

            // 1. Sky — vertical gradient filling everything above the horizon.
            float skyH = top - Horizon;
            AddLayer(root, "Sky", VGradient(skyHor, skyTop), coverW, skyH, 0f, (Horizon + top) * 0.5f, -120);

            // 2. Sun / moon — a low disc with a soft glow, sitting on the horizon behind the duel.
            AddLayer(root, "Sun", SunDisc(sunCol), 3.4f, 3.4f, 0f, Horizon + 0.1f, -118);

            // 3. Distant silhouette — hills / buttes / rooftops, rising from the horizon.
            var sil = BuildSilhouette(def, coverW, hillCol);
            var silGo = new GameObject("Silhouette");
            silGo.transform.SetParent(root.transform);
            silGo.transform.position = new Vector3(0f, Horizon - 0.12f, 0f);
            silGo.transform.localScale = Vector3.one;
            var silSr = silGo.AddComponent<SpriteRenderer>();
            silSr.sprite = sil;
            silSr.sortingOrder = -110;

            // 4. Ground — textured plane filling everything below the horizon.
            float groundH = Horizon - bottom;
            AddLayer(root, "Ground", BuildGround(def), coverW, groundH, 0f, (Horizon + bottom) * 0.5f, -100);

            // 5. Street — a soft packed-dirt band where the gunslingers stand.
            Color street = def.Divider.a > 0.02f
                ? new Color(def.Divider.r, def.Divider.g, def.Divider.b, Mathf.Min(0.35f, def.Divider.a))
                : new Color(0f, 0f, 0f, 0.12f);
            AddLayer(root, "Street", VFeather(street), coverW, 2.6f, 0f, -1.7f, -95);

            PlaceProps(def, root.transform);
            return root;
        }

        // ---------- layers ----------

        static GameObject AddLayer(GameObject root, string name, Sprite sp, float worldW, float worldH, float cx, float cy, int order)
        {
            var go = new GameObject(name);
            go.transform.SetParent(root.transform);
            go.transform.position = new Vector3(cx, cy, 0f);
            var b = sp.bounds.size;
            go.transform.localScale = new Vector3(worldW / b.x, worldH / b.y, 1f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sp;
            sr.sortingOrder = order;
            return go;
        }

        /// <summary>A 1×128 vertical gradient (bottom → top). Bilinear so it reads smooth when stretched.</summary>
        static Sprite VGradient(Color bottom, Color top)
        {
            const int H = 128;
            var px = new Color32[H];
            for (int y = 0; y < H; y++) px[y] = Color.Lerp(bottom, top, y / (float)(H - 1));
            return ProcSprites.Make(px, 1, H, new Vector2(0.5f, 0.5f), 100, FilterMode.Bilinear);
        }

        /// <summary>A 1×64 band: the colour in the middle, fading to transparent at both edges.</summary>
        static Sprite VFeather(Color mid)
        {
            const int H = 64; float c = (H - 1) * 0.5f;
            var px = new Color32[H];
            for (int y = 0; y < H; y++)
            {
                float t = Mathf.Abs(y - c) / c;          // 0 centre → 1 edge
                Color col = mid; col.a = mid.a * (1f - t) * (1f - t);
                px[y] = col;
            }
            return ProcSprites.Make(px, 1, H, new Vector2(0.5f, 0.5f), 100, FilterMode.Bilinear);
        }

        /// <summary>Low sun/moon: a bright disc that fades out into a soft glow halo.</summary>
        static Sprite SunDisc(Color core)
        {
            const int S = 128; float c = (S - 1) * 0.5f;
            float disc = S * 0.30f, glow = S * 0.5f;
            var px = new Color32[S * S];
            Color coreLt = Color.Lerp(core, Color.white, 0.4f);
            for (int y = 0; y < S; y++)
                for (int x = 0; x < S; x++)
                {
                    float d = Mathf.Sqrt((x - c) * (x - c) + (y - c) * (y - c));
                    Color col;
                    if (d <= disc) { float t = d / disc; col = Color.Lerp(coreLt, core, t * t); col.a = 1f; }
                    else if (d < glow) { float t = (d - disc) / (glow - disc); col = core; col.a = (1f - t) * (1f - t) * 0.5f; }
                    else col = new Color(0, 0, 0, 0);
                    px[y * S + x] = col;
                }
            return ProcSprites.Make(px, S, S, new Vector2(0.5f, 0.5f), 100, FilterMode.Bilinear);
        }

        // ---------- ground ----------

        static Sprite BuildGround(ArenaDef def)
        {
            string key = def != null ? def.Name : "?";
            if (Grounds.TryGetValue(key, out var cached)) return cached;

            const int T = 128;
            var px = new Color32[T * T];
            int seedInt = def.Name.GetHashCode();
            float seed = (seedInt % 1000) * 0.01f;
            var rnd = new System.Random(seedInt);

            for (int y = 0; y < T; y++)
                for (int x = 0; x < T; x++)
                {
                    float n = Mathf.PerlinNoise(x * 0.05f + seed, y * 0.05f + seed);
                    // A gentle top-to-bottom lift: the ground near the horizon catches more light.
                    float lift = Mathf.Lerp(0.10f, -0.02f, y / (float)(T - 1));
                    Color c = Color.Lerp(def.GroundShade, def.GroundBase, Mathf.Clamp01(n + lift));
                    double r = rnd.NextDouble();
                    if (r < 0.03) c = def.Speck;                              // pebbles
                    else if (r < 0.06) c = Color.Lerp(c, Color.white, 0.15f); // light fleck
                    px[y * T + x] = c;
                }

            var sprite = ProcSprites.Make(px, T, T, new Vector2(0.5f, 0.5f), 100);
            Grounds[key] = sprite;
            return sprite;
        }

        // ---------- distant silhouette ----------

        static Sprite BuildSilhouette(ArenaDef def, float coverW, Color hill)
        {
            const int PPU = 24;
            int w = Mathf.Clamp(Mathf.CeilToInt(coverW * PPU), 128, 1400);
            int h = Mathf.CeilToInt(2.4f * PPU); // ≈ 2.4 world units tall
            string key = $"{def.Name}_{w}_{h}";
            if (Silhouettes.TryGetValue(key, out var cached)) return cached;

            var px = new Color32[w * h];
            for (int i = 0; i < px.Length; i++) px[i] = new Color32(0, 0, 0, 0);
            Color32 hc = hill;
            Color32 hcRim = Color.Lerp(hill, Color.white, 0.10f);

            void Col(int x, int topY)   // fill a column from the bottom up to topY (inclusive)
            {
                if (x < 0 || x >= w) return;
                topY = Mathf.Clamp(topY, 0, h - 1);
                for (int y = 0; y <= topY; y++) px[y * w + x] = (y == topY) ? hcRim : hc;
            }
            void Block(int x0, int x1, int topY)
            {
                for (int x = x0; x <= x1; x++) Col(x, topY);
            }

            var rnd = new System.Random(def.Name.GetHashCode() ^ 0x51ED);
            int baseY = Mathf.RoundToInt(h * 0.30f);   // rolling-hill baseline
            float ph1 = (float)rnd.NextDouble() * 6.28f, ph2 = (float)rnd.NextDouble() * 6.28f;

            // Rolling ridge for every arena.
            for (int x = 0; x < w; x++)
            {
                float u = x / (float)PPU;
                float ridge = Mathf.Sin(u * 0.55f + ph1) * 0.5f + Mathf.Sin(u * 1.30f + ph2) * 0.28f;
                Col(x, baseY + Mathf.RoundToInt(ridge * h * 0.16f));
            }

            // Style accents on top of the ridge.
            switch (def.Style)
            {
                case ArenaStyle.Town:
                    for (int i = 0; i < 6; i++)
                    {
                        int bx = Mathf.RoundToInt(w * (0.08f + 0.16f * i)) + rnd.Next(-6, 6);
                        int bw = (int)(PPU * (0.7f + rnd.NextDouble() * 0.7f));
                        int bh = baseY + (int)(h * (0.28f + rnd.NextDouble() * 0.34f));
                        Block(bx, bx + bw, bh);
                        // false front / roof notch
                        Col(bx + bw / 2, bh + rnd.Next(2, 6));
                    }
                    break;

                case ArenaStyle.Desert:
                    for (int i = 0; i < 2; i++)
                    {
                        int mx = Mathf.RoundToInt(w * (0.24f + 0.5f * i)) + rnd.Next(-10, 10);
                        int mw = (int)(PPU * (1.6f + rnd.NextDouble() * 1.4f));
                        int mh = baseY + (int)(h * (0.30f + rnd.NextDouble() * 0.22f));
                        Block(mx, mx + mw, mh);                    // flat-topped butte
                        Block(mx - PPU / 3, mx - 1, mh - h / 8);   // stepped shoulder
                    }
                    break;

                case ArenaStyle.Graveyard:
                    for (int i = 0; i < 5; i++)
                    {
                        int cx = Mathf.RoundToInt(w * (0.12f + 0.18f * i)) + rnd.Next(-8, 8);
                        int stem = baseY + (int)(h * (0.18f + rnd.NextDouble() * 0.16f));
                        Block(cx - 1, cx + 1, stem);               // upright
                        int arm = stem - (int)(h * 0.06f);
                        Block(cx - (int)(PPU * 0.16f), cx + (int)(PPU * 0.16f), arm); // crossbar (thin)
                        Block(cx - (int)(PPU * 0.16f), cx + (int)(PPU * 0.16f), arm - 1);
                    }
                    break;

                case ArenaStyle.Ranch:
                    {
                        int mx = Mathf.RoundToInt(w * 0.2f);       // windmill tower
                        int mh = baseY + (int)(h * 0.5f);
                        Block(mx - 1, mx + 1, mh);
                        Block(mx - PPU / 6, mx + PPU / 6, baseY + (int)(h * 0.14f)); // splayed base
                        for (int k = -3; k <= 3; k++) Col(mx + k, mh - 1 - Mathf.Abs(k)); // fan hint
                    }
                    break;
            }

            var sprite = ProcSprites.Make(px, w, h, new Vector2(0.5f, 0f), PPU);
            Silhouettes[key] = sprite;
            return sprite;
        }

        // ---------- foreground props ----------

        static void PlaceProps(ArenaDef def, Transform parent)
        {
            switch (def.Style)
            {
                case ArenaStyle.Prairie:
                    Far(parent, PropArt.Cactus(), -6.7f, 0.55f);
                    Far(parent, PropArt.Rock(), 2.6f, 0.55f);
                    Near(parent, PropArt.GrassTuft(), -3.6f, 1.0f);
                    Near(parent, PropArt.Cactus(), 3.9f, 1.15f);
                    Near(parent, PropArt.Rock(), 6.9f, 1.05f);
                    break;

                case ArenaStyle.Town:
                    Far(parent, PropArt.Barrel(), -6.6f, 0.6f);
                    Far(parent, PropArt.FencePost(), 2.4f, 0.6f);
                    Near(parent, PropArt.Barrel(), -3.5f, 1.1f);
                    Near(parent, PropArt.Barrel(), -3.0f, 0.85f);
                    Near(parent, PropArt.Barrel(), 3.7f, 1.1f);
                    Near(parent, PropArt.FencePost(), 6.9f, 1.0f);
                    break;

                case ArenaStyle.Desert:
                    Far(parent, PropArt.Cactus(), -6.7f, 0.55f);
                    Far(parent, PropArt.Skull(), 2.5f, 0.5f);
                    Near(parent, PropArt.Rock(), -3.4f, 1.15f);
                    Near(parent, PropArt.Cactus(), 3.8f, 1.2f);
                    Near(parent, PropArt.Rock(), 6.9f, 0.95f);
                    break;

                case ArenaStyle.Graveyard:
                    Far(parent, PropArt.Tombstone(), -6.6f, 0.6f);
                    Far(parent, PropArt.Tombstone(), 2.5f, 0.55f);
                    Near(parent, PropArt.Tombstone(), -3.6f, 1.1f);
                    Near(parent, PropArt.DeadTree(), 3.9f, 1.2f);
                    Near(parent, PropArt.FencePost(), 6.9f, 1.0f);
                    break;

                case ArenaStyle.Ranch:
                    Far(parent, PropArt.HayBale(), -6.6f, 0.6f);
                    Far(parent, PropArt.GrassTuft(), 2.5f, 0.6f);
                    Near(parent, PropArt.HayBale(), -3.5f, 1.15f);
                    Near(parent, PropArt.FencePost(), 3.8f, 1.0f);
                    Near(parent, PropArt.GrassTuft(), 6.9f, 1.05f);
                    break;
            }
        }

        // Distant prop: sits on the horizon line, small, behind the duelists.
        static void Far(Transform parent, Sprite sprite, float x, float scale)
            => Add(parent, sprite, x, Horizon + 0.02f, scale, -60);

        // Foreground prop: near the bottom of the frame, larger, in front of the ground.
        static void Near(Transform parent, Sprite sprite, float x, float scale)
            => Add(parent, sprite, x, -3.0f, scale, -10);

        static void Add(Transform parent, Sprite sprite, float x, float y, float scale, int order)
        {
            var go = new GameObject("Prop");
            go.transform.SetParent(parent);
            go.transform.position = new Vector3(x, y, 0f);
            go.transform.localScale = new Vector3(scale, scale, 1f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = order;
        }

        static Color Dim(Color c, float t) => Color.Lerp(c, new Color(0.3f, 0.4f, 0.6f), t);
    }
}
