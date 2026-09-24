# CabaretSinger generation

Status: All four sheets generated using built-in image_gen: Idle, Walk, Shoot, Death. Technical validation passed for all 32 frames: 1152x144 RGBA sheets, eight 144x144 cells, alpha only 0/255, occupied bottom y=131, no cell-edge clipping. See validation.json. Death final silhouettes are 25px high versus the reference target of approximately 33px; poses were kept at a uniform scale rather than individually stretched. A correction attempt was rejected because adjacent poses overlapped. Animation timing still needs in-engine review.

Idle exported with nearest-neighbor sampling at uniform scale, centered per cell, bottom aligned to y=131; binary alpha threshold 128. Output: 1152 x 144 RGBA, eight 144 x 144 cells. Original generation was 2172 x 724.

## Idle prompt

Use case: stylized-concept. Create production pixel art animation sprites for a side-view Western game. Adult alluring cabaret singer, visibly age 28, wavy dark auburn updo with a short burgundy feather, burgundy corset stage dress with short ruffled skirt, black stockings, heeled dark ankle boots, long dark gloves, gold small earrings; holds a short double-barrel sawed-off shotgun with wood grip. Fully clothed theatrical sensuality. Consistent realistic elongated body proportions, readable 1-2px dark outlines, detailed warm shaded pixel clusters, crisp stair-step pixels, no antialiasing. Reference cowboy image is STYLE and pose reference only, replace all cowboy appearance with this woman.
REQUIRED OUTPUT: one PNG RGBA precisely 1152x144 pixels, 8 equal cells 144x144 in a single horizontal row. Transparent background, alpha only 0 or 255, no background, no ground shadow, text, dividers, labels, particles or blood. Every frame faces RIGHT. Full sprite contained in its cell, center local x72, bottom occupied pixel local y131 leaving 12 transparent rows below. Standing height 120px, top y12. Keep body scale throughout, no arbitrary shrinking or stretching. Exactly 8 distinct frames left to right.
Animation IDLE: 8 subtle breathing frames, feet planted, shotgun held low pointing down-right. Frame1 relaxed neutral; 2 gentle inhale; 3 chest rises; 4 peak inhale; 5 relaxed exhale; 6 chest lowers; 7 low breath; 8 approaching first pose for seamless loop. Deliver as CabaretSinger_Idle.png.


## Walk prompt

Create CabaretSinger_Walk.png, a production pixel art walk animation sprite sheet. Reference image is the existing IDLE sheet: preserve EXACT adult woman's identity, proportions, burgundy feather and auburn updo, burgundy corset short ruffled stage dress, black stockings, dark heeled ankle boots, long dark gloves, golden earrings, short double-barrel sawed-off shotgun. Adult alluring cabaret singer age 28. Same detailed warm pixel art, crisp dark 1-2px contours, no blur.
Output one transparent PNG RGBA exactly 1152x144, eight 144x144 cells in ONE horizontal row, no gaps. Exactly 8 poses left to right. Bottom occupied row local y131 in ALL frames, standing height about120px, centered local x72. Binary alpha 0/255. Full bodies, no cropping, no text, labels, floor, shadows, blood or particles.
All frames face RIGHT. WALK IN PLACE full seamless cycle: 1 right foot forward contact and left foot back; 2 down weight transfer; 3 left leg passes planted right leg; 4 up left leg swings forward; 5 left foot forward contact right foot back; 6 down opposite weight transfer; 7 right leg passes planted left leg; 8 up right leg swings forward towards first pose. Clear alternating legs, natural knee bending and planted feet. Same body scale every frame, natural slight vertical bob. Hold shotgun low pointing down-right in both hands throughout. Skirt/feather subtle follow-through. Do not repeat idle poses.

## Shoot prompt

Generate CabaretSinger_Shoot.png. Attached idle sheet is identity/style reference; preserve exactly this adult cabaret singer with auburn updo, burgundy feather, burgundy corset short ruffled skirt, black stockings, heeled ankle boots, long gloves and short double-barrel sawed-off shotgun. Detailed warm pixel art, elongated realistic proportions, crisp dark outlines, no antialiasing. Single transparent RGBA PNG exactly1152x144 pixels, 8 cells of144x144 in one row, all characters FACE RIGHT. Bottom occupied local y131 all frames, height about120, center x72. Binary alpha. No text/dividers/background/shadow/blood/particles. Same body scale across frames, full body contained each cell.
8 SHOOT poses left to right: 1 relaxed shotgun down-right identical idle start; 2 raise shotgun to chest level; 3 aim horizontally RIGHT with both hands; 4 discharge pointing RIGHT, arms extended, weapon action, no external muzzle flash particles; 5 strong recoil gun tilted up-right shoulders back and knees slightly bent; 6 recover recoil aiming right; 7 lower gun; 8 original relaxed shotgun down-right pose. Clear progression, consistent short two barrels wood grip gun and same character every frame.

## Death prompt

Generate CabaretSinger_Death.png. Image1 is woman's identity reference, image2 cowboy is EXACT death pose sequence reference. Replace cowboy with same adult cabaret singer: auburn updo with burgundy feather, burgundy corset short ruffled skirt, black stockings, dark heeled ankle boots, long gloves, gold earrings, short double-barrel sawed-off shotgun. Preserve woman's identity/equipment in EVERY pose. Detailed crisp warm pixel art matching reference, realistic elongated proportions, no soft edges.
One transparent RGBA PNG 1152x144 pixels, 8 cells144x144 in ONE horizontal row. Exactly8 frames. Local ground occupied bottom y131 EVERY frame, silhouette centered x72. Binary alpha. No ground/shadow/text/labels/particles/blood. Full body fits cell. Fixed anatomical scale: falling means bent pose, NOT shrinking body. Same right-facing side visible throughout, never mirror.
CRITICAL 8 DEATH stages left-to-right:
1 standing upright facing RIGHT gun low, height120px, local top12.
2 head and torso recoil backwards to LEFT knees bend, height109 top23.
3 sinks and folds torso forward, head lowers, height87 top45.
4 falls onto one knee hand and gun near ground, height73 top59.
5 nearly horizontal torso supporting herself on one hand, head LEFT of hips, height51 top81.
6 torso lowers further and legs extend to RIGHT, head at LEFT, height43 top89.
7 fully prone flat on ground, head at LEFT and feet at RIGHT, face toward ground/right retaining same side, height33 top99, width99, local x23..121.
8 held final prone pose with tiny settling difference in skirt and hand, same bounds as7 but not identical.
See cowboy reference for exact collapse choreography, especially head LEFT and feet RIGHT in last frames. Do not make crawling loop or left-facing upright poses. Shotgun remains visibly short, close to hand on ground final frames.
