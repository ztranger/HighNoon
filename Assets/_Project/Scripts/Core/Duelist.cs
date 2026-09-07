namespace HighNoon
{
    /// <summary>Runtime state for one cowboy in a duel (player or bot).</summary>
    public class Duelist
    {
        public DuelSide Side;
        public int Lane;              // 0..n within a side (used later for 2v2)
        public DuelistKind Kind;
        public IDuelInput Input;
        public DuelistView View;
        public string Label = "Cowboy";

        // Per-round result
        public DuelOutcome Outcome = DuelOutcome.None;
        public double ReactionSeconds = -1;
        public bool FalseStarted;
        public bool ShotFx;   // immediate shoot feedback already played for this fire

        public bool Fired => Input != null && Input.HasFired;

        public void ResetRound()
        {
            Outcome = DuelOutcome.None;
            ReactionSeconds = -1;
            FalseStarted = false;
            ShotFx = false;
            Input?.ResetInput();
        }
    }
}
