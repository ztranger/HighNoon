using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace HighNoon.EditorTools
{
    /// <summary>
    /// Splits an .aseprite file's layers into the per-part PNGs the cut-out rig (<see cref="CowboyRig"/>)
    /// loads — one full-canvas <c>{layerName}.png</c> per image layer. Parses the Aseprite binary directly,
    /// so no Aseprite install / Python is needed. Run from the <b>High Noon</b> menu after editing the art.
    ///
    /// <c>template.aseprite</c> is a pristine template — don't edit it. Duplicate it to a working file per
    /// character (e.g. <c>hero.aseprite</c>) and draw there; the default menu item imports <c>hero.aseprite</c>.
    /// Layer names must match the rig: torso, head, hat, arm_upper, arm_fore, arm_back, leg_front, leg_back,
    /// and optionally hand (fingers drawn over the gun). If there's no <c>hand</c> layer, one is derived from
    /// the lower part of arm_fore so the over-the-gun overlay keeps working.
    /// </summary>
    public static class AsepriteRigImporter
    {
        const string DefaultAseprite = "Assets/_Project/Resources/Art/hero.aseprite"; // working file (copy of the template)
        const string DefaultOutDir   = "Assets/_Project/Resources/Art/Cowboys/hero_rig";

        [MenuItem("High Noon/Import Rig Parts (hero.aseprite)")]
        public static void ImportDefault()
        {
            if (!File.Exists(ToAbs(DefaultAseprite)))
            {
                EditorUtility.DisplayDialog("Rig import",
                    $"Not found:\n{DefaultAseprite}\n\nDuplicate template.aseprite → hero.aseprite and draw your " +
                    "character there (keep the template untouched), or use \"Import Rig Parts (pick file)…\".", "OK");
                return;
            }
            Run(ToAbs(DefaultAseprite), DefaultOutDir);
        }

        // Each character → its own folder named after the file (Cowboys/<file>/), which becomes the
        // character id auto-discovered by the AnimTest preview scene.
        [MenuItem("High Noon/Import Rig Parts (pick file → new character)…")]
        public static void ImportPicked()
        {
            string abs = EditorUtility.OpenFilePanelWithFilters(
                "Pick an Aseprite character file", ToAbs("Assets/_Project/Resources/Art"),
                new[] { "Aseprite", "aseprite,ase" });
            if (string.IsNullOrEmpty(abs)) return;

            string id = Path.GetFileNameWithoutExtension(abs);
            Run(abs, "Assets/_Project/Resources/Art/Cowboys/" + id);
        }

        static string ToAbs(string projectRel) => Path.GetFullPath(Path.Combine(Application.dataPath, "..", projectRel));

        static void Run(string asepriteAbs, string outDirRel)
        {
            try
            {
                var doc = Parse(File.ReadAllBytes(asepriteAbs));
                string outAbs = ToAbs(outDirRel);
                Directory.CreateDirectory(outAbs);

                var written = new List<string>();
                Color32[] armFore = null;
                bool hasHand = false;

                foreach (var lay in doc.Layers)
                {
                    if (lay.IsGroup || lay.Pixels == null) continue;
                    WritePng(Path.Combine(outAbs, lay.Name + ".png"), lay.Pixels, doc.W, doc.H);
                    written.Add(lay.Name);
                    if (lay.Name == "arm_fore") armFore = lay.Pixels;
                    if (lay.Name == "hand") hasHand = true;
                }

                if (!hasHand && armFore != null)
                {
                    WritePng(Path.Combine(outAbs, "hand.png"), DeriveHand(armFore, doc.W, doc.H), doc.W, doc.H);
                    written.Add("hand (auto from arm_fore)");
                }

                AssetDatabase.Refresh();
                string id = Path.GetFileName(outAbs.TrimEnd('/', '\\'));
                Debug.Log($"[AsepriteRigImporter] {Path.GetFileName(asepriteAbs)} {doc.W}x{doc.H} → {outDirRel}\n  {string.Join(", ", written)}");
                EditorUtility.DisplayDialog("Rig import",
                    $"Imported {written.Count} parts ({doc.W}x{doc.H}) into\n{outDirRel}\n\n{string.Join(", ", written)}\n\n" +
                    $"Open Scenes/AnimTest.unity → Play to preview \"{id}\" (auto-listed).", "OK");
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                EditorUtility.DisplayDialog("Rig import failed", e.Message, "OK");
            }
        }

        // Aseprite pixels are top-down; Unity textures are bottom-up → flip rows on write.
        static void WritePng(string path, Color32[] topDown, int w, int h)
        {
            var flipped = new Color32[w * h];
            for (int y = 0; y < h; y++)
                Array.Copy(topDown, y * w, flipped, (h - 1 - y) * w, w);

            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.SetPixels32(flipped);
            tex.Apply();
            File.WriteAllBytes(path, tex.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(tex);
        }

        // Keep only the lower ~38% of arm_fore (the hand/wrist) so it can render over the gun.
        static Color32[] DeriveHand(Color32[] src, int w, int h)
        {
            int y0 = h, y1 = -1;
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                    if (src[y * w + x].a > 8) { if (y < y0) y0 = y; if (y > y1) y1 = y; break; }

            var outp = (Color32[])src.Clone();
            if (y1 < y0) return outp;
            int cut = y0 + (int)(0.62f * (y1 - y0)); // top-down: keep rows >= cut (the bottom = the hand)
            for (int y = 0; y < cut; y++)
                for (int x = 0; x < w; x++)
                {
                    var c = outp[y * w + x];
                    c.a = 0;
                    outp[y * w + x] = c;
                }
            return outp;
        }

        // ---------- minimal Aseprite parser (frame 0) ----------

        class Layer { public string Name; public bool IsGroup; public Color32[] Pixels; }
        class Doc { public int W, H; public readonly List<Layer> Layers = new List<Layer>(); }

        static Doc Parse(byte[] d)
        {
            int U16(int o) => d[o] | (d[o + 1] << 8);
            int S16(int o) { int v = U16(o); return v >= 0x8000 ? v - 0x10000 : v; }
            long U32(int o) => (uint)(d[o] | (d[o + 1] << 8) | (d[o + 2] << 16) | (d[o + 3] << 24));

            if (U16(4) != 0xA5E0) throw new Exception("Not an .aseprite file (bad magic).");
            int frames = U16(6), W = U16(8), H = U16(10), depth = U16(12);
            int transIndex = d[28];
            var doc = new Doc { W = W, H = H };
            var palette = new Color32[256];

            int o = 128;
            for (int f = 0; f < frames; f++)
            {
                long frameBytes = U32(o);
                long nch = U32(o + 12);
                if (nch == 0) nch = U16(o + 6);
                int p = o + 16;
                for (long c = 0; c < nch; c++)
                {
                    long csize = U32(p);
                    int ctype = U16(p + 4);
                    int body = p + 6;

                    if (ctype == 0x2004) // layer
                    {
                        int ltype = U16(body + 2);
                        int nlen = U16(body + 16);
                        string name = Encoding.UTF8.GetString(d, body + 18, nlen);
                        doc.Layers.Add(new Layer { Name = name, IsGroup = ltype == 1 });
                    }
                    else if (ctype == 0x2005 && f == 0) // cel (frame 0 only)
                    {
                        int li = U16(body); int x = S16(body + 2), y = S16(body + 4); int celType = U16(body + 7);
                        if ((celType == 0 || celType == 2) && li >= 0 && li < doc.Layers.Count)
                        {
                            int cw = U16(body + 16), chh = U16(body + 18);
                            int dataOff = body + 20;
                            byte[] raw = celType == 2
                                ? Inflate(d, dataOff, (int)(p + csize - dataOff))
                                : Slice(d, dataOff, cw * chh * Bpp(depth));
                            var full = new Color32[W * H];
                            Blit(full, W, H, x, y, cw, chh, raw, depth, palette, transIndex);
                            doc.Layers[li].Pixels = full;
                        }
                    }
                    else if (ctype == 0x2019) // palette
                    {
                        long first = U32(body + 4), last = U32(body + 8);
                        int q = body + 20;
                        for (long i = first; i <= last && i < 256; i++)
                        {
                            int flags = U16(q);
                            palette[i] = new Color32(d[q + 2], d[q + 3], d[q + 4], d[q + 5]);
                            q += 6;
                            if ((flags & 1) != 0) q += 2 + U16(q);
                        }
                    }
                    p += (int)csize;
                }
                o += (int)frameBytes;
            }
            return doc;
        }

        static int Bpp(int depth) => depth == 32 ? 4 : depth == 16 ? 2 : 1;

        static byte[] Slice(byte[] d, int off, int len)
        {
            var b = new byte[len];
            Array.Copy(d, off, b, 0, len);
            return b;
        }

        static byte[] Inflate(byte[] d, int off, int count)
        {
            using (var ms = new MemoryStream(d, off + 2, count - 2)) // skip 2-byte zlib header
            using (var ds = new DeflateStream(ms, CompressionMode.Decompress))
            using (var outMs = new MemoryStream())
            {
                ds.CopyTo(outMs);
                return outMs.ToArray();
            }
        }

        static void Blit(Color32[] full, int W, int H, int x, int y, int cw, int ch, byte[] raw, int depth, Color32[] pal, int transIndex)
        {
            for (int yy = 0; yy < ch; yy++)
                for (int xx = 0; xx < cw; xx++)
                {
                    int ax = x + xx, ay = y + yy;
                    if (ax < 0 || ax >= W || ay < 0 || ay >= H) continue;
                    Color32 col;
                    if (depth == 32) { int s = (yy * cw + xx) * 4; col = new Color32(raw[s], raw[s + 1], raw[s + 2], raw[s + 3]); }
                    else if (depth == 8) { int idx = raw[yy * cw + xx]; col = pal[idx]; if (idx == transIndex) col.a = 0; }
                    else { int s = (yy * cw + xx) * 2; byte g = raw[s]; col = new Color32(g, g, g, raw[s + 1]); }
                    full[ay * W + ax] = col;
                }
        }
    }
}
