namespace HighNoon
{
    public enum GameMode { PvP, Coop, PvE }
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

        /// <summary>Reaction (quick-draw) vs Timing (sweet-spot bar). Menu sets it for PvP/Coop;
        /// the PvE campaign overrides it per stage in <see cref="Campaign.ApplyToMatch"/>.</summary>
        public static DuelType Type = DuelType.Reaction;

        /// <summary>Menu location pick for PvP/Coop. Null = random. PvE still pins the stage arena.</summary>
        public static string MenuArena = null;

        /// <summary>Arena name to pin for this duel (PvE stage or a copy of <see cref="MenuArena"/>), or null for a random pick.</summary>
        public static string ForcedArena = null;

        public static string MenuArenaLabel =>
            string.IsNullOrEmpty(MenuArena) ? "RANDOM" : MenuArena.ToUpperInvariant();

        public static void CycleMenuArena(int dir)
        {
            var all = Arenas.All;
            int n = all.Length + 1; // 0 = random
            int i = 0;
            if (!string.IsNullOrEmpty(MenuArena))
                for (int k = 0; k < all.Length; k++)
                    if (all[k].Name == MenuArena) { i = k + 1; break; }
            i = (i + dir) % n;
            if (i < 0) i += n;
            MenuArena = i == 0 ? null : all[i - 1].Name;
        }

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
