using UnityEngine;

namespace HighNoon
{
    /// <summary>
    /// Duel SFX: a tense heartbeat loop, the noon bell, gunshots and death thuds. Clips come from
    /// <see cref="AudioBank"/> (a random variant each time, procedural fallback if none imported);
    /// volume respects <see cref="GameSettings"/>.
    /// </summary>
    public class DuelAudio : MonoBehaviour
    {
        AudioSource _oneShot;
        AudioSource _loop;

        public void Setup()
        {
            _oneShot = gameObject.AddComponent<AudioSource>();
            _oneShot.playOnAwake = false;

            _loop = gameObject.AddComponent<AudioSource>();
            _loop.playOnAwake = false;
            _loop.loop = true;
        }

        static float V => GameSettings.SfxVolume;

        public void StartTension()
        {
            _loop.clip = AudioBank.GetRandom("tension", ProcAudio.Tension); // fresh heartbeat each round
            _loop.volume = 0.4f * V;
            if (V > 0f) _loop.Play();
        }

        public void StopTension() { if (_loop != null) _loop.Stop(); }
        public void Bang() { StopTension(); _oneShot.PlayOneShot(AudioBank.GetRandom("bang", ProcAudio.Bell), 0.9f * V); }
        public void Gunshot() { Gunshot(Weapons.Selected); }
        public void Gunshot(WeaponDef weapon) { _oneShot.PlayOneShot(AudioBank.GunshotFor(weapon, ProcAudio.Gunshot), 0.95f * V); }
        public void Death() { _oneShot.PlayOneShot(AudioBank.GetRandom("death", ProcAudio.Thud), 0.9f * V); }
    }
}
