namespace HighNoon
{
    /// <summary>Runtime state for one cowboy in a duel (player or bot).</summary>
    public class Duelist
    {
        public DuelSide Side;
        public int Lane;              // 0..n within a side (used for 2v2); a tie-break sets it to 0
        public int HomeLane;          // the originally-assigned lane; Lane is reset to this each match
        public DuelistKind Kind;
        public IDuelInput Input;
        public DuelistView View;
        public string Label = "Cowboy";
        public WeaponDef Weapon;      // gun sound this duelist fires (player = chosen, bot = default)

        // Per-round result
        public DuelOutcome Outcome = DuelOutcome.None;
        public double ReactionSeconds = -1;
        public bool FalseStarted;
        public bool ShotFx;   // immediate shoot feedback already played for this fire

        // Timing-duel result (sweet-spot bar): whether the locked pointer landed on green
        // and its distance (0..1) from the green centre. -1 = did not aim this round.
        public bool AimHit;
        public float AimError = -1f;

        public bool Fired => Input != null && Input.HasFired;

        public void ResetRound()
        {
            Outcome = DuelOutcome.None;
            ReactionSeconds = -1;
            FalseStarted = false;
            ShotFx = false;
            AimHit = false;
            AimError = -1f;
            Input?.ResetInput();
        }
    }
}
