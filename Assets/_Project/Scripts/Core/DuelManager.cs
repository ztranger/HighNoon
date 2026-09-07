using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HighNoon
{
    /// <summary>
    /// Orchestrates one duel as a coroutine state machine:
    /// Intro -> Stance -> Tension (hidden countdown) -> Bang -> Resolve -> Result.
    /// Fires are compared by timestamp; the earliest valid tap wins. A tap during
    /// the tension window is a false start and loses the duel.
    /// </summary>
    public class DuelManager : MonoBehaviour
    {
        public DuelConfig Config;
        public DuelHUD Hud;
        public DuelAudio Audio;
        public List<Duelist> Duelists = new List<Duelist>();

        public DuelPhase Phase { get; private set; } = DuelPhase.Idle;

        double _bangTime;

        public void StartDuel()
        {
            StopAllCoroutines();
            StartCoroutine(RunDuel());
        }

        IEnumerator RunDuel()
        {
            Phase = DuelPhase.Intro;
            foreach (var d in Duelists) d.ResetRound();
            Hud.HideAll();

            // Intro: walk in from off-screen.
            foreach (var d in Duelists) d.View.SetIdleOffscreen();
            yield return null;
            foreach (var d in Duelists) StartCoroutine(d.View.WalkIn(Config.introDuration));
            yield return new WaitForSeconds(Config.introDuration);

            // Stance.
            Phase = DuelPhase.Stance;
            foreach (var d in Duelists) d.View.Stance();
            yield return new WaitForSeconds(Config.stancePause);

            // Tension: arm inputs, build tension, watch for false starts.
            Phase = DuelPhase.Tension;
            foreach (var d in Duelists) d.Input.Arm();
            Audio.StartTension();

            float tension = Config.RollTension();
            float elapsed = 0f;
            bool falseStart = false;
            while (elapsed < tension)
            {
                elapsed += Time.deltaTime;
                double now = Time.realtimeSinceStartupAsDouble;
                TickInputs(now);
                foreach (var d in Duelists)
                {
                    if (d.Input.HasFired) { d.FalseStarted = true; falseStart = true; }
                }
                if (falseStart) break;
                yield return null;
            }

            if (falseStart)
            {
                Audio.StopTension();
                Phase = DuelPhase.Resolved;
                ResolveFalseStart();
                yield return StartCoroutine(ShowResults());
                yield break;
            }

            // BANG!
            Phase = DuelPhase.Bang;
            _bangTime = Time.realtimeSinceStartupAsDouble;
            foreach (var d in Duelists) d.Input.OnBang(_bangTime);
            Audio.Bang();
            Hud.ShowBang();

            // Wait until everyone has fired, or the reaction window elapses.
            const float maxWindow = 3f;
            float w = 0f;
            while (w < maxWindow)
            {
                w += Time.deltaTime;
                double now = Time.realtimeSinceStartupAsDouble;
                TickInputs(now);
                bool allFired = true;
                foreach (var d in Duelists) if (!d.Input.HasFired) { allFired = false; break; }
                if (allFired) break;
                yield return null;
            }

            Phase = DuelPhase.Resolved;
            ResolveNormal();
            yield return StartCoroutine(ShowResults());
        }

        void TickInputs(double now)
        {
            foreach (var d in Duelists) d.Input.Tick(now);
        }

        void ResolveFalseStart()
        {
            foreach (var d in Duelists)
                d.Outcome = d.FalseStarted ? DuelOutcome.FalseStart : DuelOutcome.Won;
        }

        void ResolveNormal()
        {
            Duelist winner = null;
            double best = double.MaxValue;
            foreach (var d in Duelists)
            {
                if (d.Input.HasFired)
                {
                    d.ReactionSeconds = d.Input.FireTimeRealtime - _bangTime;
                    if (d.ReactionSeconds < best) { best = d.ReactionSeconds; winner = d; }
                }
                else
                {
                    d.ReactionSeconds = -1;
                }
            }
            foreach (var d in Duelists)
                d.Outcome = (d == winner) ? DuelOutcome.Won : DuelOutcome.Lost;
        }

        IEnumerator ShowResults()
        {
            bool anyGunshot = false;
            foreach (var d in Duelists)
            {
                bool win = d.Outcome == DuelOutcome.Won;
                if (win) { d.View.PlayShoot(); anyGunshot = true; }
                else { d.View.PlayDeath(); }

                Hud.ShowReaction(
                    d.Side,
                    Mathf.Max(0f, (float)d.ReactionSeconds),
                    win,
                    d.Outcome == DuelOutcome.FalseStart,
                    d.Fired);
            }
            if (anyGunshot) Audio.Gunshot();
            Audio.Death();

            yield return new WaitForSeconds(Config.resultDelay);

            Phase = DuelPhase.Result;
            Hud.ShowResult(BuildResultMessage(), OnRematch, OnMenu);
        }

        string BuildResultMessage()
        {
            foreach (var d in Duelists)
                if (d.Outcome == DuelOutcome.Won)
                    return $"{d.Label} WINS!";
            return "DRAW";
        }

        void OnRematch() => StartDuel();

        void OnMenu()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        }
    }
}
