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
    /// <see cref="Timing"/> = a sweeping "sweet spot" bar; tap while the pointer is in the
    /// green zone. PvE uses Timing for every stage except the final boss (Reaction).
    /// </summary>
    public enum DuelType { Reaction, Timing }
}
