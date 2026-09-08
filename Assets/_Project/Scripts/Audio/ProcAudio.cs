using System;
using UnityEngine;

namespace HighNoon
{
    /// <summary>
    /// Procedural SFX generator (no external assets). Builds short mono <see cref="AudioClip"/>s
    /// at runtime: a punchy gunshot, a noon bell, a body thud, a tense heartbeat loop, victory /
    /// defeat stings and a UI click. Swap for real samples later; keep the factory names.
    /// </summary>
    public static class ProcAudio
    {
        const int Rate = 44100;

        static AudioClip Make(string name, float[] data)
        {
            var clip = AudioClip.Create(name, data.Length, 1, Rate, false);
            clip.SetData(data, 0);
            return clip;
        }

        static float Env(float t01, float attack, float decay)
        {
            // quick linear attack then exponential decay over the normalized clip time
            if (t01 < attack) return t01 / Mathf.Max(1e-4f, attack);
            return Mathf.Exp(-decay * (t01 - attack));
        }

        /// <summary>Sharp crack + low body with a fast pitch drop — a revolver shot.</summary>
        public static AudioClip Gunshot()
        {
            float dur = 0.30f; int n = (int)(Rate * dur); var d = new float[n];
            var rnd = new System.Random(7); float lp = 0f; double ph = 0;
            for (int i = 0; i < n; i++)
            {
                float t = (float)i / n;
                float noise = (float)(rnd.NextDouble() * 2.0 - 1.0);
                lp += (noise - lp) * 0.55f;               // one-pole low-pass softens the hiss
                float crack = lp * Mathf.Exp(-13f * t);
                float freq = 60f + 150f * Mathf.Exp(-7f * t);
                ph += 2.0 * Math.PI * freq / Rate;
                float body = (float)Math.Sin(ph) * Mathf.Exp(-9f * t);
                d[i] = Mathf.Clamp(crack * 0.9f + body * 0.7f, -1f, 1f);
            }
            return Make("gunshot", d);
        }

        /// <summary>Bright two-partial bell — the noon "GO" signal.</summary>
        public static AudioClip Bell()
        {
            float dur = 0.9f; int n = (int)(Rate * dur); var d = new float[n];
            for (int i = 0; i < n; i++)
            {
                float t = (float)i / n; float tt = (float)i / Rate;
                float a = Mathf.Sin(2f * Mathf.PI * 660f * tt) * Mathf.Exp(-3.5f * t);
                float b = Mathf.Sin(2f * Mathf.PI * 990f * tt) * Mathf.Exp(-4.5f * t) * 0.6f;
                float c = Mathf.Sin(2f * Mathf.PI * 1320f * tt) * Mathf.Exp(-6f * t) * 0.3f;
                d[i] = Mathf.Clamp((a + b + c) * 0.6f, -1f, 1f);
            }
            return Make("bell", d);
        }

        /// <summary>Low falling thump — a body dropping.</summary>
        public static AudioClip Thud()
        {
            float dur = 0.42f; int n = (int)(Rate * dur); var d = new float[n];
            var rnd = new System.Random(21); double ph = 0; float lp = 0f;
            for (int i = 0; i < n; i++)
            {
                float t = (float)i / n;
                float freq = 55f + 90f * Mathf.Exp(-10f * t);
                ph += 2.0 * Math.PI * freq / Rate;
                float body = (float)Math.Sin(ph) * Mathf.Exp(-7f * t);
                float noise = (float)(rnd.NextDouble() * 2.0 - 1.0);
                lp += (noise - lp) * 0.15f;               // dull thud, mostly low
                float dust = lp * Mathf.Exp(-16f * t) * 0.5f;
                d[i] = Mathf.Clamp(body * 0.9f + dust, -1f, 1f);
            }
            return Make("thud", d);
        }

        /// <summary>Seamless heartbeat loop (two thumps then silence) — building tension.</summary>
        public static AudioClip Tension()
        {
            float dur = 1.05f; int n = (int)(Rate * dur); var d = new float[n];
            // two heartbeat thumps at these fractions of the loop
            float[] beats = { 0.02f, 0.20f };
            float[] gains = { 1.0f, 0.75f };
            for (int b = 0; b < beats.Length; b++)
            {
                int start = (int)(beats[b] * n);
                float len = 0.16f; int m = (int)(Rate * len);
                double ph = 0;
                for (int i = 0; i < m && start + i < n; i++)
                {
                    float t = (float)i / m;
                    float freq = 48f + 40f * Mathf.Exp(-9f * t);
                    ph += 2.0 * Math.PI * freq / Rate;
                    float s = (float)Math.Sin(ph) * Mathf.Exp(-7f * t) * gains[b];
                    d[start + i] += s;
                }
            }
            for (int i = 0; i < n; i++) d[i] = Mathf.Clamp(d[i] * 0.9f, -1f, 1f);
            return Make("tension", d);
        }

        /// <summary>Rising three-note major arpeggio — a win.</summary>
        public static AudioClip Victory()
        {
            float[] notes = { 523.25f, 659.25f, 783.99f }; // C5 E5 G5
            return Arpeggio("victory", notes, 0.16f, 0.55f);
        }

        /// <summary>Falling two-note minor drop — a loss.</summary>
        public static AudioClip Defeat()
        {
            float[] notes = { 392.00f, 311.13f }; // G4 → Eb4
            return Arpeggio("defeat", notes, 0.26f, 0.9f);
        }

        static AudioClip Arpeggio(string name, float[] notes, float noteDur, float total)
        {
            int n = (int)(Rate * total); var d = new float[n];
            for (int k = 0; k < notes.Length; k++)
            {
                int start = (int)(k * noteDur * Rate);
                int m = (int)(noteDur * 1.6f * Rate); // overlap tails
                double ph = 0;
                for (int i = 0; i < m && start + i < n; i++)
                {
                    float t = (float)i / m;
                    ph += 2.0 * Math.PI * notes[k] / Rate;
                    float s = (float)Math.Sin(ph);
                    s += 0.3f * (float)Math.Sin(ph * 2.0); // a little brightness
                    d[start + i] += s * Env(t, 0.03f, 4f) * 0.45f;
                }
            }
            for (int i = 0; i < n; i++) d[i] = Mathf.Clamp(d[i], -1f, 1f);
            return Make(name, d);
        }

        /// <summary>Short soft blip — UI tap.</summary>
        public static AudioClip Click()
        {
            float dur = 0.05f; int n = (int)(Rate * dur); var d = new float[n];
            double ph = 0;
            for (int i = 0; i < n; i++)
            {
                float t = (float)i / n;
                ph += 2.0 * Math.PI * 1100f / Rate;
                d[i] = Mathf.Clamp((float)Math.Sin(ph) * Mathf.Exp(-22f * t) * 0.5f, -1f, 1f);
            }
            return Make("click", d);
        }
    }
}
