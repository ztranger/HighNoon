using UnityEngine;

namespace HighNoon
{
    /// <summary>
    /// Persisted lifetime player records (PlayerPrefs): fastest quick-draw reaction, best timing
    /// accuracy, campaign completions and furthest campaign node reached. Report-methods return
    /// true when a new best was set, so the UI can celebrate it.
    /// </summary>
    public static class Records
    {
        const string KReaction = "hn_best_reaction_ms";
        const string KAccuracy = "hn_best_accuracy";
        const string KCompletions = "hn_completions";
        const string KFurthestCh = "hn_furthest_ch";
        const string KFurthestSt = "hn_furthest_st";

        public static float BestReactionMs => PlayerPrefs.GetFloat(KReaction, 0f);
        public static float BestAccuracy   => PlayerPrefs.GetFloat(KAccuracy, 0f); // 0..1
        public static int Completions      => PlayerPrefs.GetInt(KCompletions, 0);
        public static int FurthestChapter  => PlayerPrefs.GetInt(KFurthestCh, 0);
        public static int FurthestStage    => PlayerPrefs.GetInt(KFurthestSt, 0);

        public static bool HasReaction => BestReactionMs > 0f;
        public static bool HasAccuracy => BestAccuracy > 0f;

        /// <summary>Record a valid reaction time (ms). Returns true if it's a new best (faster).</summary>
        public static bool ReportReaction(float ms)
        {
            if (ms <= 0f) return false;
            float best = BestReactionMs;
            if (best > 0f && ms >= best) return false;
            PlayerPrefs.SetFloat(KReaction, ms);
            PlayerPrefs.Save();
            return true;
        }

        /// <summary>Record a timing accuracy (0..1, higher = closer to centre). Returns true if new best.</summary>
        public static bool ReportAccuracy(float accuracy01)
        {
            accuracy01 = Mathf.Clamp01(accuracy01);
            if (HasAccuracy && accuracy01 <= BestAccuracy) return false;
            PlayerPrefs.SetFloat(KAccuracy, accuracy01);
            PlayerPrefs.Save();
            return true;
        }

        public static void ReportCompletion()
        {
            PlayerPrefs.SetInt(KCompletions, Completions + 1);
            PlayerPrefs.Save();
        }

        /// <summary>Remember the furthest campaign node ever reached (chapter, then stage).</summary>
        public static void ReportProgress(int chapter, int stage)
        {
            if (chapter > FurthestChapter || (chapter == FurthestChapter && stage > FurthestStage))
            {
                PlayerPrefs.SetInt(KFurthestCh, chapter);
                PlayerPrefs.SetInt(KFurthestSt, stage);
                PlayerPrefs.Save();
            }
        }
    }
}
