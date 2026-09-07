using UnityEngine;

namespace HighNoon
{
    /// <summary>
    /// Procedurally-generated placeholder SFX so the duel has audible tension and
    /// feedback before real audio assets are added. Swap clips later; keep the API.
    /// </summary>
    public class DuelAudio : MonoBehaviour
    {
        AudioSource _oneShot;
        AudioSource _loop;
        AudioClip _tension, _bell, _gunshot, _thud;

        public void Setup()
        {
            _oneShot = gameObject.AddComponent<AudioSource>();
            _oneShot.playOnAwake = false;

            _loop = gameObject.AddComponent<AudioSource>();
            _loop.playOnAwake = false;
            _loop.loop = true;

            _tension = MakeTone("tension", 90f, 0.7f, k => 0.25f * (1f + Mathf.Sin(2f * Mathf.PI * k * 2f)));
            _bell = MakeTone("bell", 660f, 0.9f, k => Mathf.Exp(-4f * k));
            _gunshot = MakeTone("gunshot", 0f, 0.25f, k => Mathf.Exp(-18f * k), noise: true);
            _thud = MakeTone("thud", 120f, 0.3f, k => Mathf.Exp(-10f * k));
        }

        public void StartTension() { _loop.clip = _tension; _loop.volume = 0.4f; _loop.Play(); }
        public void StopTension() { if (_loop != null) _loop.Stop(); }
        public void Bang() { StopTension(); _oneShot.PlayOneShot(_bell, 0.9f); }
        public void Gunshot() { _oneShot.PlayOneShot(_gunshot, 0.9f); }
        public void Death() { _oneShot.PlayOneShot(_thud, 0.8f); }

        static AudioClip MakeTone(string name, float freq, float dur, System.Func<float, float> env, bool noise = false)
        {
            const int rate = 44100;
            int n = Mathf.CeilToInt(rate * dur);
            var data = new float[n];
            var rnd = new System.Random(1337);
            for (int i = 0; i < n; i++)
            {
                float tt = (float)i / rate;
                float s = noise ? (float)(rnd.NextDouble() * 2.0 - 1.0) : Mathf.Sin(2f * Mathf.PI * freq * tt);
                data[i] = s * env(tt / dur);
            }
            var clip = AudioClip.Create(name, n, 1, rate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
