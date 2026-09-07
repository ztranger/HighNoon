namespace HighNoon
{
    public enum GameMode { PvP, Coop, PvE }   // Coop/PvE wired later
    public enum PvPPlayers { OnePlayer, TwoPlayers }
    public enum Difficulty { Easy, Normal, Hard }

    /// <summary>
    /// Selections carried from the main menu into the duel scene. Static so it
    /// survives scene loads without a persistent GameObject. Defaults make the
    /// Duel scene playable even when opened directly.
    /// </summary>
    public static class MatchSettings
    {
        public static GameMode Mode = GameMode.PvP;
        public static PvPPlayers Players = PvPPlayers.OnePlayer;
        public static Difficulty BotDifficulty = Difficulty.Normal;

        /// <summary>Arena name to force, or null for a random arena each duel. The menu can set this later.</summary>
        public static string ForcedArena = null;

        /// <summary>Writes difficulty-appropriate reaction times into a bot config.</summary>
        public static void ApplyDifficulty(BotConfig cfg)
        {
            if (cfg == null) return;
            switch (BotDifficulty)
            {
                case Difficulty.Easy:
                    cfg.reactionMin = 0.45f; cfg.reactionMax = 0.75f; cfg.displayName = "Easy Bot"; break;
                case Difficulty.Normal:
                    cfg.reactionMin = 0.30f; cfg.reactionMax = 0.48f; cfg.displayName = "Bot"; break;
                case Difficulty.Hard:
                    cfg.reactionMin = 0.18f; cfg.reactionMax = 0.30f; cfg.displayName = "Fast Bot"; break;
            }
        }
    }
}
