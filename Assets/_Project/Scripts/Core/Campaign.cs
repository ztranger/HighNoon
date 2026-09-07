using UnityEngine;

namespace HighNoon
{
    /// <summary>One PvE encounter: which arena, which opponent, how tough.</summary>
    public class StageDef
    {
        public string Title;
        public string Arena;
        public Difficulty Difficulty;
    }

    /// <summary>
    /// PvE campaign: a linear run of staged duels with lives. Static so run state
    /// survives the Duel scene reloading between fights. A visual mission map can
    /// sit on top of this later (each map node = a stage that pins its arena).
    /// </summary>
    public static class Campaign
    {
        public static readonly StageDef[] Stages =
        {
            new StageDef { Title = "The Drifter",   Arena = "Prairie",     Difficulty = Difficulty.Easy   },
            new StageDef { Title = "Town Trouble",  Arena = "Dusty Town",  Difficulty = Difficulty.Easy   },
            new StageDef { Title = "Canyon Ambush", Arena = "Red Canyon",  Difficulty = Difficulty.Normal },
            new StageDef { Title = "The Undertaker",Arena = "Boot Hill",   Difficulty = Difficulty.Normal },
            new StageDef { Title = "The Sheriff",   Arena = "Salt Flats",  Difficulty = Difficulty.Hard   },
        };

        public const int StartingLives = 3;

        public static bool Active;
        public static int Stage;   // index into Stages
        public static int Lives;

        public static StageDef Current => Stages[Mathf.Clamp(Stage, 0, Stages.Length - 1)];
        public static bool IsLastStage => Stage >= Stages.Length - 1;

        public static void StartRun()
        {
            Active = true;
            Stage = 0;
            Lives = StartingLives;
        }

        /// <summary>Pins the current stage's arena + difficulty into MatchSettings for the duel.</summary>
        public static void ApplyToMatch()
        {
            MatchSettings.Mode = GameMode.PvE;
            MatchSettings.ForcedArena = Current.Arena;
            MatchSettings.BotDifficulty = Current.Difficulty;
        }
    }
}
