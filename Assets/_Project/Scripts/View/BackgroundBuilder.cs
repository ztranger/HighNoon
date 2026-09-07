using UnityEngine;

namespace HighNoon
{
    /// <summary>
    /// Builds a duel arena's background at runtime: a noise-textured ground, a styled
    /// centre divider, and scattered props. Returns the root so it can be rebuilt.
    /// </summary>
    public static class BackgroundBuilder
    {
        public static GameObject Build(ArenaDef def)
        {
            var root = new GameObject("BackgroundRoot");

            // Coverage sized to the current camera view (over-covers for any aspect).
            float cover = 20f;
            var cam = Camera.main;
            if (cam != null)
            {
                float h = cam.orthographicSize * 2f;
                float w = h * cam.aspect;
                cover = Mathf.Max(h, w) * 1.25f;
            }

            // Ground.
            var ground = new GameObject("Ground");
            ground.transform.SetParent(root.transform);
            var gsr = ground.AddComponent<SpriteRenderer>();
            gsr.sprite = BuildGround(def);
            gsr.sortingOrder = -100;
            float native = 128f / 100f; // sprite native size (units)
            float s = cover / native;
            ground.transform.localScale = new Vector3(s, s, 1f);

            // Centre divider.
            var divider = new GameObject("Divider");
            divider.transform.SetParent(root.transform);
            var dsr = divider.AddComponent<SpriteRenderer>();
            dsr.sprite = PlaceholderArt.SolidSprite(def.Divider, 4, 4, 1);
            dsr.sortingOrder = -40;
            divider.transform.localScale = new Vector3(cover, def.Style == ArenaStyle.Town ? 0.35f : 0.14f, 1f);

            PlaceProps(def, root.transform, cover);
            return root;
        }

        static Sprite BuildGround(ArenaDef def)
        {
            const int T = 128;
            var tex = new Texture2D(T, T, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            var px = new Color32[T * T];
            int seedInt = def.Name.GetHashCode();
            float seed = (seedInt % 1000) * 0.01f;
            var rnd = new System.Random(seedInt);

            for (int y = 0; y < T; y++)
                for (int x = 0; x < T; x++)
                {
                    float n = Mathf.PerlinNoise(x * 0.05f + seed, y * 0.05f + seed);
                    Color c = Color.Lerp(def.GroundShade, def.GroundBase, n);
                    double r = rnd.NextDouble();
                    if (r < 0.03) c = def.Speck;                          // pebbles
                    else if (r < 0.06) c = Color.Lerp(c, Color.white, 0.15f); // light fleck
                    px[y * T + x] = c;
                }

            tex.SetPixels32(px);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, T, T), new Vector2(0.5f, 0.5f), 100);
        }

        static void PlaceProps(ArenaDef def, Transform parent, float cover)
        {
            switch (def.Style)
            {
                case ArenaStyle.Prairie:
                    Add(parent, PropArt.Cactus(), -3.9f, -1.2f, 1.1f);
                    Add(parent, PropArt.Cactus(), 3.7f, 1.6f, 0.9f);
                    Add(parent, PropArt.Rock(), -3.0f, 2.6f, 1.0f);
                    Add(parent, PropArt.Rock(), 3.1f, -2.6f, 1.2f);
                    Add(parent, PropArt.GrassTuft(), -2.4f, -3.7f, 1.0f);
                    Add(parent, PropArt.GrassTuft(), 2.6f, 3.4f, 1.0f);
                    Add(parent, PropArt.GrassTuft(), 4.3f, -3.9f, 1.0f);
                    break;

                case ArenaStyle.Town:
                    // Boardwalk planks framing the street (top & bottom edges).
                    AddScaled(parent, PropArt.Plank(), 0f, 5.1f, cover / 1.5f, 0.9f, -30);
                    AddScaled(parent, PropArt.Plank(), 0f, -5.1f, cover / 1.5f, 0.9f, -30);
                    Add(parent, PropArt.Barrel(), -3.6f, -2.2f, 1.1f);
                    Add(parent, PropArt.Barrel(), -3.0f, -2.9f, 0.9f);
                    Add(parent, PropArt.Barrel(), 3.6f, 2.2f, 1.1f);
                    Add(parent, PropArt.FencePost(), -2.5f, 3.6f, 1.0f);
                    Add(parent, PropArt.FencePost(), 2.6f, -3.6f, 1.0f);
                    break;

                case ArenaStyle.Desert:
                    Add(parent, PropArt.Cactus(), -3.8f, 1.4f, 1.2f);
                    Add(parent, PropArt.Cactus(), 4.0f, -2.6f, 1.0f);
                    Add(parent, PropArt.Skull(), 3.1f, -1.4f, 1.0f);
                    Add(parent, PropArt.Rock(), -3.2f, -2.8f, 1.2f);
                    Add(parent, PropArt.Rock(), 2.6f, 3.2f, 1.0f);
                    Add(parent, PropArt.Rock(), -4.2f, 3.4f, 0.9f);
                    break;

                case ArenaStyle.Graveyard:
                    Add(parent, PropArt.Tombstone(), -3.6f, -1.4f, 1.1f);
                    Add(parent, PropArt.Tombstone(), -2.6f, -3.2f, 0.9f);
                    Add(parent, PropArt.Tombstone(), 3.4f, 1.6f, 1.1f);
                    Add(parent, PropArt.Tombstone(), 2.8f, 3.4f, 0.9f);
                    Add(parent, PropArt.DeadTree(), 4.3f, -2.4f, 1.2f);
                    Add(parent, PropArt.FencePost(), -4.3f, 2.6f, 1.0f);
                    Add(parent, PropArt.Rock(), -3.0f, 3.2f, 0.9f);
                    break;

                case ArenaStyle.Ranch:
                    Add(parent, PropArt.HayBale(), -3.5f, -2.4f, 1.1f);
                    Add(parent, PropArt.HayBale(), 3.4f, 2.2f, 1.1f);
                    Add(parent, PropArt.FencePost(), -2.6f, 3.6f, 1.0f);
                    Add(parent, PropArt.FencePost(), 2.6f, -3.6f, 1.0f);
                    Add(parent, PropArt.GrassTuft(), -3.9f, 1.2f, 1.0f);
                    Add(parent, PropArt.GrassTuft(), 3.9f, -1.2f, 1.0f);
                    Add(parent, PropArt.Barrel(), 3.2f, -2.9f, 1.0f);
                    break;
            }
        }

        static void Add(Transform parent, Sprite sprite, float x, float y, float scale, int order = -10)
            => AddScaled(parent, sprite, x, y, scale, scale, order);

        static void AddScaled(Transform parent, Sprite sprite, float x, float y, float sx, float sy, int order = -10)
        {
            var go = new GameObject("Prop");
            go.transform.SetParent(parent);
            go.transform.position = new Vector3(x, y, 0f);
            go.transform.localScale = new Vector3(sx, sy, 1f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = order;
        }
    }
}
