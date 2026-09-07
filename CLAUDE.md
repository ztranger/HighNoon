# High Noon — project guide

2D pixel-art Wild West **reaction-duel** game. Ship order: **Android → iOS → Web**. Unity **6000.3.21f1**, URP **2D Renderer**, new **Input System**, portrait orientation.

Two players hold opposite ends of one phone; each taps their half. On `BANG!` the first valid tap wins. Tapping *before* BANG is a false start (loss). Modes: **PvP** (built), **Coop 2v2** and **PvE** (planned).

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
Fires are compared by timestamp (`Time.realtimeSinceStartupAsDouble`); earliest valid tap wins. A tap during Tension = false start (that duelist loses).

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
PvP core ✅ · main menu ✅ · pixel art + frame animation ✅ · **juice (in progress)** → local 2-player on device → Coop 2v2 → PvE campaign. Deliberately **no visible pre-BANG countdown** (tension is audio-only).
