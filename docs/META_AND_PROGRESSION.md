# High Noon — Meta, Progression & Monetization design

**Status:** PROPOSAL — nothing here is implemented yet. This is a direction doc for future AI agents / devs.
**Created:** 2026-09-09.
**Scope:** the "meta" layer that lives *around* the core reaction/timing duel — the systems that keep players
coming back over weeks and that carry monetization. Written in English to match the codebase & `CLAUDE.md`.

Read `CLAUDE.md` first for how the game is built (code-first bootstraps, `namespace HighNoon`, PlayerPrefs
save, the 5-tab menu, `DuelManager`, `Records`, `Weapons`, `Campaign`, `AudioBank`, etc.). This doc references
those systems by name — reuse them, don't reinvent.

Engineering prerequisite: [ARCHITECTURE_REVIEW.md](ARCHITECTURE_REVIEW.md) (reaction timestamps, BANG-window GC,
texture leaks, consolidated save). Do not start the pillars below until that doc's P0 is done — ranked / pass
on top of a dishonest millisecond clock is a trap.

---

## 0. Design principles (do not violate)

1. **This is a skill / reaction game → NO pay-to-win.** Nothing that changes the *outcome* of a duel may be
   bought or grinded into an advantage. Never sell / never let progression buff: reaction speed, the green-zone
   width or sweep speed (`TimingTuning`), the hidden tension window, bot difficulty in ranked, "auto-win", etc.
   Anything competitive (PvP / ranked / leaderboards) must be settled purely by player skill.
2. **Monetize cosmetics, convenience, and the pass** — not power. Convenience = ×2 rewards, an extra continue,
   a bounty reroll, remove-ads. This keeps whales spending while skilled F2P players never feel cheated.
3. **Ad philosophy:** **rewarded** ads are opt-in and generous (the workhorse); **interstitials** are rare and
   only at natural stops (after a gauntlet run / a lost campaign life), always removable via a `Remove Ads` IAP.
   No forced ads mid-duel — duels are seconds long and latency-sensitive.
4. **Session shape:** a duel lasts *seconds*. Meta must reward very short bursts and, above all, **daily return**
   (streaks, dailies, a shop that rotates, a pass that ticks).
5. **Respect the tech:** save today is scattered PlayerPrefs (`GameSettings`, `Records`, `Campaign`). Anything
   online (ranked, leaderboards, cloud cosmetics) needs a consolidated `SaveData` + a backend later
   (Firebase / Play Games / custom). Local-first is fine to start; design for a cloud sync seam.

---

## 1. Cross-cutting foundations (build these FIRST — the other pillars depend on them)

- **Economy / currency.** A soft currency (working name **"Bounty $"** / gold) earned from every duel, bounty,
  gauntlet run, campaign clear. Optionally a hard currency (**"Gold Bars"**, bought via IAP) for premium shop /
  pass skip. Define **sources** (duel win, daily, gauntlet depth, pass tiers, rewarded ads) and **sinks**
  (crates, shop, cosmetics, streak-save, bounty reroll) so the economy stays balanced.
- **Consolidated save + grant service.** A single `SaveData` (currency, owned cosmetics, equipped loadout, XP,
  rank, pass tier, streak, daily state, ranked rating). Keep the PlayerPrefs mirrors during transition; add a
  seam for cloud sync. One `Economy.Grant(...)` / `Economy.Spend(...)` API so every reward flows through one place.
- **Analytics events.** Duel start/win/loss + reaction ms + accuracy, daily login, streak length, ad
  shown/rewarded, IAP, currency source/sink, session length. Retention/monetization can't be tuned blind.
- **Reward popup / "you earned X" flow.** One reusable UI (fits the `DuelHUD.ShowBanner` / result-panel style).

These aren't glamorous but every pillar reads/writes them.

---

## 2. The five pillars

Each: **Concept · Why it fits · Retention hook · Monetization hook · Plugs into · Effort · Dependencies · Open questions.**

### Pillar A — Gunslinger Reputation (XP + Ranks) + Season Pass  ★ backbone
- **Concept:** a persistent XP track earned per duel / campaign stage / bounty. Rank titles:
  *Greenhorn → Gunslinger → Outlaw → Legend → Most Wanted*. On top, a **Season Pass** (Battle Pass) with a free
  and a premium reward lane over ~6–8 weeks.
- **Why it fits:** gives the skill loop a long-term number that always moves; a season is a recurring reason to
  return; it's the rail every other reward (cosmetics, currency) rides on.
- **Retention:** ever-present "one more duel to level up" bar; the season timer creates urgency and re-engagement.
- **Monetization:** the premium pass is the single most reliable casual-game revenue line; plus XP boosters
  (IAP or a rewarded-ad "×2 XP for 15 min"); premium-currency **tier skip**.
- **Plugs into:** XP source already exists (duels, `Campaign`, `Records`). Add an XP/rank service + a **STATS**
  tab section (or a new tab) showing rank + pass progress.
- **Effort:** Medium. **Dependencies:** economy + save (§1).
- **Open questions:** season length? pass price? reset ranks each season or keep lifetime rank + seasonal ladder?

### Pillar B — Arsenal & Cosmetics (crates + rotating shop)  ★ primary spend sink
- **Concept:** turn today's 5 sound-only weapons into a **collection** — guns, hats / ponchos / bandanas
  (`CowboyLook` slots), pre-duel emotes, victory effects (muzzle-flash color, tumbleweed skin, win pose).
  Acquire via **"Supply Crates"** (currency / rewarded-ad / IAP) and a **daily-rotating shop**.
- **Why it fits:** Wild-West fantasy is *made* for cosmetics; the game already has procedural customization
  (`WeaponArt`, `CowboyLook`) to extend into rarities and slots — cheap content velocity.
- **Retention:** "complete the collection," rarities, daily new shop items, limited-time themed bundles.
- **Monetization:** direct-buy skins, crates, "unlock via rewarded-ad," themed bundles — cosmetic whale revenue
  with **zero** pay-to-win. Pass rewards are cosmetics too (feeds Pillar A).
- **Plugs into:** `WeaponDef` (add icon/skin variants + rarity), `CowboyLook` (slots: hat / body / gun / effect),
  the **GUNS** tab → grow into a full "Arsenal / Wardrobe". `WeaponArt` proves procedural icons work.
- **Effort:** Medium–High (inventory + equip + shop UI). **Dependencies:** economy + save (§1).
- **Open questions:** how many slots? crate odds & rarity tiers? procedural vs hand-made art pipeline?

### Pillar C — Wanted Board (daily bounties + login streak + outlaw of the day)  ★ cheapest retention win
- **Concept:** a **"Wanted Board"** with ~3 rotating daily bounties ("win 3 duels under 250 ms", "5 PERFECT
  greens in Timing", "beat a Hard bot"), a **daily-login streak** ladder with escalating rewards, and a named
  **"Outlaw of the Day"** with a bigger prize.
- **Why it fits:** cheap to build off already-tracked data (`Records`, duel outcomes) and thematically perfect
  (wanted posters). It is the single strongest *daily* return driver.
- **Retention:** streak + daily reset = habit formation; bounties give a reason to play *today*.
- **Monetization:** rewarded-ad **reroll a bounty** / **×2 bounty reward**; IAP **"save my streak"**.
- **Plugs into:** reads `Records`/`DuelManager` results; a new **daily state** in save; a "Wanted Board" screen
  (could be the **STATS** tab or a new one).
- **Effort:** Low–Medium (biggest bang for the buck). **Dependencies:** economy + save (§1).
- **Open questions:** how many bounties/day? reroll cost? streak reward curve & catch-up?

### Pillar D — Ranked Async PvP ("ghost" duels) + seasonal leaderboards
- **Concept:** live online is hard, so go **asynchronous** — duel recorded "ghosts" (other players' reaction /
  timing samples). A ranked ladder *Bronze → … → Outlaw King* with **seasonal resets** and rewards; global +
  friends leaderboards (best reaction ms). Delivers the roadmap's **leaderboards** item.
- **Why it fits:** a reaction game's deepest long-term hook is *competition*; async keeps it feasible without
  real-time netcode. "Beat your rival" is a powerful returner.
- **Retention:** competitive ladder + seasonal reset = the strongest long-tail retention for skill games.
- **Monetization:** ranked season pass, rewarded-ad on rank-up, entries to special events. **No pay-to-win** —
  ranked outcome is pure reaction; only cosmetics/pass are sold.
- **Plugs into:** `Records` (best reaction) → leaderboard; `DuelManager` produces the ghost samples; ranked
  rating in save.
- **Effort:** High — **needs a backend** (Firebase / Play Games / custom) for ghosts + leaderboards + cloud
  save. **Can start** with locally-simulated ghosts (from difficulty bands) and swap to real data later.
- **Open questions:** backend choice? ghost data model & anti-cheat (server-validate reaction times)? seasons.

### Pillar E — "High Noon Gauntlet" (endless / boss-rush with continues)
- **Concept:** a standalone mode — a run of escalating duels, **one miss = run over**, score = how far you got.
  Feeds XP / currency / rank (Pillars A–C).
- **Why it fits:** "one more run, beat my record" is the canonical loop for a reaction game; reuses the entire
  duel machine — cheap to build.
- **Retention:** personal-best chasing + weekly gauntlet leaderboard; a quick, self-contained daily habit.
- **Monetization:** the **most natural ad/IAP spot** — a **"Continue?"** after death via one rewarded-ad
  (or an IAP revive); a sparse interstitial between runs (removable via Remove Ads).
- **Plugs into:** reuses `DuelManager` (set "one life," escalate difficulty each round), a new score + records
  entry, a HOME/mode button.
- **Effort:** Low (mostly a `DuelManager` run-mode + a score screen). **Dependencies:** economy + save (§1) for
  rewards; works standalone even before them.
- **Open questions:** difficulty ramp curve? continue price/limit per run? separate leaderboard from ranked?

---

## 3. Monetization matrix

| Placement | Rewarded ad | Interstitial | IAP |
|---|---|---|---|
| Season Pass | ×2 XP booster | — | **Premium pass**, tier skip |
| Cosmetics / Arsenal | Unlock a crate | — | Skins, crates, bundles, hard currency |
| Wanted Board | Reroll / ×2 bounty | — | Save streak |
| Gauntlet | **Continue after death** | Between runs (sparse) | Revive, Remove Ads |
| Global | Daily free crate/coins | After lost campaign life (sparse) | **Remove Ads**, currency packs |

Rule of thumb: **rewarded = opt-in & generous** (drives engagement + eCPM), **interstitial = rare & removable**,
**IAP = cosmetics / convenience / pass only** (never power).

---

## 4. Recommended build order

1. **Foundations (§1):** currency + consolidated save + grant/analytics seams. *(Blocks everything.)*
2. **Wanted Board / dailies (Pillar C):** cheapest, biggest immediate retention lift; validates the loop.
3. **Arsenal & Cosmetics (Pillar B):** the place to *spend* currency → closes the earn→spend loop; whale revenue.
4. **Reputation + Season Pass (Pillar A):** wraps 2–3 in a long-term rail + the main revenue line.
5. **Gauntlet (Pillar E):** light code; adds a mode, ad points, and a fresh goal; can slot in earlier if desired.
6. **Ranked async PvP + leaderboards (Pillar D):** once a backend exists; the long competitive tail.

---

## 5. Integration notes (existing systems to reuse)

- **`Records` (PlayerPrefs):** already tracks best reaction / accuracy / completions / furthest — the seed for
  leaderboards (D), bounty checks (C), and the STATS tab. Extend, don't duplicate.
- **`Weapons` / `WeaponDef` / `WeaponArt`:** the base for the Arsenal (B). Add rarity, unlock state, skin variants,
  more slots (`CowboyLook`: hat/body/gun/effect).
- **`Campaign`:** an XP + currency + bounty source; already persists a resumable run.
- **`MainMenuBootstrap` (5 tabs):** natural homes — STATS→rank/records/wanted board, GUNS→Arsenal/Wardrobe,
  HOME→mode buttons incl. Gauntlet/Ranked, SETUP/SOUND unchanged. Tabs make room for meta without new scenes.
- **`DuelManager`:** produces reaction ms / accuracy (rewards, ghosts, bounties). Gauntlet = a `DuelManager`
  run-mode (one life, escalating). Ranked = duel vs a ghost input source (new `IDuelInput`).
- **`AudioBank` / `Sfx`:** reward stings, crate-open SFX (folder-per-sound pattern already established).
- **`GameSettings` (PlayerPrefs):** template for how to persist new prefs; migrate toward one `SaveData`.

---

## 6. Decisions needed from the product owner (before deep implementation)

- Currency model: one soft currency, or soft + hard (premium)?
- Backend for online (ranked/leaderboards/cloud save): Firebase? Play Games? custom? (gates Pillar D.)
- Ad network & IAP: which SDK (AdMob / LevelPlay / Unity LevelPlay), store setup (Google Play / App Store).
- Season length & pass price; cosmetic content pipeline (procedural vs hand-drawn).
- Target markets / pricing; is a `Remove Ads` one-time IAP the anchor?

## 7. Out of scope / anti-patterns (avoid)

- **Hard energy/lives gating** on core play (kills a fast reaction game's session cadence). Campaign lives are OK
  as-is; don't paywall standard duels behind energy.
- **Any pay-to-win** (see §0.1) — especially in ranked/PvP.
- **Forced/mid-duel ads** — never interrupt a latency-sensitive duel.
- **Loot boxes with real-money-only odds** in regions where restricted — prefer currency/rewarded paths + clear odds.
