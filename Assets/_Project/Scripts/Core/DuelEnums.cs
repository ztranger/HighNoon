namespace HighNoon
{
    /// <summary>High-level phases of a single duel round.</summary>
    public enum DuelPhase { Idle, Intro, Stance, Tension, Bang, Resolved, Result }

    /// <summary>Which logical half of the screen a duelist belongs to.</summary>
    public enum DuelSide { Bottom, Top }

    public enum DuelistKind { Human, Bot }

    public enum DuelOutcome { None, Won, Lost, FalseStart, Draw }

    /// <summary>
    /// How a duel is decided. <see cref="Reaction"/> = classic quick-draw on BANG.
    /// <see cref="Timing"/> = one sweeping "sweet spot" bar. <see cref="Volley"/> = a PvE
    /// sequence of those bars, one per opponent. <see cref="Sync"/> = two bars at once,
    /// one hand each, both must land green on the same pass.
    /// Campaign stages pick this on <see cref="StageDef.Type"/> (default Timing; the final boss
    /// is Reaction; Town Trouble is Sync; Canyon Ambush is Volley).
    /// </summary>
    public enum DuelType { Reaction, Timing, Volley, Sync }

    /// <summary>
    /// Shared numbers for a <see cref="DuelType.Volley"/> mission. The stage sets the foe count;
    /// the round reads these so the roster and the tap sequence cannot drift apart.
    /// </summary>
    public static class Volley
    {
        public const int MinFoes = 2;
        public const int MaxFoes = 3;

        /// <summary>How long one opponent's window stays open before a silent hesitation counts as a miss.</summary>
        public const float BeatSeconds = 4.5f;

        /// <summary>Pause after a hit or a miss so the drop (or the return fire) can be read before the next window.</summary>
        public const float BetweenBeats = 0.45f;

        /// <summary>
        /// 2 or 3 foes. A missing count (0 or 1) becomes <see cref="MaxFoes"/> — a Volley stage
        /// that forgets <see cref="StageDef.Opponents"/> still fields a full line.
        /// </summary>
        public static int FoeCount(int requested)
        {
            if (requested < MinFoes) requested = MaxFoes;
            if (requested > MaxFoes) return MaxFoes;
            return requested;
        }
    }

    /// <summary>
    /// Shared numbers for a <see cref="DuelType.Sync"/> mission: two pistols (two bars, two hands).
    /// <see cref="StageDef.Opponents"/> is the whole line, always even — 2 means one shot per pistol,
    /// 4 means two shots per pistol. Green width and sweep speed still come from <c>TimingTuning</c>.
    /// The phase lag is a fraction of that green half-width, so both pointers can be green together.
    /// </summary>
    public static class SyncRules
    {
        public const int Hands = 2;
        public const int MinFoes = 2;
        public const int MaxFoes = 4;

        /// <summary>How long one pair of bars stays live before an untouched hand is a miss.</summary>
        public const float RoundSeconds = 8f;

        /// <summary>Phase offset = green half-width × this. Keeps an overlap; does not scale past the zone.</summary>
        public const float PhaseInGreen = 0.75f;

        /// <summary>
        /// Even foe count from 2 to 4. A missing count becomes 2 (one shot per hand).
        /// An odd count rounds up, then clamps — 3 becomes 4, 5 becomes 4.
        /// Shots per pistol are <see cref="ShotsPerHand"/>.
        /// </summary>
        public static int FoeCount(int requested)
        {
            if (requested < MinFoes) requested = MinFoes;
            if ((requested & 1) == 1) requested++;
            if (requested > MaxFoes) requested = MaxFoes;
            return requested;
        }

        public static int ShotsPerHand(int foes) => FoeCount(foes) / Hands;
    }
}
