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
        public DuelType Type = DuelType.Reaction; // Reaction (quick-draw) or Timing (sweet-spot bar)
        public bool SkipFirstIntro; // set when a pre-duel dialog already put the duelists on the field
        public List<Duelist> Duelists = new List<Duelist>();

        public DuelPhase Phase { get; private set; } = DuelPhase.Idle;

        double _bangTime;

        public void StartDuel()
        {
            StopAllCoroutines();
            Time.timeScale = 1f;
            foreach (var d in Duelists) d.Lane = d.HomeLane; // undo any tie-break lane change from a prior match (fixes rematch pairing)
            StartCoroutine(Type == DuelType.Timing ? RunTimingMatch() : RunMatch());
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

            yield return StartCoroutine(PlayIntroAndStance(active, doIntro));

            Phase = DuelPhase.Tension;
            foreach (var d in active) d.Input.Arm();
            Audio.StartTension();
            if (Random.value < 0.75f) Tumbleweed.Spawn();

            _bangTime = 0;
            var byLane = BucketByLane(active, out int liveLanes);
            int laneCount = byLane.Length;
            var resolved = new bool[laneCount];
            int resolvedCount = 0;

            // Tension: any tap now is a FALSE START — that lane is settled AT ONCE (jumper loses),
            // no waiting for the hidden timer to finish.
            float tension = Config.RollTension();
            float e = 0f;
            while (e < tension && resolvedCount < liveLanes)
            {
                e += Time.deltaTime;
                TickInputs(active, Time.realtimeSinceStartupAsDouble);
                for (int i = 0; i < laneCount; i++)
                {
                    if (resolved[i] || byLane[i].Count == 0) continue;
                    var g = byLane[i];
                    bool anyFired = false;
                    Duelist held = null;
                    foreach (var d in g)
                    {
                        if (d.Input.HasFired) { d.FalseStarted = true; anyFired = true; }
                        else if (held == null) held = d;
                    }
                    if (!anyFired) continue;
                    DecideLane(g, held); // the one who held survives; both jumped → draw
                    resolved[i] = true;
                    resolvedCount++;
                }
                yield return null;
            }
            Audio.StopTension();

            // BANG — only if a lane is still live.
            if (resolvedCount < liveLanes)
            {
                Phase = DuelPhase.Bang;
                _bangTime = Time.realtimeSinceStartupAsDouble;
                foreach (var d in active) d.Input.OnBang(_bangTime);
                Audio.Bang();
                Hud.ShowBang();
                Shake?.Shake(0.10f, 0.12f);
                Hud.Flash(new Color(1f, 1f, 1f, 0.45f), 0.14f);

                // A lane is DECIDED the instant its first valid shot lands — the shooter wins and the
                // opponent drops at once (they never shoot). We then keep listening a short grace so
                // the slower player's OWN tap time is still captured and shown.
                const float maxWindow = 2f;
                const float graceAfterDecided = 0.6f;
                float w = 0f, grace = -1f;
                while (w < maxWindow)
                {
                    w += Time.deltaTime;
                    double now = Time.realtimeSinceStartupAsDouble;
                    TickInputs(active, now);

                    // Decide before FX so a same-frame loser never PlayShoot + gunshot.
                    for (int i = 0; i < laneCount; i++)
                    {
                        if (resolved[i] || byLane[i].Count == 0) continue;
                        if (!DuelResolve.TryPickLaneWinner(byLane[i], _bangTime, out var winner)) continue;
                        DecideLane(byLane[i], winner); // winner == null → equal times, lane draw
                        resolved[i] = true;
                        resolvedCount++;
                    }
                    FireFxForNew(active); // winner (and anyone still live who just tapped) flashes now

                    if (resolvedCount == liveLanes)
                    {
                        if (grace < 0f) grace = graceAfterDecided;
                        grace -= Time.deltaTime;
                        if (grace <= 0f || AllHaveFired(active)) break; // loser's time captured (or they gave up)
                    }
                    yield return null;
                }

                // Timed out: any undecided lane — nobody drew (no survivor).
                for (int i = 0; i < laneCount; i++)
                    if (!resolved[i] && byLane[i].Count > 0)
                        DecideLane(byLane[i], null);
            }

            // Finalize: compute each reaction time and show every popup — the winner's AND the
            // slower player's own time (captured during the grace window) both appear now.
            bool newRecord = false;
            foreach (var d in active)
            {
                d.ReactionSeconds = (!d.FalseStarted && _bangTime > 0 && d.Input.HasFired && d.Input.FireTimeRealtime >= _bangTime)
                    ? d.Input.FireTimeRealtime - _bangTime
                    : -1;
                Vector3 wp = d.View.PopupAnchor; // pinned above each head (feet-pivoted real art or procedural)
                Hud.ShowReactionAt(wp, ReactionText(d), ReactionColor(d));

                // A human's winning quick-draw counts toward the fastest-reaction record.
                if (d.Kind == DuelistKind.Human && d.Outcome == DuelOutcome.Won && d.ReactionSeconds > 0
                    && Records.ReportReaction((float)d.ReactionSeconds * 1000f))
                    newRecord = true;
            }
            if (newRecord) Hud.ShowBanner("NEW BEST DRAW!", new Color(1f, 0.85f, 0.3f));

            Phase = DuelPhase.Resolved;
            yield return StartCoroutine(ShowRoundResults(active));
        }

        IEnumerator PlayIntroAndStance(List<Duelist> active, bool doIntro)
        {
            Phase = DuelPhase.Intro;
            if (doIntro)
            {
                foreach (var d in active) d.View.SetIdleOffscreen();
                yield return null;
                foreach (var d in active) d.View.BeginWalkIn(Config.introDuration);
                yield return new WaitForSeconds(Config.introDuration);
            }

            Phase = DuelPhase.Stance;
            foreach (var d in active) d.View.Stance();
            yield return new WaitForSeconds(Config.stancePause);
        }

        void TickInputs(List<Duelist> active, double now)
        {
            foreach (var d in active) d.Input.Tick(now);
        }

        /// <summary>
        /// Shoot flash + gunshot for anyone who just fired and is still allowed to
        /// (losers already have ShotFx from <see cref="DecideLane"/>).
        /// </summary>
        void FireFxForNew(List<Duelist> active)
        {
            foreach (var d in active)
                if (d.Input.HasFired && !d.ShotFx)
                {
                    d.ShotFx = true;
                    d.View.PlayShoot();
                    Audio.Gunshot(d.Weapon);
                    Haptics.Light();
                    Shake?.Shake(0.12f, 0.10f);
                }
        }

        /// <summary>Group active duelists by lane once per round — no LINQ in Tension/Bang.</summary>
        static List<Duelist>[] BucketByLane(List<Duelist> active, out int liveLanes)
        {
            int laneCount = 1;
            foreach (var d in active)
                if (d.Lane + 1 > laneCount) laneCount = d.Lane + 1;

            var byLane = new List<Duelist>[laneCount];
            for (int i = 0; i < laneCount; i++)
                byLane[i] = new List<Duelist>(2);
            foreach (var d in active)
                byLane[d.Lane].Add(d);

            liveLanes = 0;
            for (int i = 0; i < laneCount; i++)
                if (byLane[i].Count > 0) liveLanes++;
            return byLane;
        }

        static bool AllHaveFired(List<Duelist> active)
        {
            foreach (var d in active)
                if (!d.Input.HasFired) return false;
            return true;
        }

        /// <summary>
        /// Settles one lane: <paramref name="winner"/> survives; everyone else drops at once
        /// (visual death, blocked from firing). Reaction times + popups are shown later in the
        /// finalize step, so the slower player's own tap time can still be captured first.
        /// A null winner is a draw — nobody in the lane survives.
        /// </summary>
        void DecideLane(List<Duelist> group, Duelist winner)
        {
            bool anyDeath = false;
            foreach (var d in group)
            {
                if (d == winner)
                {
                    d.Outcome = DuelOutcome.Won;
                }
                else
                {
                    d.Outcome = d.FalseStarted ? DuelOutcome.FalseStart : DuelOutcome.Lost;
                    d.ShotFx = true;     // out — no shoot FX
                    d.View.PlayDeath();  // drops the instant the lane is decided
                    anyDeath = true;
                }
            }
            if (anyDeath)
            {
                Audio.Death();
                Haptics.Heavy();
                Shake?.Shake(0.22f, 0.22f);
                Hud.Flash(new Color(1f, 0.92f, 0.82f, 0.30f), 0.10f);
            }
        }

        IEnumerator ShowRoundResults(List<Duelist> active)
        {
            // Shots, deaths and verdict popups already played the instant each lane resolved.
            Time.timeScale = 0f;
            yield return new WaitForSecondsRealtime(0.08f);
            Time.timeScale = 1f;

            yield return new WaitForSeconds(Config.resultDelay);
        }

        // ===================== Timing duel (sweet-spot bar) =====================

        /// <summary>
        /// Timing match: each duelist locks a sweeping pointer, trying to land on the green zone.
        /// PvE = hit green or the opponent shoots you (one round, team must all hit). PvP/Coop =
        /// whoever stops closest to the green centre wins their lane (reuses the tie-break loop).
        /// </summary>
        IEnumerator RunTimingMatch()
        {
            foreach (var d in Duelists) d.ResetRound();
            var active = new List<Duelist>(Duelists);

            if (PveMode)
            {
                yield return StartCoroutine(RunTimingRound(active, !SkipFirstIntro, pve: true));
                var humans = active.Where(d => d.Kind == DuelistKind.Human).ToList();
                bool playerWon = humans.Count > 0 && humans.All(h => h.AimHit);
                Phase = DuelPhase.Result;
                ShowPveResult(playerWon);
                yield break;
            }

            bool firstRound = true;
            DuelSide? winningSide = null;
            bool draw = false;
            while (true)
            {
                yield return StartCoroutine(RunTimingRound(active, firstRound && !SkipFirstIntro, pve: false));
                firstRound = false;

                var survivors = active.Where(d => d.Outcome == DuelOutcome.Won).ToList();
                if (survivors.Count == 0) { draw = true; break; }
                var sides = new HashSet<DuelSide>(survivors.Select(s => s.Side));
                if (sides.Count == 1) { winningSide = survivors[0].Side; break; }

                active = survivors;
                foreach (var d in active) d.Lane = 0;
                yield return new WaitForSeconds(0.6f);
            }

            Phase = DuelPhase.Result;
            ShowFinal(active, winningSide, draw);
        }

        IEnumerator RunTimingRound(List<Duelist> active, bool doIntro, bool pve)
        {
            Hud.HideAll();
            foreach (var d in active) d.ResetRound();

            yield return StartCoroutine(PlayIntroAndStance(active, doIntro));

            // --- Build the bars. Humans always aim; bots only get a (self-aiming) bar in PvP/Coop. ---
            TimingTuning(out float greenHalf, out float sweepSpeed);
            var bars = new Dictionary<Duelist, TimingBar>();
            var botTargetX = new Dictionary<Duelist, float>();
            var botLockTime = new Dictionary<Duelist, float>();

            var barers = active.Where(d => d.Kind == DuelistKind.Human || !pve).ToList();
            var bottom = barers.Where(d => d.Side == DuelSide.Bottom).OrderBy(d => d.Lane).ToList();
            var top = barers.Where(d => d.Side == DuelSide.Top).OrderBy(d => d.Lane).ToList();

            foreach (var d in barers)
            {
                var list = d.Side == DuelSide.Bottom ? bottom : top;
                Vector2 pos = BarPosition(d.Side, list.IndexOf(d), list.Count);
                float gc = Random.Range(greenHalf + 0.08f, 1f - greenHalf - 0.08f);
                // Landscape: bars sit on each player's half, upright (no 180° flip).
                var bar = Hud.AddTimingBar(false, pos, 720f, 84f, gc, greenHalf, d.Label);
                bars[d] = bar;

                if (d.Kind == DuelistKind.Bot)
                {
                    float err = BotAimError(greenHalf);
                    float sign = Random.value < 0.5f ? -1f : 1f;
                    botTargetX[d] = Mathf.Clamp01(gc + sign * err);
                    botLockTime[d] = Random.Range(0.5f, 2.2f);
                }
            }

            foreach (var d in active) if (d.Kind == DuelistKind.Human) d.Input.Arm();
            Audio.StartTension();

            Phase = DuelPhase.Tension;
            const float maxAim = 8f;
            float t = 0f;
            while (t < maxAim)
            {
                t += Time.deltaTime;
                float sweepX = Mathf.PingPong(t * sweepSpeed, 1f);
                double now = Time.realtimeSinceStartupAsDouble;

                foreach (var kv in bars)
                {
                    var d = kv.Key; var bar = kv.Value;
                    if (bar.Locked) continue;
                    if (d.Kind == DuelistKind.Human)
                    {
                        d.Input.Tick(now);
                        if (d.Input.HasFired) { bar.Lock(sweepX); PlayShootFx(d); }
                        else bar.SetSweepX(sweepX);
                    }
                    else if (t >= botLockTime[d]) { bar.Lock(botTargetX[d]); PlayShootFx(d); }
                    else bar.SetSweepX(sweepX);
                }

                if (bars.Values.All(b => b.Locked)) break;
                yield return null;
            }
            Audio.StopTension();

            // A human who never tapped hesitated → force an obvious miss (they get shot).
            foreach (var kv in bars)
            {
                var bar = kv.Value;
                if (bar.Locked) continue;
                float miss = bar.GreenCenter > 0.5f
                    ? Mathf.Clamp01(bar.GreenCenter - (bar.GreenHalf + 0.15f))
                    : Mathf.Clamp01(bar.GreenCenter + (bar.GreenHalf + 0.15f));
                bar.Lock(miss);
            }
            foreach (var kv in bars)
            {
                kv.Key.AimHit = kv.Value.IsHit(kv.Value.LockedX);
                kv.Key.AimError = kv.Value.Error(kv.Value.LockedX);
            }

            yield return new WaitForSeconds(0.35f); // let players read where the pointer stopped

            if (pve) ResolveTimingPve(active);
            else ResolveTimingContest(active);

            bool newAim = false;
            foreach (var kv in bars)
            {
                var d = kv.Key; var bar = kv.Value;
                Vector3 wp = d.View.PopupAnchor; // pinned above each head
                bool hit = bar.IsHit(bar.LockedX);
                bool perfect = hit && bar.Error(bar.LockedX) <= bar.GreenHalf * 0.28f;
                string text = !hit ? "MISS" : perfect ? "PERFECT!" : "HIT";
                Color col = !hit ? new Color(1f, 0.5f, 0.5f)
                          : perfect ? new Color(1f, 0.9f, 0.35f)
                          : new Color(0.3f, 1f, 0.3f);
                Hud.ShowReactionAt(wp, text, col);

                // A human's green hit counts toward the best-accuracy record (1 = dead centre).
                if (d.Kind == DuelistKind.Human && hit
                    && Records.ReportAccuracy(1f - bar.Error(bar.LockedX)))
                    newAim = true;
            }
            if (newAim) Hud.ShowBanner("NEW BEST AIM!", new Color(1f, 0.85f, 0.3f));

            Phase = DuelPhase.Resolved;
            yield return StartCoroutine(ShowRoundResults(active));
        }

        /// <summary>PvE: the team must all land green. Otherwise the opponent(s) shoot the players.</summary>
        void ResolveTimingPve(List<Duelist> active)
        {
            var humans = active.Where(d => d.Kind == DuelistKind.Human).ToList();
            var bots = active.Where(d => d.Kind == DuelistKind.Bot).ToList();
            bool allHit = humans.Count > 0 && humans.All(h => h.AimHit);

            if (allHit)
            {
                foreach (var h in humans) h.Outcome = DuelOutcome.Won;
                foreach (var b in bots) { b.Outcome = DuelOutcome.Lost; b.View.PlayDeath(); }
            }
            else
            {
                foreach (var b in bots) PlayShootFx(b);       // the opponent draws on the fumble
                foreach (var h in humans)
                {
                    h.Outcome = h.AimHit ? DuelOutcome.Won : DuelOutcome.Lost;
                    if (!h.AimHit) h.View.PlayDeath();
                }
            }
            DeathFx();
        }

        /// <summary>PvP/Coop: each lane is its own 1v1 — whoever stopped closest to the green centre
        /// wins their lane; the loser drops. In 2v2 that's two independent duels; if the survivors are
        /// one per side, a tie-break round settles it (in `RunTimingMatch`). (Fair and intended.)</summary>
        void ResolveTimingContest(List<Duelist> active)
        {
            var lanes = active.Select(d => d.Lane).Distinct().ToList();
            bool anyDeath = false;
            foreach (var lane in lanes)
            {
                var g = active.Where(d => d.Lane == lane).ToList();
                var winner = DuelResolve.PickTimingWinner(g); // null → dead heat, nobody survives

                foreach (var d in g)
                {
                    if (d == winner) { d.Outcome = DuelOutcome.Won; }
                    else { d.Outcome = DuelOutcome.Lost; d.View.PlayDeath(); anyDeath = true; }
                }
            }
            if (anyDeath) DeathFx();
        }

        /// <summary>Immediate shoot flash + gunshot for one duelist (timing lock).</summary>
        void PlayShootFx(Duelist d)
        {
            if (d.ShotFx) return;
            d.ShotFx = true;
            d.View.PlayShoot();
            Audio.Gunshot(d.Weapon);
            Haptics.Light();
            Shake?.Shake(0.12f, 0.10f);
        }

        void DeathFx()
        {
            Audio.Death();
            Haptics.Heavy();
            Shake?.Shake(0.22f, 0.22f);
            Hud.Flash(new Color(1f, 0.92f, 0.82f, 0.30f), 0.10f);
        }

        /// <summary>Screen-space anchored position (reference 1920×1080) for a bar on the given side.
        /// Landscape: the left player's bar sits on the left half of the street, the right player's on
        /// the right half, both low so they don't cover the gunslingers' faces.</summary>
        static Vector2 BarPosition(DuelSide side, int index, int count)
        {
            float x = side == DuelSide.Bottom ? -470f : 470f; // Bottom = left of the street, Top = right
            if (count <= 1) return new Vector2(x, -300f);
            // Two humans share a side (Coop) — stack them vertically on that half.
            float y = index == 0 ? -180f : -400f;
            return new Vector2(x, y);
        }

        static void TimingTuning(out float greenHalf, out float sweepSpeed)
        {
            switch (MatchSettings.BotDifficulty)
            {
                case Difficulty.Easy:   greenHalf = 0.130f; sweepSpeed = 0.75f; break;
                case Difficulty.Hard:   greenHalf = 0.055f; sweepSpeed = 1.40f; break;
                default:                greenHalf = 0.085f; sweepSpeed = 1.00f; break;
            }
        }

        /// <summary>A bot's distance-from-centre when it self-aims, banded by difficulty.</summary>
        static float BotAimError(float greenHalf)
        {
            switch (MatchSettings.BotDifficulty)
            {
                case Difficulty.Easy:
                    return Random.value < 0.45f ? Random.Range(0f, greenHalf) : Random.Range(greenHalf, greenHalf + 0.18f);
                case Difficulty.Hard:
                    return Random.value < 0.85f ? Random.Range(0f, greenHalf * 0.7f) : Random.Range(greenHalf, greenHalf + 0.08f);
                default:
                    return Random.value < 0.65f ? Random.Range(0f, greenHalf) : Random.Range(greenHalf, greenHalf + 0.12f);
            }
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

            Hud.ShowResult(msg, "REMATCH", OnRematch, "MENU", DuelFlow.Menu);
        }

        void ShowPveResult(bool playerWon)
        {
            if (playerWon)
            {
                if (Campaign.IsFinalStage)
                {
                    Campaign.EndRun(victory: true);   // clears the save + counts a completion
                    DuelFlow.Story(StoryKind.Victory);
                }
                else if (Campaign.IsLastStageOfChapter)
                {
                    Campaign.AdvanceStage();          // into the next chapter → intro screen (persists)
                    DuelFlow.Story(StoryKind.ChapterIntro);
                }
                else
                {
                    Campaign.AdvanceStage();          // persists progress
                    Hud.ShowResult($"STAGE CLEARED!\nNext: {Campaign.CurrentStage.Title}", "MAP", DuelFlow.Map, "MENU", DuelFlow.Menu);
                }
            }
            else
            {
                Campaign.LoseLife();                  // persists
                if (Campaign.Lives <= 0)
                {
                    Campaign.EndRun(victory: false);
                    DuelFlow.Story(StoryKind.Defeat);
                }
                else
                {
                    Hud.ShowResult($"YOU DIED\nLives left: {Campaign.Lives}", "RETRY", DuelFlow.Duel, "MAP", DuelFlow.Map);
                }
            }
        }

        void OnRematch() => StartDuel();
    }
}
