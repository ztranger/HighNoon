# High Noon — locations and faces for final art

**Status:** ART SPEC. The Wire is in the game now as procedural sky, ground, and pixel cowboys (no `Background`, no `CharacterId`). Everything below is what those stand-ins should become. The other five roads are not built yet; their specs are here so the pictures can be drawn before the code.
**Created:** 2026-09-30.
**Language:** English.

A finished location is a painted backdrop at `Resources/Art/Locations/…`, wired by setting `ArenaDef.Background`. Until that file exists, leave `Background` empty and the procedural palette stands in. A finished face is a sheet id on `CowboyLook.CharacterId`. Until then the color, hat, chest, and beard fields are the only portrait.

Side view, landscape, two figures across a street. Horizon low. No readable text in the painting. Props sit at the edges so they do not cover the duelists at world x ≈ ±5.

Stories: [CAMPAIGN_IDEAS.md](CAMPAIGN_IDEAS.md).

---

## The Wire — in the game, procedural

Telegraph line from a railhead to a night station. Brass, creosote, pale grass, then dark. The wire itself should be visible in every painting: one sagging line, or a pile of cut coils. Never a modern pole.

### Locations

**Railhead** (`Railhead`, town, morning). End of track. A wood depot and a water tank on the horizon, false-front stores smaller behind them. Warm dust, blue sky, low gold sun. Foreground: a hand cart, a coil of bright wire, a practice key on a barrel. The poles start at the edge of town and march out of frame. Mood: the morning the hum died.

**Dry Creek** (`Dry Creek`, town, dusk). A one-room station beside a dry creek bed. Sky purple-brown, one orange lamp in the window, the sun already down. Two chairs and two brass keys on a desk should read even at distance. Foreground: the creek stones, a dark lantern. Mood: the night shift that stayed.

**Pole 12** (`Pole 12`, open prairie, noon). Flat grass, a hard blue sky, a row of thin poles shrinking to the horizon. Pole twelve is the nearest, and the wire on it is cut, the loose end curled in the dirt. No town. A few rocks, no cactus forest. Mood: the first place the line is on the ground.

**Alkali Tank** (`Alkali Tank`, open and bleached, harsh noon). White crust instead of grass, a round water tank on stilts, sky washed almost white. The tank is empty. A saddled horse that is not the rider's. Mood: a man waiting in a place with nothing to sell except what he stole.

**The Cut** (`The Cut`, broken desert, late day). A red gash where the grade was dug. Buttes. Coils of wire like snakes. Four dark coats would stand out against the red. Harsh sun, short shadows. Mood: the splice crew's hole.

**Mile 70** (`Mile 70`, prairie, sunset). Long orange horizon, hills almost black. One fencepost with a small receiver box nailed to it and a wire down to the ground. No building. The sun is a flat red disc. Mood: someone has been listening from nowhere.

**Mile 90 Yard** (`Mile 90 Yard`, town edge, night). The last two poles still carrying wire, in front of a dark station. Moon, not sun. Blue-black sky, iron roofs. Foreground: insulators, a ladder. Mood: the only singing wire left.

**Mile 90 Shed** (`Mile 90 Shed`, yard behind the station, night). Closer and greener than the yard. A shed door open, a ground rod, a green glass lamp (the procedural sun is that lamp). Tools, not graves. Mood: the spark lives here.

**Mile 90** (`Mile 90`, the station platform, full night). One building, one brass lamp over the door, the platform empty. Sky almost black, the lamp the only warm pixel. No crowd. The key inside is visible through the window and it is still. Mood: he turned the line off himself.

### Faces

Procedural stand-ins use hat, chest, and beard only. Final sheets should match these notes. Extras in a line are the same job, slightly different: shorter, older, a different hat. They are not new names.

**The Tapper.** Boy, about sixteen, railhead. Clean face, no beard. Bowler too big for him. Pale clerk shirt, sleeves rolled, brass key-dust on the cuff. No gunbelt until the fight; then a single revolver he is not proud of. Eager, not cruel. He thinks this is a repair.

**The Night Desk.** Two night operators. Leader: older, dark skin, bowler, green-black vest, thin mustache, lamp-orange pocket square. The second is younger, same desk, shirtsleeves, no hat, spectacles if the sheet can hold them. Each sits a key. They are tired, not bandits.

**The First Cut.** Three wire cutters at pole twelve. Leader: wide hat, bandana over a beard, sunburned, brown shirt, cutters in a fist, copper coil over one shoulder. The other two are leaner, one bareheaded, one in a cap. Work clothes, not uniforms. They started at the easy end and they know it.

**The Relief.** The rider who sold the coils. Faded yellow shirt, cowhide vest, mustache, sun-cracked hat. A grin. Saddlebags too heavy for a man who "lost" the wire. Smug, alone, alkali dust on the boots.

**The Splicers.** Four men in black-green rubber coats that read as ponchos in the stand-in. Wide hats, small mustaches or none, brass buttons, steel plates hinted under the coat at the chest. Faces oily, not monstrous. They are a crew. The leader is the broadest. Reserve 3 is those plates.

**The Listener.** A woman, about forty, alone at mile 70. No beard in the stand-in; the final sheet must read as a woman. Faded rose-gray shirt, soft neckerchief, plain hat, hair pinned. A small receiver in her hand, not a rifle, until the fight. Calm. She has already heard how this ends.

**The Last Poles.** Two night guards under the last live poles. Leader: iron-gray coat, wide hat, full beard, pale eyes, rifle down until the window. The second is thinner, younger, same coat. Reserve 3, plates under the wool. They are protecting a sound, not a safe.

**The Ground.** Two men on the ground wire. Leader: no beard, cowhand hat, a brass badge that means nothing outside this station, green-lamp light on the face. The partner is bulkier, gloves on, holding the rod. Strike 2 is the spark: the final art can show a blue-white jump at the rod, not a second gun.

**The Chief.** The man who silenced the line. Sixties, gray in the beard, bowler, black vest, charcoal shirt, brass watch chain. Older skin, lamp-warm. Not a black-clad devil and not a judge. He looks like the person who owns the key. No bar in the scene. He draws on the click.

---

## The Flood — in the game, procedural

Lumen the morning the levee failed. The nine arenas below are procedural stand-ins (seeds 1301–1309, no painted `Background`). Water should read as climbing: market still warm, streets gray-green, the roof dry and bright. Mark a waterline on the final paintings so the set is one flood.

### Locations

**Lower Market.** Morning, ankle-deep, sacks stacked into a wall, awnings still up. Warm brick, nervous blue sky.

**Cork Lane.** A skiff jammed across a narrow lane. Water to the hubs. Two oars.

**Tanner's.** Interior stair, three men coming up, tannery vats below starting to float. Wet wood, brown light.

**Chapel Street.** Waist-deep, the church door a dark rectangle, bell moving. Gray-green water, white trim.

**Bank Steps.** Stone steps still dry at the top. Four men. Paper money in a belt. Hard noon reflected off the water below.

**Chapel Roof.** Tiles, the town seen as canals, a sexton with a shovel. Late light.

**Flood Street.** Fast water between buildings. Two men abreast. Spray. The street is gone.

**Courthouse Steps.** The last dry stone. Two boats tied. Payroll vests. Sky the color of tin.

**Courthouse Roof.** Above the water. Flagpole, no flag. Judge alone. Noon, somehow, on a drowned town.

### Faces

**The Grocer.** Apron, flour, a pistol he keeps under the till. Frightened owner, not a robber.

**The Sisters.** Two women in one boat. Practical dresses hiked, hair tied, one older. They shoot because the lane is their boat. The generator cannot change body sex, so the in-game stand-in is clean-shaven with a softer shirt; the final sheets must read as women.

**The Stairs.** Three tannery men, wet to the thigh, climbing. Leader oldest, arms stained.

**The Bellringer.** Soaked black coat, rope burn on the hands. He is ringing and aiming.

**The Tellers.** Four, vests stuffed with notes and lined with steel. Collars still buttoned. Reserve 3.

**The Sexton.** Gray, shovel, cemetery dirt turned to mud. Kind voice, accurate gun.

**The Current.** Two men wasted by the river, shirts plastered, no plates. The miss costs double because of the water, not armor.

**The Last Boats.** Two oarsmen in the thick payroll vests. Reserve 6. Faces sunburned, silks wrong for a flood.

**Judge Pell.** Dry on the roof. Black coat, no hat in his hand, white hair. He sentenced the town at dawn. Not Mather, not the Wire's chief.

---

## White Season — in the game, procedural

Elbow Pass in a closed winter. The eight arenas below are procedural stand-ins (seeds 1401–1408, no painted `Background`). Almost no color: bone, wool, iron, one red crack of sunset. Breath should read. The silver is a small locked box, not a cartoon pile. Prairie and desert props (cactus, skull, rock) are the generator's, not the pass; the final paintings replace them with snow, rime, and the cabin. The red tail of the bar is the cold. Do not paint a gauge.

### Locations

**Elbow Gate.** A shut timber gate, a toll board, first snow, still polite. Pale sun.

**Switchback.** Two figures on a turn, drop to one side, mountain close. Wind lines in the snow.

**The Bowl.** Whiteout distance. Three riders resolving out of nothing. No horizon detail past twenty meters.

**Cache Bowl.** A dug cache, four coats, the box. Steel-gray light, red rock only where they dug.

**Bowl Rim.** One column of green-wood smoke. Sunset rim. She is a speck that becomes a person.

**Saddle Drift.** A drift that was men. Two shapes. Hard blue shadow.

**Saddle Stair.** Ice stair to a cabin. Wind. The procedural shake is cold; paint rime, not a gauge.

**Saddle Cabin.** One door, ice on the roof about to crack. Night-blue snow, a slit of firelight.

### Faces

**The Tollman.** Wrapped, red nose, mitten with the shooting finger cut out. Small man, official about a closed road.

**The Mittens.** Two guides. Mismatched furs, rifles, the look of hired men who changed their minds. One missing a mitten.

**The First White.** Three riders, faces mostly scarf. Leader's beard is ice. They are hard to count on purpose.

**The Cache.** Four burial-party coats with plates. Broad leader. Fur, brass, the box behind them. Reserve 3.

**The Smoke.** Woman on the rim. Hood back so the face reads. Green wood staining her gloves. Calm as the Listener, colder. The generator cannot change body sex, so the in-game stand-in is clean-shaven; the final sheet must read as a woman.

**The Drift.** Two sent down from the cabin. Plates, short line, reserve 3. Almost the same coat as the cache, newer ice.

**The Wind.** Two on the stair. No plates. The cold is the weapon: paint them leaning into wind. Strike 2.

**The Tenant.** Months of beard, cabin-pale, a man who has not been down. No bar. He waits for the ice. Not the Chief, not Pell.

---

## San Isidro — in the game, procedural

A mission on the southern border. The nine arenas below are procedural stand-ins (seeds 1501–1509, no painted `Background`). Adobe, whitewash, a gold that is old. One statue in the yard, hollow, big enough to hide rifles. Oil, not incense, once you are inside. Town props (barrels, fence posts) are the generator's; the final paintings replace them with the arch, the statue, and the cells. The girl who left is not a node.

### Locations

**Mission Gate.** Open arch, bell rope, morning, dust gold. The saint visible small in the yard beyond.

**Candle Court.** Two candles still lit in daylight. Hard shadows. Quiet.

**West Cloister.** A walk of arches. Three figures in file, not breaking step. A rifle wrapped like a relic.

**The Nave.** One singer. Pews. The smell is oil; a painting can show a dark stain, not a label.

**Vestry.** Robes on pegs, four people still wearing theirs, plates under linen. Reserve 3.

**Side Chapel.** Smaller, one novice who stayed. A dead girl's name scratched and then abandoned. Do not letter it so it reads.

**Confession Alley.** Too narrow for three. Two men. Hard light at the far end.

**Hollow Saint.** The statue open at the back. Empty. Two guardians wearing what was inside. Reserve 6. Adobe noon.

**The Cells.** White rooms, one woman in front of a door. No bar anywhere in the frame. She had it carried out.

### Faces

**The Porter.** Old, sun, keys, a pilgrim's patience until the gun. Clean hands.

**The Two Candles.** Two novices. Young. One candle each in the story; in the fight, pistols. Short hair, simple habits that are not costumes from a play.

**The Procession.** Three, mixed ages, walking a rifle out in pieces. Leader carries the stock. Work of the mission, not bandits.

**The Choir.** One woman, mouth still open from the note. Dark habit, accurate hands. The generator cannot change body sex and always puts a hat on, so the stand-in is clean-shaven in a dark hat; the final sheet is a woman, uncovered if the habit allows the face.

**The Vestry.** Four. Vestments over steel. The leader is broad under the cloth. Faces devout and armed. Reserve 3.

**The Novice.** The one who stayed. Young, frightened, honest. She is not the woman the player came for. Clean-shaven stand-in; the final sheet must read as a young woman.

**The Confession.** Two older men, narrow alley, strike 2. Ink on a finger. They forgive in lead.

**The Reliquary.** Two guardians. The plates are church steel, thick, reserve 6. Bareheaded in respect to the opened statue, or hoods back.

**The Superior.** Sixties, uncovered hair, plain dark dress, no jewelry. The bar is absent from the room. A girl is behind her and is not a combatant. Not the Listener, not the Smoke. The stand-in wears a dark bowler because every generated face has a hat; the final sheet is uncovered, no beard, no jewelry.

**The girl who left.** Not a node. Final art may show her once, behind the superior: another woman's name, one rifle, alive.

---

## Sunday Horses — in the game, procedural

Bright's Meadow. The nine arenas below are procedural stand-ins (seeds 1601–1609, no painted `Background`). Start funny: bunting, dust, silk. End with the bunting in the mud. Daylight throughout. The horse is never a fighter and is not a node. Ranch props (hay, a windmill) and town props (barrels) are the generator's; the final paintings replace them with the rail, the ribbon, and one curious horse in the stable shadow.

### Locations

**Bright's Rail.** Starting rail, silk ribbon, morning, crowds painted soft and out of the duel. A boy.

**The Paddock.** Two horses, two riders abreast. Fence, bright shirts. The joke is already over.

**Starting Gate.** Three men on foot where horses should be. Gate open. Advertising failed.

**Betting Rail.** Tickets, a tout, five-to-one chalk that should not be readable words. Noon glare.

**Cade's Stable.** Four grooms, stalls, one curious horse in the back, not armed. Vests sewn heavy.

**Winners' Circle.** A bugler, trampled flowers, late light. The call is over.

**Scale House.** Interior-exterior of a weigh shack. Two stewards. Brass scale. Hard sun through slats.

**Owners' Box.** Elevated, two minders in racing silk over plates. The meadow behind them.

**The Gun.** Cade alone, starter's pistol raised. Ribbon in the mud at the bottom of the frame. Bright and finished pretending.

### Faces

**The Ribbon.** Boy, silk in his hands, too big a smile. Not the Tapper: this one likes a crowd.

**The Pair.** Two jockeys, small, silk, guns awkward in riding hands. One missing a crop. Abreast.

**The Field.** Three dismounted "horses." Leader tall, the others jockey-small. Gate mud on their knees.

**The Tout.** Checked suit, tickets, a grin that is a threat. Sunburn in the part of his hair.

**The Stable.** Four grooms. Leader broad, vest stuffed with notes and steel, reserve 3. Hay on the shoulders.

**The Bugler.** Older, instrument lowered. He tells the truth. Kind, done.

**The Weigh-In.** Two stewards. Exact, humorless. A miss costs double. Ink and chalk.

**The Favorite.** Two minders, not horses. Silk over plate, reserve 6. Faces that know the joke was never for them.

**Cade.** Owner. Pale suit, clean boots in mud, starter's pistol. Bright meadow villain. Not Mather, not Pell, not the Chief.

**The horse.** One painting, curious, in the stable shadow. Not a node.

---

## The Circuit — in the game, procedural

Judge Mather's wagon season. The twelve arenas below are procedural stand-ins (seeds 1701–1712, no painted `Background`). Five towns and a bench. The gallows is a repeated prop: in pieces at Red Ankle, standing at the elm, absent at the bench because he no longer needs to point at it. Paper, ink, rope, dust. Graveyard crosses at Hanging Elm and Elm Shade are the generator's stand-in for one tree; the final paintings are rope and an elm, not a cemetery. The court's clock is the bar. Mather's room has none.

### Locations

**Red Ankle.** Morning wagon, gallows in pieces, a square, a bailiff with a watch. Dust gold.

**Red Ankle Yard.** Two deputies, one warrant, the yard of a small court. Hard sun.

**Red Ankle Square.** Three jurors in a neat line. A clerk's table empty behind them. They want the record tidy.

**Miller's Ford.** A ferry, a widow, brown water, toll board. Overcast.

**Ford Office.** Four clerks, steel boxes, paper. Interior gloom, green lamp.

**Ford Chapel.** Night court. A boy with a broom. Candles. The bench is a table.

**Glass Hill.** Two rifles, the whole road visible and small. Wind, hard light, reserve 3.

**Hill Notary.** A stamp, a desk on a hill that should feel illegal. Strike 2. Late day.

**Hanging Elm.** The tree has more rope than leaves. Three who already served on juries. Dusk.

**Elm Shade.** One woman, cut ropes in a pile, the tree huge. Quiet. Shade.

**Judges' Seat.** No town name on a sign. Two bailiffs in spare robes. The bench building is plain and final.

**The Bench.** Interior. No clock on the wall. Book closed. Mather seated or just stood up. The room is the town.

### Faces

**The Bailiff.** Watch in the palm, your name on a card. Neat, early, believes in lateness as a crime.

**The Deputies.** Two, one paper, two guns. Your name written twice. Younger than the bailiff. Ink on a cuff.

**The Jury.** Three ordinary town people who voted at breakfast. Leader is a shopkeeper type. They are not uniformed. The horror is how tidy they stand.

**The Widow.** Ferry woman. Shawl, strong arms, paid in advance. Not the Listener. She is angry at the convenience, not at you personally, and she shoots anyway. Clean-shaven stand-in in a wide hat; the final sheet must read as a woman.

**The Clerks.** Four. Collars, boxes lined in steel, reserve 3. The leader's box is largest. They love a clean docket.

**The Boy.** Chapel sweeper. Young, not the Tapper and not the Ribbon: he has seen night court. Broom, then a gun he was handed.

**The Glass.** Two riflemen above the circuit. Coats, plates, reserve 3. Distance in the face, windburn.

**The Notary.** A woman with a stamp. Exact. Strike 2. She cares about a spoiled seal more than a corpse. Clean-shaven stand-in; the final sheet must read as a woman.

**The Panel.** Three repeat jurors, dustier than the first jury. Hard green, no plates. They followed the wagon.

**The Elm.** Woman who cuts the rope down after every court. Knife, pile of rope, tired hands. She wants the tree. She is not the superior. Clean-shaven stand-in; the final sheet must read as a woman.

**The Robe.** Two bailiffs in Mather's extra robes. Plates measured on him, reserve 6. The cloth is too long on the shorter one.

**Judge Mather.** No clock. Book closed. Plain dark suit, not Pell's roof and not Cade's pale linen. He looks at people, not at watches. The docket has had your name since the first town.

---

## Do not cross the casts

These are different last rooms: the Chief at a dead key, Pell on a dry roof, the Tenant under cracking ice, the Superior in front of a girl, Cade with a starter's pistol, Mather with no clock. Final sheets must not share a face. Procedural stand-ins for The Wire already use different hat, cloth, and beard so a line does not clone its leader.
