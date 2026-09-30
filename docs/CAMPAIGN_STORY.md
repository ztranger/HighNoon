# High Noon — PvE campaign story

**Status:** APPLIED. This is the narrative source of truth for the first solo road, **The Playbill** (`Campaign.All` index 0). The 13-node bill, intros, chapter-3 tagline, and victory/defeat lines are in `Campaign.cs` and `StoryBootstrap.cs`. Two short test roads follow it in the menu chain and are not part of this bill: **Salt Debt**, then **Boot Hill Night**. Lengthening and cutting in §7 are not built. The two-player road is a separate, unbuilt bill: [COOP_CAMPAIGN.md](COOP_CAMPAIGN.md). Do not satisfy it by enabling the 2-player toggle on this roster.
**Created:** 2026-09-30.
**Language:** English, to match the codebase and `CLAUDE.md`. In-game banter below is the copy to put in `StageDef.Intro`.

Read `CLAUDE.md` for how a stage is built (`StageDef.Type`, `Opponents`, `Hp`, `Strike`, `Campaign.Lives` vs `GameSettings.ArmorMax`). This doc says *why* a node is a solo, a pair, a line, or a draw, and what the player should hear before it.

Chapter count and nodes per chapter are free. The spine and the rhythm rule in §2 stay. Lengthening and cutting are in §7.

---

## 1. Logline

Black Jack runs a traveling show that is also a gang. The playbill is the list. The player's partner is already crossed off it: Jack killed them at Devil's Crossroads and took the company. The player rides the bill from the lookout at the edge to the star at the end. Salazar is not a second gang. It is the silver mine that pays the troupe, and the player learns the name when the road reaches the hills.

Three campaign hearts cover the whole 13-node road. Armor is a vest for one town: every duel starts full, and a lost duel spends one heart.

---

## 2. Rules that keep the road coherent

**Fiction of a mode.** Future nodes follow this. Do not invent a new `GameMode` for a mission.

| Duel | On the bill | What the player learns |
|---|---|---|
| **Timing** | One formal number. One person, one bar. | The green is the show. |
| **Sync × 2** | A rehearsed pair. One shot per pistol. | A green hand wounds its foe even when the other misses. A miss makes *that* foe fire at once. |
| **Sync × 4** | Four people, two shots per pistol. | Same two hands, and the costume has steel under it (`Hp` above 1), so the gun you brought matters. |
| **Volley** | An ambush. They do not share one moment. One window at a time until that foe's reserve is gone. | The line is a queue, not a lane duel. |
| **Reaction** | Someone who refuses the show. | Only Black Jack. Set `Type = Reaction` on that stage. Do not infer it from "last node". |

**Rhythm.** No more than two Timing solos in a row. A pair and a line come back before the player can forget the second hand or the queue. Reaction happens once, at the crossroads, and it takes both tricks away: no bar, no second pistol.

**Steel vs hearts.**

- `Campaign.Lives` stays 3 for this bill. Do not refill them between chapters at this length.
- `GameSettings.ArmorMax` (base 3, cap 8, STATS **REINFORCE**) refills at the start of every duel. A miss spends `StageDef.Strike` from the vest. At 0 the standing foes shoot and the player drops in that moment; `ShowPveResult` then removes one heart.
- A health bar is drawn only when `Hp` is greater than the equipped gun's damage. Reaction ignores `Hp`. A Timing stage ignores `Hp` — only Volley and Sync should set it above 1.
- Gun damage (one green hit): Revolver 1, Desert Eagle 2, Steampunk 2, Shotgun 3, Sniper 4.

**Where steel shows up.**

| Reserve | Who feels it |
|---|---|
| 1 | No bar. Every gun ends the window. Acts 1 and most solos. |
| 3 | Revolver needs 3 greens, Desert Eagle and Steampunk need 2, Shotgun and Sniper need 1. First real gun choice (the Headliner), then the payroll guards. |
| 6 | Nobody one-shots. Sniper needs 2, Shotgun 2, Desert Eagle and Steampunk 3, Revolver 6. The Prima's vest. |

Strike stays 1 everywhere except the Chorus Girl, where a miss spends 2. That is the only time the reinforced vest changes the math before the finale.

**Bodies.** Named solos keep their sheet `CharacterId`. People standing behind a leader (the Headliner's three, the payroll's second guard, the Boot Hill mourners, the Chorus clerk) reuse existing cabaret ids. They are not extra map nodes. The Drifter and Black Jack stay procedural looks.

**Save.** Inserting a stage in the middle of a chapter shifts saved `Stage` indexes. Appending, or retuning `Type` / `Hp` / `Strike` on a node that keeps its index, does not. The 2-player menu toggle does not add a partner on Volley or Sync.

---

## 3. The bill

Recommended length: **5 chapters, 13 nodes.** Mode row:

solo → pair → line of 3 → solo → four with steel → solo → solo → line of 2 with steel → solo → line of 3 → pair, costly misses → vest → BANG.

The only two solos in a row are the Showgirl and the Chanteuse: the witness, then the canyon repeating her.

### Chapter 1 — The Frontier

**Tagline:** Dust, heat, and men with nothing to lose.

The player does not know yet that the scrap of paper is a playbill. The show has already left.

| # | Node | Arena | Diff | Mode | Steel | Face |
|---|---|---|---|---|---|---|
| 1 | The Drifter | Prairie | Easy | Timing | — | procedural |
| 2 | The Saloon Singer | Dusty Town | Easy | Sync × 2 | Hp 1 | `saloon_singer` |
| 3 | Canyon Ambush | Red Canyon | Normal | Volley × 3 | Hp 1 | `native_archer` |

**1. The Drifter.** A lookout on the prairie. Teaches the bar. He carries a torn corner of the bill; the partner's name on it is already crossed out. He thinks the player is just another grave waiting for Boot Hill.

- Opponent: "Long way from anywhere, stranger."
- You: "Just passin' through."
- Opponent: "Folks who pass through end up buried here."

**2. The Saloon Singer.** She and the piano man. First time both hands are in the fight, one shot each. A green hand drops its foe even if the other misses. A miss, and that one shoots immediately.

- Opponent: "This saloon's mine. Turn around, cowboy."
- You: "I don't turn around."
- Opponent: "Left and right. Miss either one and you're done."

**3. Canyon Ambush.** The archer and two riders, one window at a time. The line does not share a moment. Last door before the company's country. Still no steel: any gun ends a window.

- Opponent: "Should've watched the ridgeline."
- You: "Three of you. Still not enough."
- Opponent: "Then drop every one of us. If your hand can."

### Chapter 2 — Dust & Bounty

**Tagline:** There's a price on your head — and hungry men to collect it.

Jack has put the player's face on the poster. This chapter is the advance company, not the whole troupe.

| # | Node | Arena | Diff | Mode | Steel | Face |
|---|---|---|---|---|---|---|
| 4 | The Diva | Painted Hills | Normal | Timing | — | `cabaret_singer_4` |
| 5 | The Headliner | Green Valley | Normal | Sync × 4 | Hp 3, Strike 1 | `cabaret_singer_3` |
| 6 | The Showgirl | Salt Flats | Normal | Timing | — | `cabaret_singer_2` |

**4. The Diva.** A solo. She wants the poster with the player's face more than she wants the town.

- Opponent: "This town already has a star. You ain't it."
- You: "Then take a bow."
- Opponent: "Jack paid for the ink. I just deliver it."

**5. The Headliner.** Four of them, two shots per pistol, reserve 3. Corset over steel plates. First node where the equipped gun decides whether a bar appears. The three behind her are chorus, not new names on the map.

- Opponent: "Four of us. And you've only got two hands."
- You: "Each hand's good for two."
- Opponent: "The stays are steel. Don't let either hand slip."

**6. The Showgirl.** Last person who saw the partner alive. She says the ending out loud, early, so the Kid later is a confirmation and not a twist from nowhere: Jack does not play the bar. He waits for the bang.

- Opponent: "I saw your partner at the crossroads. Jack didn't use a bar."
- You: "Then I'll walk there myself."
- Opponent: "He waits for the bang. You'll hear it."

### Chapter 3 — Blood & Silver

**Tagline:** The hills pay the show. The mine is called Salazar.

The name on the payroll is the mine, not another outfit. Ghost Town is the cashier's office.

| # | Node | Arena | Diff | Mode | Steel | Face |
|---|---|---|---|---|---|---|
| 7 | The Chanteuse | Red Canyon | Normal | Timing | — | `cabaret_singer_5` |
| 8 | The Payroll | Ghost Town | Hard | Volley × 2 | Hp 3, Strike 1 | `cabaret_singer` |
| 9 | The Kid | Midnight Mesa | Hard | Timing | — | `lady_1` |

**7. The Chanteuse.** The canyon keeps every encore. She is an echo of the Showgirl, not a new gang. This is the one place two solos sit back to back.

- Opponent: "This canyon keeps every encore. Even the last."
- You: "Then I won't sing."
- Opponent: "Draw. The rocks will do the chorus."

**8. The Payroll.** Two guards on the mine's cash, reserve 3. Shorter than the canyon ambush, and this time the windows have steel. Same gun lesson as the Headliner, told as a queue instead of a pair. The leader keeps `cabaret_singer`; the second guard reuses a troupe look.

- Opponent: "Salazar silver. It dresses the whole company."
- You: "Then the mine can bury them."
- Opponent: "Two of us. Steel under the coats. Take the windows."

**9. The Kid.** Raised by Jack. She hates the bar and treats only the bang as honest. She repeats what the Showgirl already said, and she is proud of it.

- Opponent: "Fastest hand west of the river. That's me."
- You: "You talk faster than you draw."
- Opponent: "Jack won't give you a bar. He waits for the bang. So will I, when he's done with you."

### Chapter 4 — Judgement Day

**Tagline:** The dead don't forgive. Neither does the law.

One night between the mine and noon. The company buries its own and signs the warrant.

| # | Node | Arena | Diff | Mode | Steel | Face |
|---|---|---|---|---|---|---|
| 10 | The Dancer | Boot Hill | Hard | Volley × 3 | Hp 1, Strike 1 | `cabaret_dancer_6` |
| 11 | The Chorus Girl | Salt Flats | Hard | Sync × 2 | Hp 1, Strike 2 | `cabaret_dancer_7` |

**10. The Dancer.** She danced at the funeral Jack paid for — the partner's. Three mourners, one window at a time, Hard green, no extra reserve. The horror is the count and the narrow zone, not a long health bar. She leads; the other two reuse sheet looks.

- Opponent: "I danced at the funeral. Jack paid for the floor."
- You: "Then you can bury him next."
- Opponent: "Three of us. Fall in time, or don't fall at all."

**11. The Chorus Girl.** She and a clerk. A pair again, easier to count than the Headliner's four, and a miss spends 2 armor. This is the node that makes REINFORCE matter. In the same breath she names the Prima's vest: it was cut to stop a rifle once.

- Opponent: "The warrant's signed. Miss once and it costs you twice."
- You: "I didn't come for a show."
- Opponent: "The Prima's vest stopped a rifle. You'll meet it before you meet him."

### Chapter 5 — High Noon

**Tagline:** One street. One bullet. It ends at noon.

The day is short on purpose. Two nodes.

| # | Node | Arena | Diff | Mode | Steel | Face |
|---|---|---|---|---|---|---|
| 12 | The Prima | Gallows Hill | Hard | Volley × 2 | Hp 6, Strike 1 | `cabaret_dancer_8` |
| 13 | Black Jack | Devil's Crossroads | Hard | Reaction | ignores Hp | procedural |

**12. The Prima.** She and a second. Reserve 6: the vest is thicker than any gun in the locker. Still a show — a queue, not a fair draw. Strike returns to 1 so the fight is about staying on the green, not about bleeding out on the first miss.

- Opponent: "I've buried better men than you. And this vest has stopped worse."
- You: "Then I'll keep shootin' till it doesn't."
- Opponent: "Two of us. The gallows can wait."

**13. Black Jack.** The partner's crossroads. No bar. The second hand the road spent twelve nodes teaching is not in this fight. He is the last name because the bill was written that way, and `Type` is set on the stage.

- Opponent: "So. You're the one who won't stay dead."
- You: "And you're the last name on my list."
- Opponent: "Then draw, and let's see whose sun sets."

---

## 4. What the player should understand by the end

1. The bar is the company's trick. Jack refuses it.
2. A pair or a four is a rehearsed number, which is why two hands exist.
3. A line is an ambush that will not share one moment.
4. Steel under a costume is why the gun locker exists. It first matters at the Headliner, again at the payroll, and it decides the Prima.
5. Three hearts are the road. The vest is this town only.
6. A green hand still wounds its foe when the other hand misses. A miss is answered by that foe's gun at once.

---

## 5. Chapter, victory, and defeat cards

Chapter intros stay the shared Story screen: `CHAPTER n/N`, the title, the tagline in §3, then BEGIN.

Victory and defeat copy in `StoryBootstrap` is the bill:

- **Victory:** "The bill is done. The crossroads keeps the name you came for."
- **Defeat:** "Three hearts. The list keeps the last name."

PLAY AGAIN / TRY AGAIN still call `Campaign.StartRun()` and open the chapter intro. MENU leaves.

---

## 6. Lines that stayed

The Drifter, the Saloon Singer, Canyon Ambush, and the Chanteuse keep the banter they already had. The Prima keeps her first exchange and adds the gallows line. Black Jack is unchanged. Every other intro in §3 is the live `StageDef.Intro`.

---

## 7. How to lengthen or cut

Add a node between acts, after the player has met all three grammars (end of chapter 1). A new node either repeats one grammar on a harder green or raises reserve. Do not put a third Timing solo in a row. Do not add a second Reaction.

**Short road, 10 nodes.** Drop the Chanteuse and the Boot Hill line. Move the vest warning into the Kid's last line. What remains: Frontier (3), Dust & Bounty (3), the mine as Payroll + Kid, then High Noon (2). Hearts stay 3.

**Full bill, 16 nodes.** Insert a chapter **The House** between Blood & Silver and Judgement Day: the company's winter quarters in Ghost Town.

| Node | Mode | Why it exists |
|---|---|---|
| The Gate | Timing, Hard | One quiet solo at the door. |
| The Company | Sync × 4, Hp 4, Strike 1 | Sniper one-shots; every other gun still sees a bar. A third steel lesson between 3 and 6. |
| The Empty Hall | Timing, Hard | The room after the number. Then Judgement Day begins. |

At that length, refill campaign hearts to 3 on the door of The House. A 16-node road on a single stack of 3 hearts stops being about aim.

Payroll can stay in chapter 3 if Ghost Town is needed twice, or move into The House if one ghost town should mean one act. Arenas may repeat; the story already repeats Red Canyon and Salt Flats on purpose.

---

## 8. Where it lives in code

`Campaign.Chapters` is this bill: five titles, 13 stages, intros from §3, Blood & Silver tagline from §3. Victory and defeat copy from §5 is in `StoryBootstrap`. Black Jack's `Type = Reaction` is data on the stage. Town Trouble, The Trapper, The Marshal, and The Cabaret Singer are retired titles.

Retuning a node in place does not shift a saved `Stage` index. A run that was standing on the old Ghost Town solo now loads **The Payroll** at that same index. Inserting a node mid-chapter still shifts saved indexes.
