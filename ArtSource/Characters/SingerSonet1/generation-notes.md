# SingerSonet1 generation

Status: **not generated yet**. This session has no image-generation tool available, so
these are the four ready-to-run prompts only — Idle, Walk, Shoot and Death are all
still missing. Run each prompt through an image-gen tool, then export/crop per
`ArtSource/Generation/sprite_sheet_spec.md` (nearest-neighbor, uniform scale, centered
per cell, bottom aligned to y=131, binary alpha threshold 128) and save as:

```text
ArtSource/Characters/SingerSonet1/
    SingerSonet1_Idle.png
    SingerSonet1_Walk.png
    SingerSonet1_Shoot.png
    SingerSonet1_Death.png
```

Reference cowboy image (style/pose/scale reference only, replace all appearance):
`G:/Projects/HighNoon/ArtSource/cowboy_death_sheet.png`

Shared character description (keep identical across all four prompts):

> Western saloon stage singer, adult, confident stance, dark hair in a simple upswept
> style with a small feather trim, high-necked burgundy performance dress with long
> sleeves and a modest skirt hem, dark ankle boots, a thin choker, small earrings.
> Fully clothed, professional stage-performer costume, no exposed midriff or plunging
> neckline. Holds a short double-barrel sawed-off shotgun with a wood grip. Consistent
> realistic elongated body proportions, readable 1-2px dark outlines, detailed warm
> shaded pixel clusters, crisp stair-step pixels, no antialiasing.

Shared technical block (append to every prompt):

> REQUIRED OUTPUT: one PNG RGBA precisely 1152x144 pixels, 8 equal cells 144x144 in a
> single horizontal row. Transparent background, alpha only 0 or 255, no background, no
> ground shadow, text, dividers, labels, particles or blood. Every frame faces RIGHT.
> Full sprite contained in its cell, center local x72, bottom occupied pixel local
> y131 leaving 12 transparent rows below. Standing height 120px, top y12 (Death sheet
> only: height shrinks per the fall sequence, bottom row stays y131). Keep body scale
> throughout, no arbitrary shrinking or stretching. Exactly 8 distinct frames left to
> right.

## Idle prompt

Use case: stylized-concept. Create production pixel art animation sprites for a
side-view Western game. [shared character description above]. [shared technical
block above].
Animation IDLE: 8 subtle breathing frames, feet planted, shotgun held low pointing
down-right. Frame1 relaxed neutral; 2 gentle inhale; 3 chest rises; 4 peak inhale;
5 relaxed exhale; 6 chest lowers; 7 low breath; 8 approaching first pose for seamless
loop. Deliver as SingerSonet1_Idle.png.

## Walk prompt

Use case: stylized-concept. Create production pixel art animation sprites for a
side-view Western game. [shared character description above]. [shared technical
block above].
Animation WALK: full step cycle in place for rightward movement, shotgun held low
in one hand, natural arm swing and skirt sway. Frame1 contact pose front leg forward;
2 passing/recoil; 3 contact opposite leg; 4 passing; 5 back to frame1-like contact
(other leg leading, matching a full stride); 6-8 continue the cycle smoothly so frame8
flows back into frame1 with no pop. Deliver as SingerSonet1_Walk.png.

## Shoot prompt

Use case: stylized-concept. Create production pixel art animation sprites for a
side-view Western game. [shared character description above]. [shared technical
block above].
Animation SHOOT: weapon aimed RIGHT throughout the firing beat. Frame1 ready/aim
raised; 2 steady aim; 3 trigger squeeze; 4 muzzle flash + recoil, shotgun kicks back
and up; 5 peak recoil, body leans back slightly; 6 recovering, weapon lowering back
toward aim; 7 near return to aim pose; 8 settled back to frame1 aim pose. Deliver as
SingerSonet1_Shoot.png.

## Death prompt

Use case: stylized-concept. Create production pixel art animation sprites for a
side-view Western game. [shared character description above]. [shared technical
block above].
Animation DEATH (single playthrough, hold last frame, no loop): overall motion is
lean back → sink → fall forward → lying pose. Frame1 upright standing, weapon lowered;
Frame2 hit reaction, head/torso lean back, knees bend; Frame3 sinking, torso tips
forward, head drops; Frame4 falling to one knee, arm and weapon approach the ground;
Frame5 toppling, bracing on one arm, torso near horizontal; Frame6 torso continues
lowering, legs extending rightward; Frame7 lying down, head on the left, legs on the
right, low elongated silhouette; Frame8 final resting pose, same footprint as frame7
but a distinct drawing. Follow the scaled-up bounding boxes from
`ArtSource/Generation/sprite_sheet_spec.md` section 2 as a guide (frame1 ~48x120,
frame7/8 ~99x33, bottom always at local y131). No blood, no separate cast shadow.
Deliver as SingerSonet1_Death.png.
