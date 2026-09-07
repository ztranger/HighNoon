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
                FireFxForNew(active); // instant feedback even on an early (false-start) tap
                yield return null;
            }

            Phase = DuelPhase.Bang;
            _bangTime = Time.realtimeSinceStartupAsDouble;
            foreach (var d in active) d.Input.OnBang(_bangTime);
            Audio.Bang();
            Hud.ShowBang();
            Shake?.Shake(0.10f, 0.12f);
            Hud.Flash(new Color(1f, 1f, 1f, 0.45f), 0.14f);

            // Anyone who tapped before BANG jumped the gun.
            foreach (var d in active)
                d.FalseStarted = d.Input.HasFired && d.Input.FireTimeRealtime < _bangTime;

            var lanes = active.Select(d => d.Lane).Distinct().ToList();
            var resolved = new HashSet<int>();

            // A false start settles its lane at once: the jumper loses, the other survives.
            foreach (var lane in lanes)
            {
                var g = active.Where(d => d.Lane == lane).ToList();
                if (g.Any(d => d.FalseStarted))
                {
                    ResolveLaneInstant(g, g.FirstOrDefault(d => !d.FalseStarted));
                    resolved.Add(lane);
                }
            }

            // Each remaining lane resolves the INSTANT its first valid (post-BANG) shot lands —
            // the shooter wins and the opponent is dead before they draw (they never fire).
            const float maxWindow = 1.5f;
            float w = 0f;
            while (resolved.Count < lanes.Count && w < maxWindow)
            {
                w += Time.deltaTime;
                double now = Time.realtimeSinceStartupAsDouble;
                TickInputs(active, now);
                FireFxForNew(active); // the instant you tap, your gun fires
                foreach (var lane in lanes)
                {
                    if (resolved.Contains(lane)) continue;
                    var g = active.Where(d => d.Lane == lane).ToList();
                    Duelist winner = null;
                    double best = double.MaxValue;
                    foreach (var d in g)
                        if (d.Input.HasFired && d.Input.FireTimeRealtime >= _bangTime && d.Input.FireTimeRealtime < best)
                        { best = d.Input.FireTimeRealtime; winner = d; }
                    if (winner != null) { ResolveLaneInstant(g, winner); resolved.Add(lane); }
                }
                yield return null;
            }

            // Timed out: nobody in the lane drew in time — both are too slow (no survivor).
            foreach (var lane in lanes)
                if (!resolved.Contains(lane))
                    ResolveLaneInstant(active.Where(d => d.Lane == lane).ToList(), null);

            Phase = DuelPhase.Resolved;
            yield return StartCoroutine(ShowRoundResults(active));
        }

        void TickInputs(List<Duelist> active, double now)
        {
            foreach (var d in active) d.Input.Tick(now);
        }

        /// <summary>Immediate shoot flash + gunshot the moment a duelist fires (once each).</summary>
        void FireFxForNew(List<Duelist> active)
        {
            foreach (var d in active)
                if (d.Input.HasFired && !d.ShotFx)
                {
                    d.ShotFx = true;
                    d.View.PlayShoot();
                    Audio.Gunshot();
                    Shake?.Shake(0.12f, 0.10f);
                }
        }

        /// <summary>
        /// Settles one lane immediately: <paramref name="winner"/> survives; everyone else is
        /// out — they fall now and are blocked from firing (the loser never gets a shot off).
        /// </summary>
        void ResolveLaneInstant(List<Duelist> group, Duelist winner)
        {
            bool anyDeath = false;
            foreach (var d in group)
            {
                d.ReactionSeconds = (d.Input.HasFired && d.Input.FireTimeRealtime >= _bangTime)
                    ? d.Input.FireTimeRealtime - _bangTime
                    : -1;

                if (d == winner)
                {
                    d.Outcome = DuelOutcome.Won;
                }
                else
                {
                    d.Outcome = d.FalseStarted ? DuelOutcome.FalseStart : DuelOutcome.Lost;
                    d.ShotFx = true;     // the loser is out — no shoot FX for them
                    d.View.PlayDeath();  // and they drop the instant the winner fires
                    anyDeath = true;
                }
            }
            if (anyDeath)
            {
                Audio.Death();
                Shake?.Shake(0.22f, 0.22f);
                Hud.Flash(new Color(1f, 0.92f, 0.82f, 0.30f), 0.10f);
            }
        }

        IEnumerator ShowRoundResults(List<Duelist> active)
        {
            // Per-duelist reaction popup, above each cowboy (captured before death anims move them).
            foreach (var d in active)
            {
                Vector3 wp = d.View.transform.position + Vector3.up * (d.Side == DuelSide.Bottom ? 1.6f : -1.6f);
                Hud.ShowReactionAt(wp, ReactionText(d), ReactionColor(d));
            }

            // Shots + deaths already played the instant each lane resolved — just a beat, then buttons.
            Time.timeScale = 0f;
            yield return new WaitForSecondsRealtime(0.08f);
            Time.timeScale = 1f;

            yield return new WaitForSeconds(Config.resultDelay);
        }

        static string ReactionText(Duelist d)
        {
            if (d.Outcome == DuelOutcome.FalseStart) return "FALSE START";
            if (!d.Fired) return "—";
            return $"{Mathf.Max(0f, (float)d.ReactionSeconds) * 1000f:0} ms";
        }

        static Color ReactionColor(Duelist d)
        {
            if (d.Outcome == DuelOutcome.FalseStart) return new Color(1f, 0.6f, 0.1f);
            if (!d.Fired) return new Color(1f, 0.5f, 0.5f);
            return d.Outcome == DuelOutcome.Won ? new Color(0.3f, 1f, 0.3f) : new Color(1f, 0.5f, 0.5f);
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
