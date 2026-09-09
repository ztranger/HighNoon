using System.Collections.Generic;
using UnityEngine;

namespace HighNoon
{
    /// <summary>
    /// Pure decide-step helpers for reaction lanes and timing contests.
    /// Kept off <see cref="DuelManager"/> so Edit Mode tests can cover ties / false starts
    /// without spinning up a MonoBehaviour coroutine.
    /// </summary>
    public static class DuelResolve
    {
        /// <summary>
        /// Earliest valid post-BANG fire in the group. Equal best times are a lane draw
        /// (<paramref name="winner"/> is null) — not a list-order win.
        /// Returns false if nobody has a valid fire yet.
        /// </summary>
        public static bool TryPickLaneWinner(List<Duelist> group, double bangTime, out Duelist winner)
        {
            winner = null;
            double best = double.MaxValue;
            int bestCount = 0;
            foreach (var d in group)
            {
                if (d.Input == null || !d.Input.HasFired) continue;
                double t = d.Input.FireTimeRealtime;
                if (t < bangTime) continue;
                if (t < best)
                {
                    best = t;
                    winner = d;
                    bestCount = 1;
                }
                else if (t == best)
                {
                    bestCount++;
                }
            }
            if (bestCount == 0) return false;
            if (bestCount > 1) winner = null;
            return true;
        }

        /// <summary>
        /// PvP/Coop timing: whoever stopped closest to the green centre wins the lane.
        /// Exact tie (within 0.0001) → nobody survives.
        /// </summary>
        public static Duelist PickTimingWinner(List<Duelist> group)
        {
            Duelist winner = null;
            float best = float.MaxValue;
            bool tie = false;
            foreach (var d in group)
            {
                if (d.AimError < best - 0.0001f) { best = d.AimError; winner = d; tie = false; }
                else if (Mathf.Abs(d.AimError - best) <= 0.0001f) tie = true;
            }
            return tie ? null : winner;
        }
    }
}
