# SingerOpus481 generation

Status: **complete** — all four sheets rendered.

```text
ArtSource/Characters/SingerOpus481/
    SingerOpus481_Idle.png
    SingerOpus481_Walk.png
    SingerOpus481_Shoot.png
    SingerOpus481_Death.png
```

## Method

This session had **no AI image-generation tool** available (the earlier singer sheets used a
"built-in image_gen" that is not present here, and was hitting usage limits anyway). So instead
of a diffusion pass, the sprites are **procedurally rendered pixel art** — a small rigged
character drawn with Python + Pillow. The generator is checked in beside the sheets as
`generate.py` (run with `python generate.py`; writes the four PNGs here and 4× previews to the
scratchpad). Everything is reproducible and tweakable from that one file.

The look is deliberately cleaner / flatter than the diffusion reference sheets (`Lady_1`,
`CabaretSinger`), but it honours the same technical contract and character brief. If a
diffusion tool becomes available later, re-run the same four prompts (see below) and re-export
per the spec — the folder layout and naming already match.

## Character (identical across all four sheets)

Cabaret / saloon stage singer, adult, sultry confident posture. Dark auburn hair in an updo
with a short **burgundy feather**; **burgundy laced corset** dress with cream lacing and a hint
of décolletage; **short ruffled skirt**; **long dark opera gloves**; **black stockings**;
**dark-brown heeled ankle boots**; small gold earring + thin choker. Carries a short
**double-barrel sawed-off shotgun** with a wood grip. Always **faces RIGHT**; weapon points
right when shooting.

## Spec compliance (`ArtSource/Generation/sprite_sheet_spec.md`, ×1.5 scale)

- Each sheet **1152 × 144 px** RGBA, **8 cells of 144 × 144** in one row, 0 gutter/margin.
- Background fully transparent; **alpha is binary (only 0 / 255)** — no soft halo.
- **Bottom occupied row = y131** in every one of the 32 frames (12 px clearance below).
- Silhouette centred on **x ≈ 72**; standing height ≈ 120 px (top ≈ y10–12).
- Right-facing in all frames; not mirrored (the death still uses the right-facing design even
  though the head ends on the left in the lying pose).
- Idle and Walk loop seamlessly; Shoot returns to the ready pose; Death is a single play with a
  held final frame.
- Death follows the reference stages — upright → lean-back hit reaction → sink/hunch → drop to
  a knee → pitch forward → prone with **head left, boots right** — and shrinks in height while
  widening (measured frame heights ≈ 123 · 109 · 95 · 74 · 40 · 34 · 28 · 28, tracking the
  spec's guide table).
- Hard stair-step pixels, 1–2 px dark warm outline, warm muted burgundy/brown palette. Display
  with Nearest / Point and integer scaling.

## Prompts (if regenerating with a real image-gen tool)

Shared character + technical blocks: use the description above plus the spec's §1/§5 technical
requirements (1152×144, 8×144 cells, binary alpha, right-facing, bottom row y131, standing
~120 px). Per animation:

- **Idle** — 8 subtle breathing frames, feet planted, shotgun held low pointing down-right;
  frame 8 approaches frame 1 for a seamless loop.
- **Walk** — full rightward step cycle in place, natural arm swing + skirt sway, shotgun held
  low in one hand; frame 8 flows back into frame 1.
- **Shoot** — weapon aimed RIGHT: ready → aim → fire (muzzle flash + up/back recoil) → recover
  → settle back to the aim/ready pose.
- **Death** — single fall, hold last frame: upright → lean-back reaction → sink → to one knee →
  pitch forward → lie prone, head left / boots right, low elongated silhouette.
