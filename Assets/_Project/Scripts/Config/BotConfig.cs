using UnityEngine;

namespace HighNoon
{
    /// <summary>
    /// Difficulty settings for an AI duelist. Reaction time is the core knob:
    /// lower = harder. Reused for empty top slots in PvP/Coop and always in PvE.
    /// </summary>
    [CreateAssetMenu(fileName = "BotConfig", menuName = "High Noon/Bot Config")]
    public class BotConfig : ScriptableObject
    {
        [Tooltip("Display / difficulty label shown in UI.")]
        public string displayName = "Bot";

        [Tooltip("Minimum reaction time after BANG (seconds).")]
        public float reactionMin = 0.28f;

        [Tooltip("Maximum reaction time after BANG (seconds).")]
        public float reactionMax = 0.45f;

        [Range(0f, 1f)]
        [Tooltip("Chance the bot jumps the gun (fires before BANG). 0 = never. Reserved for later tuning.")]
        public float falseStartChance = 0f;

        public float RollReaction() => Random.Range(reactionMin, reactionMax);

        /// <summary>Restore field defaults. Used when a session-cached runtime instance
        /// is reused without <see cref="MatchSettings.ApplyDifficulty"/>.</summary>
        public void ApplyDefaults()
        {
            displayName = "Bot";
            reactionMin = 0.28f;
            reactionMax = 0.45f;
            falseStartChance = 0f;
        }
    }
}
