# High Noon — project guide

2D pixel-art Wild West **reaction-duel** game. Ship order: **Android → iOS → Web**. Unity **6000.3.21f1**, URP **2D Renderer**, new **Input System**, portrait orientation.

Two players hold opposite ends of one phone; each taps their half. On `BANG!` the first valid tap wins. Tapping *before* BANG is a false start (loss). Modes: **PvP** (built), **Coop 2v2** (built), **PvE campaign** (built — staged run with lives, solo 1v1 or 2-player 2v2; visual mission map TODO). **Android build/testing is the user's responsibility** — don't attempt APK builds here.

## Run it
Open `Assets/_Project/Scenes/MainMenu.unity` → Play. Pick players / difficulty → PLAY.
- Editor test controls: Player 1 = tap lower half or **S**; Player 2 = tap upper half or **W** (only when "2 Players").
- Open `Duel.unity` directly to test a duel in isolation — uncheck **Use Match Settings** on the `Bootstrap` object to use its manual flags instead of the menu selection.

## Scenes (Build Settings order)
- `[0] Assets/_Project/Scenes/MainMenu.unity` — `MenuBootstrap` → `MainMenuBootstrap` (code-built UI).
- `[1] Assets/_Project/Scenes/Duel.unity` — `Bootstrap` → `DuelBootstrap` + a **Global Light2D** (required by the 2D Renderer or sprites render black).

## Architecture
Code-first: each scene has ONE bootstrap component that builds the camera, background, UI, audio and gameplay objects at runtime — minimal scene wiring, easy to drive via MCP. All gameplay is in namespace `HighNoon`, default `Assembly-CSharp` (no asmdef yet).

`Assets/_Project/Scripts/`
- **Core/** — `DuelBootstrap` (assembles the duel scene), `DuelManager` (round state machine), `Duelist` (per-cowboy runtime state), `DuelEnums`, `MatchSettings` (static carrier from menu → duel).
- **Input/** — `IDuelInput`; `HumanDuelInput` (first touch/click in a screen zone, keyboard fallback, multi-touch aware); `BotDuelInput` (fires at BANG + random reaction).
- **Config/** — `DuelConfig`, `BotConfig` (ScriptableObjects; runtime instances created if none assigned).
- **View/** — `DuelistView` (poses via frame animation), `FrameAnimator`, `CowboyArt` (procedural pixel cowboy frames), `PlaceholderArt` (background/tumbleweed sprites), `CameraShake`, `Tumbleweed`.
- **UI/** — `MainMenuBootstrap`, `DuelHUD` (BANG!, reaction popups, full-screen flash, result panel).
- **Audio/** — `DuelAudio` (procedural placeholder SFX: tension loop, noon bell, gunshot, thud).

### Duel state machine (`DuelManager`)
`Intro (walk in) → Stance → Tension (hidden random countdown, players never see it) → Bang → Resolved → Result`.
Fires are compared by timestamp (`Time.realtimeSinceStartupAsDouble`); earliest valid tap wins. A tap before BANG = false start (that duelist loses their lane).

**Lane-based / rounds:** duelists are paired by `Duelist.Lane` (Bottom vs Top). 1v1 = one lane. 2v2 (Coop) = lanes 0 & 1 resolved in parallel; if each side wins one lane, the two survivors get `Lane = 0` and fight a tie-break round until one side owns the match. Coop = 2 human players share the bottom (quadrant tap zones, editor keys A/D), two bots on top.

### Arenas / locations
Random per duel (`Arenas.Pick`, avoids immediate repeat). Six arenas: Prairie, Dusty Town, Red Canyon, Boot Hill, Green Valley, Salt Flats. Each built procedurally (noise ground + props). PvE will pin an arena per mission point via `MatchSettings.ForcedArena` — there is deliberately **no arena picker in the menu**.

### PvE campaign (`Campaign`)
Static run state (Core/`Campaign.cs`): `Stages[]` of `StageDef {Title, Arena, Difficulty}`, `Lives` (start 3, shared team lives), `Stage`. Menu PLAY in PvE mode calls `Campaign.StartRun()`; `DuelBootstrap` (PvE branch) pins the stage arena/difficulty via `Campaign.ApplyToMatch()` and builds the duel from the Players toggle — **1 player = solo 1v1** ("YOU" vs the stage bot), **2 players = 2v2** (P1+P2 vs two stage bots, reusing the lane/tiebreaker logic). A top status line (`DuelHUD.SetPveStatus`) shows stage / opponent / lives / SOLO|CO-OP. On result, `DuelManager.ShowPveResult` (player side = Bottom): **win** → advance stage (last → VICTORY) with **NEXT**; **loss** → `Lives--` (0 → DEFEAT) with **RETRY**. Buttons reload the `Duel` scene. `DuelHUD.ShowResult(message, label1, act1, label2, act2)` takes configurable buttons. TODO: a visual mission map + dialogs.

### Bots
Reaction is an **interval**, never a fixed time: `BotConfig.reactionMin..reactionMax`, sampled with `RollReaction()` at BANG. Difficulty (`MatchSettings.ApplyDifficulty`): Easy 0.45–0.75s, Normal 0.30–0.48s, Hard 0.18–0.30s. Bots fill empty top slots (single-player PvP now; Coop/PvE later).

## Conventions & gotchas
- **2D Renderer** → a **Global Light2D** must exist in any scene with SpriteRenderers, or they render black.
- **`Application.runInBackground = true`** (set in both bootstraps + PlayerSettings) — without it Play mode freezes when the Unity window is unfocused (e.g. MCP-driven testing).
- **Top duelist rotated 180°** (not Y-flip) so each player at opposite phone ends sees their cowboy upright.
- **Team color** is baked into the cowboy sprite pixels; `SpriteRenderer.color` stays white.
- **uGUI Text**: always set `txt.text` explicitly; font = `Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf")`.
- Portrait, reference resolution 1080×1920. Duelist scale 1.8, cowboy sprite 24×32 @ PPU 24.
- To eyeball procedural art: blit frames to a PNG contact sheet via `execute_code(safety_checks=false)` and open it.

## Roadmap
PvP core ✅ · main menu ✅ · pixel art + frame animation ✅ · juice (shake/flash/hit-stop/tumbleweed) ✅ · 6 arenas ✅ · Coop 2v2 ✅ · PvE core loop ✅ → **PvE mission map** (visual node map; each node pins its arena) · 2-player PvE (2v2) · dialogs · balance/audio polish. Deliberately **no visible pre-BANG countdown** (tension is audio-only) and **no arena picker** (random for PvP/Coop; PvE decides per stage).
