# High Noon — Architecture review & agent backlog

**Status:** SOURCE OF TRUTH for follow-up engineering. Findings are from a full pass of `Assets/_Project/Scripts/` (~42 C# files), not from speculation.
**Created:** 2026-09-09.
**Scope:** architecture, correctness, latency, memory. Not a feature wishlist — for retention/monetization see [META_AND_PROGRESSION.md](META_AND_PROGRESSION.md).
**Language:** English, to match the codebase and `CLAUDE.md`.

Read `CLAUDE.md` first. This doc tells you what is *wrong or will not scale*, and in which order to fix it. Do **not** rewrite the game. Do **not** introduce ECS / DOTS / a DI container. The code-first bootstrap style stays.

---

## 0. How agents should use this

1. Pick the highest unchecked item in **§6 Priority backlog**.
2. Keep the change small and focused (one concern per MR).
3. When you fix an item, mark it `[x]` here and note the files touched.
4. Do **not** start meta/economy work until P0 latency + memory items are done — a reaction game that lies about milliseconds cannot carry a ranked / pass layer.
5. Android APK builds are the user's job. Verify in Editor; call out anything that is device-only (haptics, touch timestamps).

---

## 1. Verdict

This is a **coherent vertical slice**, not a messy prototype. ~42 scripts, one namespace (`HighNoon`), a readable duel state machine, and a real Input/View split. Code-first bootstraps (one assembler per scene) are the right call for MCP-driven iteration.

It will **not** carry the meta layer in `META_AND_PROGRESSION.md` without a consolidated save seam and without growing UI out of the giant bootstrap classes. The **gameplay** risk is earlier: reaction fairness on device (frame-quantized human time vs sub-frame bot time, GC/JNI in the BANG window, leaked `Texture2D`s).

**Do not replace the architecture.** Tighten the hot path, plug leaks, then cut seams (save, scene routing, shared intro ritual) before adding economy/IAP.

---

## 2. What is working (do not "fix")

Keep these. Refactors should preserve the behavior.

| Piece | Why it is right |
|---|---|
| `IDuelInput` → `HumanDuelInput` / `BotDuelInput` | Manager does not care if the trigger is a finger or a timer. |
| Round machine `Intro → Stance → Tension → Bang → Resolved → Result` | Readable; tension is audio-only (deliberate — no visible countdown). |
| Instant shot FX (`FireFxForNew`) + ~0.6s grace to capture the loser's time | Feel decision, not a bug. |
| Lane + Side model | 1v1, Coop 2v2, and tie-break are one loop. |
| Per-scene bootstrap + `AppInit.EnsureAudioListener` | 2D Renderer + no-listener scenes are documented gotchas; bootstraps already handle them. |
| `DuelConfig` / `BotConfig` as ScriptableObjects | Pacing/bot reaction stay data. Runtime `CreateInstance` when unassigned is OK for Play-from-Duel-scene. |
| Static `MatchSettings` as a scene-to-scene carrier | Fine until a real session/save object exists. Do not invent a GameObject singleton "just because". |
| Procedural `CowboyArt` / `CowboyLook` | Cheap opponent variety; swap for sheets later without touching `DuelManager`. |
| `AudioBank` folder-of-variants + procedural fallback | Game always has sound; clips are cached after first `Resources.LoadAll`. |

Deliberate product constraints (from `CLAUDE.md`) — do not violate:

- No visible pre-BANG countdown.
- No arena picker in the menu (random PvP/Coop; PvE pins per stage).
- No pay-to-win if/when meta lands (see META doc principle 1).

---

## 3. Architecture ceilings (scale problems, not today's crashes)

### 3.1 God objects

- **`DuelManager`** (~600 lines) owns: Reaction match, Timing match, lane resolve, records banners, PvE lives / chapter advance, and `SceneManager.LoadScene` for Menu / Map / Story / rematch.
- **`MainMenuBootstrap`** owns all five tabs (STATS / GUNS / HOME / SETUP / SOUND) as one class.
- **`RunRound` and `RunTimingRound`** duplicate the intro walk-in + stance ritual.

When touching duel flow, extract a shared `PlayIntroAndStance(active, doIntro)` coroutine rather than copying again. Scene routing (`OnMenu`, `LoadMap`, `GoStory`, `ShowPveResult`) should eventually leave `DuelManager` (a tiny `DuelFlow` / static navigator is enough — not a framework).

### 3.2 Static bags instead of a session

Today:

- `MatchSettings` — mode / players / difficulty / duel type / forced arena
- `Campaign` — run state + **all chapter content** in one file
- `Story` — which story beat to show
- `GameSettings` — PlayerPrefs audio/haptics/weapon/tutorial
- `Records` — PlayerPrefs lifetime stats

Each PlayerPrefs writer calls `PlayerPrefs.Save()` itself. The META doc already names the fix: one `SaveData` + `Economy.Grant/Spend`. Until then, **keep using the existing mutators** (`Campaign.Save`, `Records.Report*`, `GameSettings` setters). Do not invent a second save path.

### 3.3 Content in C#

`Campaign.Chapters`, `Arenas.All`, `Weapons.All`, dialog lines — arrays in code. Fine for 13 stages. Not fine for localization, seasonal content, or designer-only balance. Do not migrate to ScriptableObject catalogs until a human actually needs to edit them without a rebuild.

### 3.4 Code-built UI

Every screen is `new GameObject` + uGUI `Text` (legacy font). Fast for MCP, painful for a shop / pass / reward popup. New meta UI should be **prefabs or a small widget kit** (`DialogBox` / `DuelHUD.ShowResult` / `ShowBanner` style), not another 800 lines on `MainMenuBootstrap`.

### 3.5 No asmdef, no tests

Everything is `Assembly-CSharp`. Lane resolve, false-start, Timing PvE "all humans must hit green", and timestamp comparison are pure logic and should become Edit Mode tests once extracted from the MonoBehaviour coroutine. Do not add Play Mode tests as the first step — extract the decide step first.

---

## 4. Bugs & fairness

### P0 — Human vs bot clocks are not the same

**Bot** records the *scheduled* instant, not the frame that noticed it:

```csharp
// BotDuelInput.Tick
_fireTime = _scheduledFire; // sub-frame exact
```

**Human** records the coroutine tick's `nowRealtime` (`Time.realtimeSinceStartupAsDouble` at `DuelManager` `yield return null`), ignoring Input System `Touch.startTime` / `Touch.time`.

On 60 fps that is up to ~16 ms against the bot; worse on a hitch or 30 fps. Hard bots react in 0.18–0.30 s — a frame is a real fraction of the window.

**Fix:** stamp the human with the touch event time (Enhanced Touch `startTime`/`time`, same clock as `realtimeSinceStartup`). Keep the bot on the same clock. Polling in `Tick` can stay; the *timestamp* must be the event's.

**Files:** `Input/HumanDuelInput.cs`, `Input/BotDuelInput.cs`, `Core/DuelManager.cs` (passes `now` into `Tick`).

### P0 — Same-frame two-human tap: Bottom always wins

`TickInputs` feeds **one** `now` to every duelist. `DecideLane` uses `FireTimeRealtime < best`, so equal times keep the first in the list. `BuildPvp` / `BuildCoop` put Bottom first → Player 1 wins every same-frame tie.

**Fix:** per-touch timestamps (see above). If still equal, treat as a lane draw (already a concept: `winner == null` → no survivor), not "list order".

**Files:** `Core/DuelManager.cs` (`DecideLane` / bang loop), `Core/DuelBootstrap.cs` (list order — do not "fix" by reordering; fix the compare).

### P0 — Loser can still play shoot FX on the deciding frame

Comment in `DuelManager` says the loser never shoots. Code runs `FireFxForNew(active)` **before** `DecideLane`. If both `HasFired` this frame, both get `PlayShoot` + gunshot, then the loser `PlayDeath`.

**Fix:** decide the lane first (or skip FX for anyone who is about to lose this frame). Instant feedback for the *winner* must stay; that is the feel contract.

**Files:** `Core/DuelManager.cs` (`FireFxForNew` vs `DecideLane` order).

### P1 — `Texture2D` / `Sprite` leaks

`new Texture2D` is a UnityEngine.Object. Destroying the GameObject that *displays* the sprite does **not** destroy the texture.

Allocated with no `Destroy` today:

- `CowboyArt.Frame` — 7 textures per look (idle×2, ready, shoot×2, death×2)
- `BackgroundBuilder.BuildGround` — 128×128
- `PropArt`, `PlaceholderArt` (tumbleweed every spawn), `WeaponArt.For` (new sprite on every carousel click)
- Dialog intro calls `CowboyArt.Build` **again** for portraits while `DuelistView.Setup` already built the same looks

Also: `ScriptableObject.CreateInstance<BotConfig>()` / `DuelConfig` each Duel load when using match settings — runtime SOs leak until domain unload.

**Fix:** cache by look/weapon/arena name; `Destroy` textures on scene teardown (`OnDestroy` of the view / background root). Cache `WeaponArt.For`. Destroy runtime SOs with the duel GO, or reuse one.

**Files:** `View/CowboyArt.cs`, `View/DuelistView.cs`, `View/BackgroundBuilder.cs`, `View/PropArt.cs`, `View/PlaceholderArt.cs`, `View/WeaponArt.cs`, `View/Tumbleweed.cs`, `UI/DialogBox.cs` + `DuelBootstrap` intro portraits, `Core/DuelBootstrap.cs` (SO instances).

### P1 — Campaign save edge cases

```csharp
// Campaign.LoadSavedRun
Lives = Mathf.Max(1, PlayerPrefs.GetInt(KLives, StartingLives));
```

If `Active == true` and `Lives == 0` (crash between `LoseLife` and `EndRun`), the player gets a free life. `LoadSavedRun` also writes `Chapter`/`Stage` from prefs even when `Active == false`. Map/Duel already guard with `if (!Campaign.Active) Campaign.StartRun()` — keep that; still don't resurrect 0 lives.

**Fix:** if `!Active`, don't hydrate run fields (or reset them). If `Active && Lives <= 0`, treat as ended run (`EndRun(false)`), not `Max(1)`.

**Files:** `Core/Campaign.cs`.

### P2 — PvE 2v2 identical opponents

`BuildPve` two-player branch assigns the same `oppLook` and the same `Title` to both top bots. They read as twins.

**Fix:** second look (tint / hat / accessory variant) and labels (`"{Title} 1"` / `" 2"`, or a `StageDef.PartnerLook`).

**Files:** `Core/DuelBootstrap.cs`, optionally `Core/Campaign.cs` `StageDef`.

### P2 — Stale / confusing bits (fix when touching the file)

- `MatchSettings` comment: "Coop/PvE wired later" — they are wired.
- Menu toggle **MUSIC** also mutes duel wind (`MusicPlayer` gates both layers on `GameSettings.MusicEnabled`). Either rename the toggle (MUSIC / AMBIENCE) or split a flag. Product call; don't silently change feel.
- Arena ground seed is `def.Name.GetHashCode()`. `string.GetHashCode()` is not guaranteed stable across editor vs IL2CPP / CoreCLR. PvE "this mission = this ground" should use an explicit `int Seed` on `ArenaDef`.
- `DuelistView.WalkIn` is started via `DuelManager.StartCoroutine(...)`, so it runs on the manager, not the view. `SetIdleOffscreen` → `StopAllCoroutines()` on the **view** does not stop WalkIn. Harmless today because rematch does `StopAllCoroutines` on the manager; still the wrong host if someone later starts WalkIn on the view.

---

## 5. Bottlenecks (BANG-window performance)

A duel is decided in ~200–400 ms. Work in Tension/Bang must be allocation-free and FPS-stable.

### 5.1 LINQ every frame in the hot loop

**Done 2026-09-09.** `RunRound` buckets with `BucketByLane` once, then Tension/Bang iterate `byLane[i]` with no LINQ. LINQ at round *end* (`survivors.Where`, Timing setup) is still fine.

**Files:** `Core/DuelManager.cs`.

### 5.2 Input is Update-polled, not event-timestamped

`yield return null` runs in Update. Input System has already sampled the frame, but you throw away the event time (see P0). Optional later: `InputSystem.onBeforeUpdate` / a generated Input Action with `callback.time`. Not required if `Touch.time` is used inside `Tick`.

### 5.3 Haptics JNI on the shot frame

`Haptics.Light()` from `FireFxForNew` / `PlayShootFx` constructs `VibrationEffect.createOneShot` via `AndroidJavaObject` every call. JNI + GC on the same frame as the gunshot.

**Fix:** reuse one cached `VibrationEffect` per (duration, amplitude), or defer Heavy/Light to the frame *after* decide. `Handheld.Vibrate()` fallback is ~500 ms — do not use it on a gunshot if the vibrator object is null; skip instead.

**Files:** `Core/Haptics.cs`, call sites in `DuelManager`.

### 5.4 Frame rate is not locked

`AppInit.Apply()` only sets `Application.runInBackground = true`. `ProjectSettings/QualitySettings.asset`: `m_CurrentQuality: 0` (**Very Low**), that level has `vSyncCount: 0`. No `Application.targetFrameRate`.

Result: uncapped / jittery FPS on some Android devices, or 30 fps on others. For this game **one frame is gameplay**.

**Policy (2026-09-09):** `AppInit.Apply()` sets `QualitySettings.vSyncCount = 0` and `Application.targetFrameRate = 60`. Quality level stays **Very Low** (fillrate-cheap 2D). Do not enable vSync on top of the cap. 120 later if we detect a high-refresh display.

**Files:** `Core/AppInit.cs`, optionally QualitySettings.

### 5.5 First-load audio / art spikes (not in BANG)

- `AudioBank` `Resources.LoadAll` is cached after first use. First gunshot of a new weapon still scans `Audio/Shots`. Warm in the menu (`AudioBank.GunshotFor(Weapons.Selected, ...)`) if load hitch shows up.
- `BackgroundBuilder` Perlin 128×128 on the main thread at duel start — acceptable. Don't move it into the tension phase.

### 5.6 `PlayerPrefs.Save()`

Called on every `GameSettings` setter and every `Records` / `Campaign` mutate. Fine at result/menu time; **never** call it inside Tension/Bang (today you don't — keep it that way). `Records.ReportReaction` runs in the finalize step, after the window — OK.

---

## 6. Priority backlog

Check boxes as you complete work. Prefer one item (or a tight pair) per change.

### P0 — reaction fairness (do these first)

- [x] **Human fire timestamp from Input System touch/mouse event time**, not Tick's `now`. Same clock as bots. (`Input/HumanDuelInput.cs` stamps `Touch.startTime` / `ButtonControl.lastUpdateTime`; Tick `now` is fallback only. `IDuelInput` comment.)
- [x] **Same-frame equal times → lane draw**, not list-order win. (`Core/DuelManager.cs` `TryPickLaneWinner` — equal best times → `DecideLane(..., null)`.)
- [x] **Decide lane before loser shoot FX** (winner still flashes instantly). (`DuelManager` bang loop: Tick → `TryPickLaneWinner`/`DecideLane` → `FireFxForNew`. Losers get `ShotFx` in `DecideLane` so they never `PlayShoot`.)
- [x] **Lock 60 FPS** in `AppInit.Apply()`; document quality/vSync policy. (`AppInit`: `vSyncCount = 0`, `targetFrameRate = 60`. Quality level stays Very Low.)
- [x] **Remove LINQ from Tension/Bang loops**; pre-bucket by lane. (`DuelManager.BucketByLane` once per round; `bool[]` resolved; `AllHaveFired`.)

### P1 — memory & hitch

- [ ] **Cache + Destroy procedural textures** (cowboy frames, weapons, ground, props, tumbleweed). Don't rebuild portraits in the dialog if the duelist already has frames.
- [ ] **Cache `WeaponArt.For(index)`**; don't allocate on every carousel click.
- [ ] **Don't leak runtime ScriptableObjects** (`BotConfig` / `DuelConfig` `CreateInstance`).
- [ ] **Haptics:** cache JNI effects; never `Handheld.Vibrate()` on a 18 ms gunshot tick.

### P1 — campaign save

- [ ] **`LoadSavedRun`:** don't `Max(1)` a zero-life active run; don't hydrate chapter/stage when `!Active`.

### P2 — content / polish (when touching those files)

- [ ] Distinct look + labels for PvE 2v2 bots.
- [ ] Explicit `ArenaDef.Seed` instead of `Name.GetHashCode()`.
- [ ] Stale `MatchSettings` comment; MUSIC vs wind toggle product decision.
- [ ] Host `WalkIn` on `DuelistView` (or stop it from the same MB that started it).

### P3 — seams before meta (do not start META pillars without these)

- [ ] Shared intro/stance coroutine used by Reaction and Timing.
- [ ] Scene routing out of `DuelManager` (tiny navigator).
- [ ] Single `SaveData` blob (settings + records + campaign) with one `Save()`; keep PlayerPrefs keys as the backend at first (META §1).
- [ ] Extract `DecideLane` / timing contest to a testable static/pure class; add Edit Mode tests.
- [ ] New UI as widgets/prefabs, not more tabs bolted onto `MainMenuBootstrap`.

Then follow [META_AND_PROGRESSION.md](META_AND_PROGRESSION.md) (economy → reputation/pass → cosmetics). That doc is still a **proposal**; this one is the engineering prerequisite.

---

## 7. Suggested split of work (for agents / MRs)

Keep MRs small. A reasonable sequence:

1. **`fix: reaction timestamps + same-frame draw`** — HumanDuelInput + DecideLane compare. Highest skill-feel impact. **Done 2026-09-09.**
2. **`fix: decide-before-fx + no LINQ in bang loop`** — same file, separate commit if the diff is large. **Done 2026-09-09.**
3. **`fix: lock 60 fps in AppInit`** — one-liner + comment. **Done 2026-09-09.**
4. **`fix: cache/destroy procedural sprites`** — art helpers; behavior unchanged.
5. **`fix: campaign LoadSavedRun zero-life / inactive hydrate`**
6. **`refactor: shared intro/stance`** — only after P0 is green.
7. Meta foundations from the other doc.

Do not combine (1) with meta UI. Do not "while I'm here" rewrite `DuelManager` into a state-pattern class hierarchy.

---

## 8. File map (where to look)

| Concern | Files |
|---|---|
| Round loop, decide, PvE result routing | `Core/DuelManager.cs` |
| Scene assembly, zones, looks | `Core/DuelBootstrap.cs` |
| Per-cowboy runtime | `Core/Duelist.cs` |
| Campaign content + save | `Core/Campaign.cs` |
| Menu → duel carrier | `Core/MatchSettings.cs` |
| Prefs: audio / weapon / tutorial | `Core/GameSettings.cs` |
| Prefs: records | `Core/Records.cs` |
| FPS / listener | `Core/AppInit.cs` |
| Human / bot fire | `Input/HumanDuelInput.cs`, `Input/BotDuelInput.cs` |
| Procedural sprites | `View/CowboyArt.cs`, `WeaponArt.cs`, `BackgroundBuilder.cs`, `PropArt.cs`, `PlaceholderArt.cs` |
| Poses | `View/DuelistView.cs`, `FrameAnimator.cs` |
| HUD / timing bars | `UI/DuelHUD.cs`, `View/TimingBar.cs` |
| Menu | `UI/MainMenuBootstrap.cs` |
| Audio | `Audio/AudioBank.cs`, `DuelAudio.cs`, `MusicPlayer.cs`, `Sfx.cs`, `ProcAudio.cs` |
| Haptics | `Core/Haptics.cs` |
| Quality / vSync | `ProjectSettings/QualitySettings.asset` |

---

## 9. Out of scope / do not do

- Rewrite to ECS, Zenject, UniTask-everywhere, or asmdef spaghetti for 42 files.
- Visible pre-BANG countdown or a menu arena picker.
- Pay-to-win stats on weapons (META principle 1). Weapons are sound + icon until a dedicated stats pass.
- APK builds from the agent environment.
- Git commits unless the user asks.
- Expanding `MainMenuBootstrap` with pass/shop/crates panels — new screens, not more tabs in the same class.

---

## 10. Review notes (2026-09-09)

Pass covered Core, Input, View, UI, Audio, Config. No automated tests existed to confirm runtime. Highest-confidence issues are the ones with a cited code path (timestamps, list-order ties, FX order, `new Texture2D`, LINQ in the bang `while`, `LoadSavedRun` `Max(1)`, Very Low / no `targetFrameRate`). Device-only items (haptics hitch, touch `startTime` vs editor mouse) need a phone to fully prove.
