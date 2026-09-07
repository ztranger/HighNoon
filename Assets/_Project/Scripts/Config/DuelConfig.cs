using UnityEngine;

namespace HighNoon
{
    /// <summary>Pacing/timing for a duel round.</summary>
    [CreateAssetMenu(fileName = "DuelConfig", menuName = "High Noon/Duel Config")]
    public class DuelConfig : ScriptableObject
    {
        [Header("Timing (seconds)")]
        [Tooltip("Cowboys walking out onto the field.")]
        public float introDuration = 1.2f;

        [Tooltip("Settling into the stance before tension starts.")]
        public float stancePause = 0.6f;

        [Header("Hidden tension countdown (players do not see this)")]
        public float tensionMin = 1.5f;
        public float tensionMax = 4.0f;

        [Header("After the shot")]
        [Tooltip("Delay between the shot resolving and showing the result buttons.")]
        public float resultDelay = 0.9f;

        public float RollTension() => Random.Range(tensionMin, tensionMax);
    }
}
