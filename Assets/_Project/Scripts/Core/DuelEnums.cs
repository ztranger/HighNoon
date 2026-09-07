namespace HighNoon
{
    /// <summary>High-level phases of a single duel round.</summary>
    public enum DuelPhase { Idle, Intro, Stance, Tension, Bang, Resolved, Result }

    /// <summary>Which logical half of the screen a duelist belongs to.</summary>
    public enum DuelSide { Bottom, Top }

    public enum DuelistKind { Human, Bot }

    public enum DuelOutcome { None, Won, Lost, FalseStart, Draw }
}
