# High Noon — six more roads

**Status:** The Wire, The Flood, White Season, San Isidro, Sunday Horses, and The Circuit are in `Campaign.All` after Boot Hill Night, with procedural arenas and procedural faces. Final-art notes: [CAMPAIGN_ART.md](CAMPAIGN_ART.md).
**Created:** 2026-09-30.
**Language:** English, to match the codebase and `CLAUDE.md`. Banter is the copy for `StageDef.Intro`.

The live chain stays The Playbill → Salt Debt → Boot Hill Night. These six are later roads. A suggested shelf, if they are chained in this order: **The Wire → The Flood → White Season → San Isidro → Sunday Horses → The Circuit**. Any of them can also stand alone.

Shared rules, same as [CAMPAIGN_STORY.md](CAMPAIGN_STORY.md): three hearts for the road, one vest (`ArmorMax`, full every duel), one gun from the locker. A green hit removes that gun's damage. A miss on Sync or Volley makes that foe fire at once and spends `Strike`. Armor at 0 drops the player in that moment and the node costs one heart. Health bar only when reserve is greater than one green. Reaction ignores reserve. Timing ignores reserve. Reaction is set on the stage, once, at the end of each road. No more than two Timing solos in a row.

Gun damage, for the reserve notes below: Revolver 1, Desert Eagle 2, Steampunk 2, Shotgun 3, Sniper 4. Reserve 3 — Sniper and Shotgun end a foe in one green; the others need a second green or a second window. Reserve 6 — nobody ends them in one green; both greens of a Sniper (if it were coop) are irrelevant here, this is solo, so a Sniper needs 2, a Shotgun 2, Desert Eagle and Steampunk 3, a Revolver 6.

New arena strings below are future `ArenaDef` names. New faces are future looks. No `CharacterId` is required to build a road.

---

## 1. The Wire

**Logline.** The telegraph west of the railhead has gone quiet. Someone is cutting it pole by pole. You ride the line with a key and a gun, and every message is a duel. At Mile 90 the chief does not use a key. He waits for the click.

**What the modes are here.** Timing is the sounder: the green is a clean dot. Sync is two keys on one desk, left hand and right, each word its own. A miss is the line sparking back into the vest. Volley is a cutting crew, one pole at a time, until that man's reserve is gone. Reaction is the chief, who answers only the click.

The bar does not need a new widget. The fiction is the key. The existing sweep, the red tail, and the shake stay as they are.

**9 nodes. Rhythm:** solo → pair → line of 3 → solo → four with steel → solo → line of 2 with steel → costly pair → click.

### Chapter 1 — The Railhead

**Tagline:** The line hummed this morning. It doesn't now.

| # | Node | Arena | Diff | Mode | Steel |
|---|---|---|---|---|---|
| 1 | The Tapper | Railhead | Easy | Timing | — |
| 2 | The Night Desk | Dry Creek | Easy | Sync × 2 | Hp 1 |
| 3 | The First Cut | Pole 12 | Normal | Volley × 3 | Hp 1 |

**1. The Tapper.** A boy on the practice key at the railhead. Teaches the bar. He heard the hum die and thinks you are here to fix a bird's nest.

- Opponent: "It sang all morning. Then it didn't."
- You: "A line doesn't just quit."
- Opponent: "Then tap it true. The green is a clean dot."

**2. The Night Desk.** Two night operators, one key each. Your left hand takes the first, your right the second. A green sends your word even if the other hand sparks.

- Opponent: "Two keys. Two words. Don't cross them."
- You: "I didn't come to send a greeting."
- Opponent: "Then don't miss. The spark jumps back."

**3. The First Cut.** Three cutters at the first downed poles. One window each. Any gun ends a window. They have the coils you were sent to save.

- Opponent: "Pole twelve. We started at the easy end."
- You: "You'll finish on the ground."
- Opponent: "Three of us. One pole at a time. If your hand can."

### Chapter 2 — The Empty Stretch

**Tagline:** Forty miles of wire and nobody on it.

| # | Node | Arena | Diff | Mode | Steel |
|---|---|---|---|---|---|
| 4 | The Relief | Alkali Tank | Normal | Timing | — |
| 5 | The Splicers | The Cut | Normal | Sync × 4 | Hp 3, Strike 1 |
| 6 | The Listener | Mile 70 | Normal | Timing | — |

**4. The Relief.** The rider who was supposed to bring fresh coils. He sold them and stayed to see who came looking.

- Opponent: "Coils are worth more cut than strung."
- You: "Not to the man at the far end."
- Opponent: "Tap. He isn't listening anyway."

**5. The Splicers.** Four men in rubber coats, two shots from each hand, reserve 3. Plate under the rubber. First node on this road where the gun decides whether a bar appears. Your left hand takes the first and the third. Your right takes the second and the fourth.

- Opponent: "Four of us. Two hands. The coats are lined."
- You: "Each hand's good for two."
- Opponent: "Steel under the rubber. Don't slip."

**6. The Listener.** She kept a receiver on a fencepost and heard the line go dead in the order the poles fell. She says Mile 90 will not give you a key.

- Opponent: "I heard them cut it. West, then further west."
- You: "Who's at the last station?"
- Opponent: "The chief. He doesn't use a key. He waits for the click."

### Chapter 3 — Mile 90

**Tagline:** The last station answers once.

| # | Node | Arena | Diff | Mode | Steel |
|---|---|---|---|---|---|
| 7 | The Last Poles | Mile 90 Yard | Hard | Volley × 2 | Hp 3, Strike 1 |
| 8 | The Ground | Mile 90 Shed | Hard | Sync × 2 | Hp 1, Strike 2 |
| 9 | The Chief | Mile 90 | Hard | Reaction | ignores Hp |

**7. The Last Poles.** Two guards on the last standing poles, reserve 3. Shorter than the first cut, and this time the coats are lined again.

- Opponent: "These two still sing. That's the point."
- You: "Then I'll cut the singers."
- Opponent: "Both guns. One pole, then the other. Steel under the coats."

**8. The Ground.** Two men holding the ground wire. A pair again, and each miss spends 2. Both missing spends 4 and drops a vest of 3. The spark is the lesson: REINFORCE is how you survive a sloppy ground.

- Opponent: "Hold the ground or it holds you."
- You: "I didn't ride ninety miles to spark."
- Opponent: "Miss once. It costs you twice. Then you meet him."

**9. The Chief.** No key. No bar. He has been waiting for the click, which is BANG. `Type = Reaction` is set on the stage.

- Opponent: "The line is quiet because I said so."
- You: "Then hear this."
- Opponent: "No key. Draw on the click."

**Victory:** "The line hums. Mile 90 answers."
**Defeat:** "Three hearts. The wire stays cut."

Drop The Relief and The Listener for a 7-node cut; move the chief's warning into The Ground. Insert a chapter **The Repeaters** (a solo at a repeater shack, a Sync × 4 with reserve 4, a solo in the empty shack) between stretch and Mile 90 if the road should be longer, and refill hearts on that door.

---

## 2. The Flood

**Logline.** The levee at Lumen broke at dawn. The town drowns by streets. You climb ahead of the water, and the people who mean to keep the high ground are already shooting down. On the courthouse roof the judge draws, because the bars are under the river.

**What the modes are here.** Timing is a shot taken while the street is still a street. The bar's existing crawl from brown to red, and the shake in the last moment, is the water. Do not add a second clock. Sync is two people in one boat, two guns. Volley is a stair: they come up one at a time and do not share a step. Reaction is Judge Pell, waist-deep in the story and dry on the roof.

**9 nodes. Rhythm:** solo → pair → stairs of 3 → solo → four with steel → solo → costly pair → two boats with steel → the roof.

### Chapter 1 — Ankles

**Tagline:** The market is still a market. It won't be by noon.

| # | Node | Arena | Diff | Mode | Steel |
|---|---|---|---|---|---|
| 1 | The Grocer | Lower Market | Easy | Timing | — |
| 2 | The Sisters | Cork Lane | Easy | Sync × 2 | Hp 1 |
| 3 | The Stairs | Tanner's | Normal | Volley × 3 | Hp 1 |

**1. The Grocer.** He has stacked sacks into a wall and will shoot anyone who climbs them. Teaches the bar. The water is still a rumor at his door.

- Opponent: "Shop's closed. The river can wait outside."
- You: "The river isn't asking."
- Opponent: "Then hit the green before it hits the step."

**2. The Sisters.** Two sisters in a skiff jammed sideways in Cork Lane. Left gun, right gun. A green dumps your sister even if the other hand goes in the water.

- Opponent: "One boat. Two of us. Don't swamp it."
- You: "I need the lane."
- Opponent: "Then take your half. Miss, and we fire wet."

**3. The Stairs.** Three men coming up the tanner's stairs, one step at a time. They want the dry rooms. You are in the dry rooms.

- Opponent: "We started at the bottom. You're the top."
- You: "Then come up and fall down."
- Opponent: "Three steps. One at a time. The water's behind us."

### Chapter 2 — Waist

**Tagline:** Chapel Street is a canal. The bank still thinks it's a bank.

| # | Node | Arena | Diff | Mode | Steel |
|---|---|---|---|---|---|
| 4 | The Bellringer | Chapel Street | Normal | Timing | — |
| 5 | The Tellers | Bank Steps | Normal | Sync × 4 | Hp 3, Strike 1 |
| 6 | The Sexton | Chapel Roof | Normal | Timing | — |

**4. The Bellringer.** He is ringing the flood bell and shooting at anyone who treats the church as a boat.

- Opponent: "The bell is for the living. You're late."
- You: "Then stop ringing and move."
- Opponent: "Hit it clean. The water's at the pews."

**5. The Tellers.** Four tellers on the bank steps, two shots a hand, reserve 3. Payroll belts under the coats. First node where the gun matters. The money is why they will not leave the steps.

- Opponent: "Four of us. The belts are lined."
- You: "Each hand's good for two."
- Opponent: "Steel under the wool. The notes stay dry."

**6. The Sexton.** He buried this town for years and has been watching the high ground. He says Judge Pell is already on the courthouse, and Pell will not give you a bar.

- Opponent: "I dug the dry graves. He's on the roof."
- You: "Pell."
- Opponent: "He won't give you a bar. The bars are under the river."

### Chapter 3 — The Roof

**Tagline:** One roof left. The rest is river.

| # | Node | Arena | Diff | Mode | Steel |
|---|---|---|---|---|---|
| 7 | The Current | Flood Street | Hard | Sync × 2 | Hp 1, Strike 2 |
| 8 | The Last Boats | Courthouse Steps | Hard | Volley × 2 | Hp 6, Strike 1 |
| 9 | Judge Pell | Courthouse Roof | Hard | Reaction | ignores Hp |

**7. The Current.** Two men in the fastest water, abreast. Each miss spends 2. The river pulls harder than a bullet: both missing spends 4 and drops a vest of 3.

- Opponent: "Miss in this current and it takes two."
- You: "I didn't climb this far to swim."
- Opponent: "Left and right. Then the boats. Then him."

**8. The Last Boats.** Two of Pell's oarsmen, reserve 6. They wear the payroll vests the tellers died for. Nobody ends either of them in one green. Strike is back to 1: this fight is about staying on the green while the steps disappear.

- Opponent: "He kept the roof. We kept the vests."
- You: "Then the vests can sink."
- Opponent: "Two of us. Thick as the judge's door."

**9. Judge Pell.** The roof is dry. The bar is a story from the street below. He draws on BANG.

- Opponent: "I sentenced this town at dawn."
- You: "The river beat you to it."
- Opponent: "No bar. Draw. Let's see who the roof keeps."

**Victory:** "The roof holds. Pell doesn't."
**Defeat:** "Three hearts. Lumen keeps the rest."

Drop The Bellringer and The Sexton for a shorter climb; put Pell's warning into The Current. A longer flood adds **The Second Storey** between Waist and The Roof: a solo in a bedroom, a Sync × 4 at reserve 4, a solo on a balcony. Refill hearts on that door. The red shake stays the water on every bar; do not build a flood meter.

---

## 3. White Season

**Logline.** Elbow Pass closed in the night. You are taking someone else's silver over the saddle before the thaw, and the people buried with it do not agree. Cold spends armor faster than lead. At the top, a man who wintered in the cabin shoots on the crack of the ice.

**What the modes are here.** Timing is one shot before the finger goes numb. Sync is two rifles in two mittens. Volley is riders coming out of the white, one at a time, because you cannot see the line. Reaction is the tenant of the saddle cabin. Strike 2 on the last pair is the cold itself: the wind takes two links of the vest for every miss.

**8 nodes. Rhythm:** solo → pair → line of 3 → four with steel → solo → line of 2 with steel → costly pair → the crack.

### Chapter 1 — The Gate

**Tagline:** The pass is shut. The silver doesn't care.

| # | Node | Arena | Diff | Mode | Steel |
|---|---|---|---|---|---|
| 1 | The Tollman | Elbow Gate | Easy | Timing | — |
| 2 | The Mittens | Switchback | Easy | Sync × 2 | Hp 1 |
| 3 | The First White | The Bowl | Normal | Volley × 3 | Hp 1 |

**1. The Tollman.** He takes a coin or a bullet for the closed gate. Teaches the bar. The snow is still polite.

- Opponent: "Pass is shut. Toll's the same."
- You: "I'm not staying till thaw."
- Opponent: "Then hit the green before your hand sleeps."

**2. The Mittens.** Two guides who were hired to take the silver and decided to keep it. One rifle each. A green drops yours even if the other mitten misses.

- Opponent: "Two rifles. Two mittens. Don't mix them."
- You: "The silver isn't yours."
- Opponent: "Miss, and the cold collects with us."

**3. The First White.** Three riders out of the weather, one window at a time. You do not see the next until the current one falls.

- Opponent: "You won't count us. The white won't let you."
- You: "I don't need the count."
- Opponent: "One, then the next. If the hand still works."

### Chapter 2 — The Bowl

**Tagline:** The silver was buried where the wind can't find it. They can.

| # | Node | Arena | Diff | Mode | Steel |
|---|---|---|---|---|---|
| 4 | The Cache | Cache Bowl | Normal | Sync × 4 | Hp 3, Strike 1 |
| 5 | The Smoke | Bowl Rim | Normal | Timing | — |

**4. The Cache.** Four who buried the payroll, two shots a hand, reserve 3. Coat-plates. First node where the gun matters. Left hand takes the first and third. Right takes the second and fourth.

- Opponent: "Four of us. The coats are plated."
- You: "Each hand's good for two."
- Opponent: "Steel under the fur. Don't slip."

**5. The Smoke.** A woman burning green wood so someone below will see it. She has watched the saddle all winter. She says the tenant does not use a bar. He uses the ice.

- Opponent: "He's been up there since the first snow."
- You: "Then he's tired."
- Opponent: "He won't give you a bar. He waits for the ice to crack."

### Chapter 3 — The Saddle

**Tagline:** One cabin. The wind spends what the bullets don't.

| # | Node | Arena | Diff | Mode | Steel |
|---|---|---|---|---|---|
| 6 | The Drift | Saddle Drift | Hard | Volley × 2 | Hp 3, Strike 1 |
| 7 | The Wind | Saddle Stair | Hard | Sync × 2 | Hp 1, Strike 2 |
| 8 | The Tenant | Saddle Cabin | Hard | Reaction | ignores Hp |

**6. The Drift.** Two of his, reserve 3, coming out of a drift that looked like a hill. Steel again, and the line is short because the weather is not.

- Opponent: "He sent us down. The drift hid the rest."
- You: "Then the drift can keep you."
- Opponent: "Two windows. Plates under the coats."

**7. The Wind.** Two on the stair, and the cold is the strike. Each miss spends 2. Both missing spends 4 and drops a vest of 3. This is the node that makes REINFORCE a winter tool.

- Opponent: "The wind takes two for every miss."
- You: "Then I won't miss."
- Opponent: "Left and right. Then the door. He likes the crack."

**8. The Tenant.** The cabin door. No bar. The ice on the roof cracks, and that is BANG.

- Opponent: "I wintered on your silver."
- You: "It was never mine. It isn't yours."
- Opponent: "No bar. Draw when the ice speaks."

**Victory:** "The pass stays shut. The silver comes down."
**Defeat:** "Three hearts. Elbow keeps the sled."

Drop The Smoke and fold her warning into The Wind for a 7-node crossing. A longer winter adds **The Second Bowl** after chapter 2: a solo at a dead fire, a Sync × 4 at reserve 4, a solo in an empty corral. Refill hearts on that door. Do not add a temperature meter. Strike and the red tail of the bar are the cold.

---

## 4. San Isidro

**Logline.** The mission of San Isidro keeps a saint in the yard. The statue is hollow, and it is full of rifles. A novice walked out with one of them and another woman's name. You came for her. The superior has already decided the bar is a vanity.

**What the modes are here.** Timing is a bell: the green is the note. Sync is two candles, two hands, a duet the choir rehearsed. Volley is a procession, one figure at a time, and they do not break step for you. Reaction is the superior. She threw the bar out because she says God does not sweep.

**9 nodes. Rhythm:** solo → pair → procession of 3 → solo → four with steel → solo → costly pair → the saint's steel → the superior.

### Chapter 1 — The Yard

**Tagline:** The gate is open. The saint is not.

| # | Node | Arena | Diff | Mode | Steel |
|---|---|---|---|---|---|
| 1 | The Porter | Mission Gate | Easy | Timing | — |
| 2 | The Two Candles | Candle Court | Easy | Sync × 2 | Hp 1 |
| 3 | The Procession | West Cloister | Normal | Volley × 3 | Hp 1 |

**1. The Porter.** He keeps the gate and the bell rope. Teaches the bar. He thinks you are a pilgrim with a gun, which is not wrong.

- Opponent: "The yard is for prayer. Yours is late."
- You: "I'm looking for a woman who left armed."
- Opponent: "Then hit the green. The bell only rings clean."

**2. The Two Candles.** Two novices on the court, one candle, one pistol, each. Left and right. A green drops yours even if the other flame misses.

- Opponent: "Two candles. Two hands. Don't cross the flames."
- You: "She came through here."
- Opponent: "Miss either, and that one fires."

**3. The Procession.** Three in file across the cloister. They will not share a moment. Any gun ends a window. They are walking the rifle out of the saint in pieces.

- Opponent: "We don't break step for a guest."
- You: "You'll break it for a bullet."
- Opponent: "Three of us. One at a time. The saint can wait."

### Chapter 2 — The Nave

**Tagline:** The rifles were never blessed. The steel was.

| # | Node | Arena | Diff | Mode | Steel |
|---|---|---|---|---|---|
| 4 | The Choir | The Nave | Normal | Timing | — |
| 5 | The Vestry | Vestry | Normal | Sync × 4 | Hp 3, Strike 1 |
| 6 | The Novice | Side Chapel | Normal | Timing | — |

**4. The Choir.** One voice left, and she shoots on the note. The nave smells like oil, not incense.

- Opponent: "The hymn is a count. You missed the start."
- You: "I didn't come to sing."
- Opponent: "Then land the green. The note won't wait."

**5. The Vestry.** Four in vestments, two shots a hand, reserve 3. Plates sewn under the cloth, paid for with what came out of the statue. First node where the gun matters.

- Opponent: "Four of us. The cloth is lined."
- You: "Each hand's good for two."
- Opponent: "Steel under the linen. The saint paid for it."

**6. The Novice.** Not the one you came for. This one stayed. She says the woman you want is already with the superior, and the superior does not keep a bar.

- Opponent: "She took a rifle and a dead girl's name."
- You: "Where did she take them?"
- Opponent: "To the cells. The superior won't give you a bar. She threw it out."

### Chapter 3 — The Cells

**Tagline:** The hollow saint is empty. The woman who filled it is not.

| # | Node | Arena | Diff | Mode | Steel |
|---|---|---|---|---|---|
| 7 | The Confession | Confession Alley | Hard | Sync × 2 | Hp 1, Strike 2 |
| 8 | The Reliquary | Hollow Saint | Hard | Volley × 2 | Hp 6, Strike 1 |
| 9 | The Superior | The Cells | Hard | Reaction | ignores Hp |

**7. The Confession.** Two confessors in a lane too narrow for a crowd. Each miss spends 2. Both missing spends 4 and drops a vest of 3. They forgive in lead.

- Opponent: "Confess the miss. It costs you twice."
- You: "I'm not here for pardon."
- Opponent: "Left and right. Then the saint. Then her."

**8. The Reliquary.** Two guardians of the hollow statue, reserve 6. The steel that was inside it is on them now. Nobody ends either in one green. Strike returns to 1.

- Opponent: "She kept the rifle. We kept the plates."
- You: "Then the plates come off."
- Opponent: "Two of us. Thicker than the statue was."

**9. The Superior.** The cells. No bar on the floor, because she had it carried out. She draws on BANG. The novice you came for is the reason, and she is not this fight. She is what the superior is standing in front of.

- Opponent: "God does not sweep a bar for you."
- You: "I didn't ask him to."
- Opponent: "No bar. Draw. The girl stays behind me."

**Victory:** "The saint is empty. The girl walks out."
**Defeat:** "Three hearts. San Isidro keeps the name."

Drop The Choir and The Novice for a shorter office; put the warning into The Confession. A longer mission adds **The Infirmary** between nave and cells: a solo, a Sync × 4 at reserve 4, a solo in a shut ward. Refill hearts on that door. Do not put the missing novice on a node. She is the reason, standing behind the last draw.

---

## 5. Sunday Horses

**Logline.** The county fair at Bright's Meadow kept a racetrack, and the track kept a gang. It starts as a joke: a ribbon, two jockeys, a field. It ends when the owner fires the starter's pistol into the air and calls that BANG. You came for a horse. You stay because the horse was bait.

**What the modes are here.** Timing is the ribbon. Sync is two riders abreast, one gun for each. Volley is the field leaving the gate one horse at a time, which is a lie they tell with their bodies: the men dismount and shoot in order. Reaction is Cade, and the pistol that starts a race is the only one he is willing to use.

This is the bright road. Put it after a grim one. It is still a gunfight by the third chapter.

**9 nodes. Rhythm:** solo → pair → field of 3 → solo → four with steel → solo → costly pair → two minders with steel → the pistol.

### Chapter 1 — The Rail

**Tagline:** The ribbon is a game until it isn't.

| # | Node | Arena | Diff | Mode | Steel |
|---|---|---|---|---|---|
| 1 | The Ribbon | Bright's Rail | Easy | Timing | — |
| 2 | The Pair | The Paddock | Easy | Sync × 2 | Hp 1 |
| 3 | The Field | Starting Gate | Normal | Volley × 3 | Hp 1 |

**1. The Ribbon.** The starter's boy, a length of silk, a smile. Teaches the bar. He has not been told you are trouble.

- Opponent: "Ribbon's up. You blink, you lose."
- You: "I'm not racing."
- Opponent: "Everybody's racing. Hit the green."

**2. The Pair.** Two jockeys, stirrup to stirrup, guns out because the joke got old yesterday. Left and right. A green takes your rider even if the other hand fans the air.

- Opponent: "Two of us. We ride abreast. So do your hands."
- You: "I came for a horse, not a comedy."
- Opponent: "Miss either and that one shoots. The crowd likes a fall."

**3. The Field.** Three out of the gate, one at a time, on their own feet. The horses were the advertisement.

- Opponent: "They think it's a start. It's a line."
- You: "Then the line can stop."
- Opponent: "Three of us. One gate at a time."

### Chapter 2 — The Box

**Tagline:** The odds were written before you arrived.

| # | Node | Arena | Diff | Mode | Steel |
|---|---|---|---|---|---|
| 4 | The Tout | Betting Rail | Normal | Timing | — |
| 5 | The Stable | Cade's Stable | Normal | Sync × 4 | Hp 3, Strike 1 |
| 6 | The Bugler | Winners' Circle | Normal | Timing | — |

**4. The Tout.** He sold your name at five to one and will shoot to protect the ticket.

- Opponent: "I liked you better as a long shot."
- You: "Tear the ticket."
- Opponent: "Hit the green. The price doesn't move."

**5. The Stable.** Four grooms, two shots a hand, reserve 3. Prize money sewn into the vests. First node where the gun matters. The horse you came for is in a stall behind them and is not a combatant.

- Opponent: "Four of us. The vests are stuffed and lined."
- You: "Each hand's good for two."
- Opponent: "Steel under the silk. The horse stays."

**6. The Bugler.** He plays the call and then tells you the truth, because someone should. Cade will fire the starter's pistol. That sound is the only bang he believes in.

- Opponent: "He doesn't keep a bar. He keeps the pistol that starts a race."
- You: "Then I'll be there when he lifts it."
- Opponent: "He waits for that one shot. He only needs you to flinch."

### Chapter 3 — The Gun

**Tagline:** The ribbon is in the mud. The pistol is in his hand.

| # | Node | Arena | Diff | Mode | Steel |
|---|---|---|---|---|---|
| 7 | The Weigh-In | Scale House | Hard | Sync × 2 | Hp 1, Strike 2 |
| 8 | The Favorite | Owners' Box | Hard | Volley × 2 | Hp 6, Strike 1 |
| 9 | Cade | The Gun | Hard | Reaction | ignores Hp |

**7. The Weigh-In.** Two stewards. A light rider is a cheat, and so is a miss: each one spends 2. Both missing spends 4 and drops a vest of 3.

- Opponent: "You come in heavy. A miss costs double."
- You: "I'm not on the scale."
- Opponent: "Left and right. Then his two. Then him."

**8. The Favorite.** Cade's two minders, reserve 6, silks over plate. Nobody ends either in one green. The favorite is a man, not the horse. Strike is 1 again.

- Opponent: "He named us the favorite. The silks are a joke."
- You: "The plates aren't."
- Opponent: "Two of us. Thicker than a purse."

**9. Cade.** The starter's pistol goes up. That is BANG. No bar. The meadow is bright and he is done pretending.

- Opponent: "I start every race in this county."
- You: "This one ends."
- Opponent: "No ribbon. Draw on the pistol."

**Victory:** "The pistol is down. The horse is yours."
**Defeat:** "Three hearts. Bright's Meadow keeps the odds."

Drop The Tout and The Bugler for a shorter card; put Cade's warning into The Weigh-In. A longer fair adds **The Night Card** between The Box and The Gun: a solo under lamps, a Sync × 4 at reserve 4, a solo in an empty stand. Refill hearts on that door. Leave the horse out of the fights.

---

## 6. The Circuit

**Logline.** Judge Mather rides a circuit with a portable gallows and a docket that already has your name in every town. You are not the law. You are the appointment. Five towns, one season, and in the last room he does not look at a clock.

**What the modes are here.** Timing is the court's clock, a green that is the second he allows you. Sync is two deputies reading one warrant, each with a gun. Volley is a jury that voted to be the firing squad and will shoot one at a time so the record stays tidy. Reaction is Mather. He has no clock on the bench. This is the long, mean road. It is a season, not an errand.

**12 nodes. Rhythm:** solo → pair → jury of 3 → solo → four with steel → solo → line of 2 with steel → costly pair → jury of 3 → solo → two in his robes, thick → the bench.

### Chapter 1 — Red Ankle

**Tagline:** The gallows came in on the morning wagon.

| # | Node | Arena | Diff | Mode | Steel |
|---|---|---|---|---|---|
| 1 | The Bailiff | Red Ankle | Easy | Timing | — |
| 2 | The Deputies | Red Ankle Yard | Easy | Sync × 2 | Hp 1 |
| 3 | The Jury | Red Ankle Square | Normal | Volley × 3 | Hp 1 |

**1. The Bailiff.** He has your name on a card and a watch in his palm. Teaches the bar. The gallows is still in pieces.

- Opponent: "You're on today's card. The judge likes a clock."
- You: "I didn't ask for a time."
- Opponent: "You have one. Hit the green or be late."

**2. The Deputies.** Two of them, one warrant, two guns. Left takes the first name on the paper, right takes the second. Both names are yours, written twice. A green drops your deputy even if the other hand smears the ink.

- Opponent: "We read it together. We shoot the same way."
- You: "Read it to the man who wrote it."
- Opponent: "Miss either of us and that one fires."

**3. The Jury.** Three who already voted, now standing in the square as a line. They like the record: one shot, then the next, so the clerk can write them down.

- Opponent: "We voted at breakfast. This is the carrying out."
- You: "You voted without me."
- Opponent: "Three of us. One at a time. The clerk hates a mess."

### Chapter 2 — Miller's Ford

**Tagline:** The ford charges a toll. The court charges you.

| # | Node | Arena | Diff | Mode | Steel |
|---|---|---|---|---|---|
| 4 | The Widow | Miller's Ford | Normal | Timing | — |
| 5 | The Clerks | Ford Office | Normal | Sync × 4 | Hp 3, Strike 1 |
| 6 | The Boy | Ford Chapel | Normal | Timing | — |

**4. The Widow.** She runs the ferry and has ferried Mather twice. She will shoot you because the court paid the toll in advance.

- Opponent: "He paid your crossing. Both ways, he said."
- You: "I'll pay my own."
- Opponent: "Hit the green. The ford doesn't do credit."

**5. The Clerks.** Four clerks, two shots a hand, reserve 3. Warrant boxes lined in steel. First node where the gun matters. Your name is in every box.

- Opponent: "Four of us. The boxes are lined."
- You: "Each hand's good for two."
- Opponent: "Steel under the paper. Don't smudge it."

**6. The Boy.** He sweeps the chapel where Mather holds night court. He has watched the bench. He says the judge owns no clock and will not give you one at the end.

- Opponent: "He looks at you, not at a watch."
- You: "Every town so far had a clock."
- Opponent: "Those were the deputies. He waits. No bar. Not at the end."

### Chapter 3 — Glass Hill

**Tagline:** From the hill the whole circuit is one road.

| # | Node | Arena | Diff | Mode | Steel |
|---|---|---|---|---|---|
| 7 | The Glass | Glass Hill | Hard | Volley × 2 | Hp 3, Strike 1 |
| 8 | The Notary | Hill Notary | Hard | Sync × 2 | Hp 1, Strike 2 |

**7. The Glass.** Two riflemen on the hill, reserve 3, shooting down the road you still have to ride. Steel, and a short line.

- Opponent: "We see every town from here. Including you."
- You: "Then look away."
- Opponent: "Two windows. Plates under the coats."

**8. The Notary.** She stamps the sentence. A pair, and each miss spends 2, because a spoiled stamp is worse than a miss in Mather's book. Both missing spends 4 and drops a vest of 3.

- Opponent: "A bad stamp costs double. So does a miss."
- You: "Don't stamp me."
- Opponent: "Left and right. The elm is next. He is after that."

### Chapter 4 — Hanging Elm

**Tagline:** The tree has more rope than leaves.

| # | Node | Arena | Diff | Mode | Steel |
|---|---|---|---|---|---|
| 9 | The Panel | Hanging Elm | Hard | Volley × 3 | Hp 1 |
| 10 | The Elm | Elm Shade | Hard | Timing | — |

**9. The Panel.** Three who have already stood on juries in the earlier towns. They followed the wagon. One at a time, Hard green, no extra reserve. The horror is the count and the narrow zone.

- Opponent: "We served before. We came to serve again."
- You: "You served him."
- Opponent: "Three windows. Fall in order, or don't."

**10. The Elm.** One quiet solo in the shade, a woman who cuts the rope down after every court and is done cutting it down. She does not warn you. The boy already did. She only wants the tree back.

- Opponent: "I cut the rope after. I'd like to stop."
- You: "Then let me pass."
- Opponent: "Hit it clean. The shade won't judge you. He will."

### Chapter 5 — The Seat

**Tagline:** No town name. The bench is the town.

| # | Node | Arena | Diff | Mode | Steel |
|---|---|---|---|---|---|
| 11 | The Robe | Judges' Seat | Hard | Volley × 2 | Hp 6, Strike 1 |
| 12 | Judge Mather | The Bench | Hard | Reaction | ignores Hp |

**11. The Robe.** Two bailiffs in Mather's spare robes, reserve 6. The cloth is a joke. The plates are the man's real vest, cut twice and worn by other people. Nobody ends either in one green. Strike is 1.

- Opponent: "He has robes to spare. The plates were measured on him."
- You: "Then they won't fit you."
- Opponent: "Two of us. Thick as the bench."

**12. Judge Mather.** The bench. No clock. No bar. He has had your name in the book since Red Ankle, and the book is closed because he is done writing.

- Opponent: "You kept every appointment but this one."
- You: "This one isn't on your clock."
- Opponent: "I don't keep a clock. Draw."

**Victory:** "The docket is blank. The wagon can go home."
**Defeat:** "Three hearts. Mather still has the name."

Drop The Widow, The Boy, and The Elm for a shorter circuit; move the warning into The Notary. Do not refill hearts on this 12-node bill. A longer season inserts **The Fifth Town** between Glass Hill and Hanging Elm: a solo at a dry well, a Sync × 4 at reserve 4, a solo in a shut courthouse. Refill hearts on that door only. Mather stays the only Reaction. Do not give a deputy a bang.

---

## Building one

A road is a `CampaignDef` appended to `Campaign.All`, after whatever currently unlocks it. New arenas are new `ArenaDef`s. New faces are new looks, procedural until a sheet exists. Set `Type`, `Opponents`, `Hp`, and `Strike` on the node. Set Reaction only on the last stage. Victory and defeat lines live on the `CampaignDef`, the way The Playbill's do.

Do not mix these casts. The Wire's chief is not Judge Pell, Pell is not Mather, Mather is not Cade, Cade is not the tenant, the tenant is not the superior. Six endings, six last rooms.
