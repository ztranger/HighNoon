using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HighNoon
{
    /// <summary>
    /// Runs a match as one or more rounds. Duelists are paired by <see cref="Duelist.Lane"/>
    /// (Bottom vs Top). Each round: Intro → Stance → Tension → Bang → resolve every lane.
    /// A tap before BANG is a false start (that duelist loses their lane).
    ///
    /// 1v1 = a single lane. 2v2 = two lanes resolved in parallel; if each side wins one
    /// lane, the two survivors duel again (tie-break round) until one side owns the match.
    /// </summary>
    public class DuelManager : MonoBehaviour
    {
        public DuelConfig Config;
        public DuelHUD Hud;
        public DuelAudio Audio;
        public CameraShake Shake;
        public bool PveMode;
        public bool SkipFirstIntro; // set when a pre-duel dialog already put the duelists on the field
        public List<Duelist> Duelists = new List<Duelist>();

        public DuelPhase Phase { get; private set; } = DuelPhase.Idle;

        double _bangTime;

        public void StartDuel()
        {
            StopAllCoroutines();
            Time.timeScale = 1f;
            StartCoroutine(RunMatch());
        }

        IEnumerator RunMatch()
        {
            foreach (var d in Duelists) d.ResetRound();

            var active = new List<Duelist>(Duelists);
            bool firstRound = true;
            DuelSide? winningSide = null;
            bool draw = false;

            while (true)
            {
                yield return StartCoroutine(RunRound(active, firstRound && !SkipFirstIntro));
                firstRound = false;

                var survivors = active.Where(d => d.Outcome == DuelOutcome.Won).ToList();
                if (survivors.Count == 0) { draw = true; break; }

                var sides = new HashSet<DuelSide>(survivors.Select(s => s.Side));
                if (sides.Count == 1) { winningSide = survivors[0].Side; break; }

                // Split decision → tie-break round between the survivors (pair them up).
                active = survivors;
                foreach (var d in active) d.Lane = 0;
                yield return new WaitForSeconds(0.6f);
            }

            Phase = DuelPhase.Result;
            ShowFinal(active, winningSide, draw);
        }

        IEnumerator RunRound(List<Duelist> active, bool doIntro)
        {
            Hud.HideAll();
            foreach (var d in active) d.ResetRound();

            Phase = DuelPhase.Intro;
            if (doIntro)
            {
                foreach (var d in active) d.View.SetIdleOffscreen();
                yield return null;
                foreach (var d in active) StartCoroutine(d.View.WalkIn(Config.introDuration));
                yield return new WaitForSeconds(Config.introDuration);
            }

            Phase = DuelPhase.Stance;
            foreach (var d in active) d.View.Stance();
            yield return new WaitForSeconds(Config.stancePause);

            Phase = DuelPhase.Tension;
            foreach (var d in active) d.Input.Arm();
            Audio.StartTension();
            if (Random.value < 0.75f) Tumbleweed.Spawn();

            float tension = Config.RollTension();
            float e = 0f;
            while (e < tension)
            {
                e += Time.deltaTime;
                TickInputs(active, Time.realtimeSinceStartupAsDouble);
                yield return null;
            }

            Phase = DuelPhase.Bang;
            _bangTime = Time.realtimeSinceStartupAsDouble;
            foreach (var d in active) d.Input.OnBang(_bangTime);
            Audio.Bang();
            Hud.ShowBang();
            Shake?.Shake(0.10f, 0.12f);
            Hud.Flash(new Color(1f, 1f, 1f, 0.45f), 0.14f);

            const float maxWindow = 3f;
            float w = 0f;
            while (w < maxWindow)
            {
                w += Time.deltaTime;
                TickInputs(active, Time.realtimeSinceStartupAsDouble);
                if (active.All(d => d.Input.HasFired)) break;
                yield return null;
            }

            Phase = DuelPhase.Resolved;
            ResolveLanes(active);
            yield return StartCoroutine(ShowRoundResults(active));
        }

        void TickInputs(List<Duelist> active, double now)
        {
            foreach (var d in active) d.Input.Tick(now);
        }

        /// <summary>Classifies fires and picks a winner for each lane in the active set.</summary>
        void ResolveLanes(List<Duelist> active)
        {
            foreach (var d in active)
            {
                if (d.Input.HasFired)
                {
                    d.ReactionSeconds = d.Input.FireTimeRealtime - _bangTime;
                    d.FalseStarted = d.ReactionSeconds < 0;
                }
                else
                {
                    d.ReactionSeconds = -1;
                    d.FalseStarted = false;
                }
            }

            foreach (var lane in active.Select(d => d.Lane).Distinct())
            {
                var group = active.Where(d => d.Lane == lane).ToList();

                // Winner = the earliest valid (post-BANG) shot.
                Duelist winner = null;
                double best = double.MaxValue;
                foreach (var d in group)
                    if (d.Input.HasFired && !d.FalseStarted && d.ReactionSeconds < best)
                    {
                        best = d.ReactionSeconds;
                        winner = d;
                    }

                // Nobody clean: the least-bad shot (fired latest) survives, if anyone fired.
                if (winner == null)
                    foreach (var d in group)
                        if (d.Input.HasFired &&
                            (winner == null || d.Input.FireTimeRealtime > winner.Input.FireTimeRealtime))
                            winner = d;

                foreach (var d in group)
                    d.Outcome = d == winner
                        ? DuelOutcome.Won
                        : (d.FalseStarted ? DuelOutcome.FalseStart : DuelOutcome.Lost);
            }
        }

        IEnumerator ShowRoundResults(List<Duelist> active)
        {
            bool anyGunshot = false;
            foreach (var d in active)
            {
                if (d.Outcome == DuelOutcome.Won) { d.View.PlayShoot(); anyGunshot = true; }
                else { d.View.PlayDeath(); }
            }

            // One reaction popup per side: the representative (winner, else fastest shot).
            foreach (var side in new[] { DuelSide.Bottom, DuelSide.Top })
            {
                var group = active.Where(d => d.Side == side).ToList();
                if (group.Count == 0) continue;
                var rep = group.FirstOrDefault(d => d.Outcome == DuelOutcome.Won)
                          ?? group.OrderBy(d => d.Input.HasFired ? d.ReactionSeconds : double.MaxValue).First();
                Hud.ShowReaction(side, Mathf.Max(0f, (float)rep.ReactionSeconds),
                    rep.Outcome == DuelOutcome.Won, rep.Outcome == DuelOutcome.FalseStart, rep.Input.HasFired);
            }

            if (anyGunshot)
            {
                Audio.Gunshot();
                Shake?.Shake(0.32f, 0.32f);
                Hud.Flash(new Color(1f, 1f, 1f, 0.6f), 0.12f);
            }
            Audio.Death();

            // Hit-stop for impact — fx use unscaled time so they keep animating.
            Time.timeScale = 0f;
            yield return new WaitForSecondsRealtime(0.10f);
            Time.timeScale = 1f;

            yield return new WaitForSeconds(Config.resultDelay);
        }

        void ShowFinal(List<Duelist> lastRound, DuelSide? winningSide, bool draw)
        {
            string msg;
            if (draw || winningSide == null)
            {
                msg = "DRAW";
            }
            else if (Duelists.Count <= 2)
            {
                var champ = lastRound.FirstOrDefault(d => d.Outcome == DuelOutcome.Won);
                msg = champ != null ? $"{champ.Label} WINS!" : "DRAW";
            }
            else
            {
                msg = winningSide == DuelSide.Bottom ? "TEAM 1 WINS!" : "TEAM 2 WINS!";
            }

            if (PveMode)
            {
                ShowPveResult(!draw && winningSide == DuelSide.Bottom);
                return;
            }

            Hud.ShowResult(msg, "REMATCH", OnRematch, "MENU", OnMenu);
        }

        void ShowPveResult(bool playerWon)
        {
            if (playerWon)
            {
                if (Campaign.IsFinalStage)
                {
                    Campaign.Active = false;
                    GoStory(StoryKind.Victory);
                }
                else if (Campaign.IsLastStageOfChapter)
                {
                    Campaign.AdvanceStage();          // into the next chapter → intro screen
                    GoStory(StoryKind.ChapterIntro);
                }
                else
                {
                    Campaign.AdvanceStage();
                    Hud.ShowResult($"STAGE CLEARED!\nNext: {Campaign.CurrentStage.Title}", "MAP", LoadMap, "MENU", OnMenu);
                }
            }
            else
            {
                Campaign.Lives--;
                if (Campaign.Lives <= 0)
                {
                    Campaign.Active = false;
                    GoStory(StoryKind.Defeat);
                }
                else
                {
                    Hud.ShowResult($"YOU DIED\nLives left: {Campaign.Lives}", "RETRY", ReloadDuel, "MAP", LoadMap);
                }
            }
        }

        void OnRematch() => StartDuel();
        void OnMenu() => UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        void LoadMap() => UnityEngine.SceneManagement.SceneManager.LoadScene("Map");
        void ReloadDuel() => UnityEngine.SceneManagement.SceneManager.LoadScene("Duel");

        void GoStory(StoryKind kind)
        {
            Story.Kind = kind;
            UnityEngine.SceneManagement.SceneManager.LoadScene("Story");
        }
    }
}
