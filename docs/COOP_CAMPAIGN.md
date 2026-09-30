# High Noon — two-player campaign

**Status:** PROPOSAL. Not in `Campaign.cs`. The solo bill in [CAMPAIGN_STORY.md](CAMPAIGN_STORY.md) stays one gunslinger. Do not satisfy this doc by turning the solo 2-player toggle back on: that toggle already drops the second player on Sync and Volley.
**Created:** 2026-09-30.
**Language:** English, to match the codebase and `CLAUDE.md`. Banter below is the copy for `StageDef.Intro`. The pair speaks as `Speaker.You`.

Same world as the solo road: Black Jack's traveling show, Salazar the silver mine that pays it, Devil's Crossroads at the end. This is a second bill for two living partners. It does not resurrect the solo rider's dead partner, and it does not share that run's save index.

Chapter count and nodes per chapter are free. The grip and the "both are in every window" rule in §2 stay. Lengthening and cutting are in §7.

---

## 1. Logline

Two gunslingers still have each other. Jack's show makes its money by splitting pairs: one name takes a solo number, the other gets crossed off the bill. This pair refuses every solo. They walk the same company from the prairie to the crossroads, and a node is won only when both of them are still standing.

Three hearts and one vest cover the pair. A miss by either spends the vest. A lost node spends one heart, and the retry puts both of them back on the street.

---

## 2. Rules

**Grip, every node.** Left half of the screen is player 1 (editor **S**). Right half is player 2 (editor **W**). Both cowboys stand on the left of the street, foes on the right. The thumb does not move between a solo, a pair, a line, and the final draw. Do not use the arcade Coop quadrants (upper/lower of the left half, keys A/D) for this campaign. That skirmish stays a one-off 2v2 in SETUP, with its own tie-break. This road has no tie-break.

**Both play every window.** If a rule would leave one player watching, it is the wrong rule.

| Duel | What both players do | When the node is won |
|---|---|---|
| **Timing** | Two foes, one each. Each has a bar on their own half. Bots have no bar. One round. | Both land green. One miss fails the team. No survivor round. |
| **Sync × 2** | Player 1 owns the even foe (left bar). Player 2 owns the odd foe (right bar). One shot each. | Each foe is at 0. A green wounds your foe even when your partner misses. |
| **Sync × 4** | Same hands. Each player fires twice, front foe then back foe. | Same. Reserve above 1 is why the shared gun matters. |
| **Volley** | One foe at a time, but **two bars on that foe**. Same window, same centre, right bar ahead by `greenHalf × SyncRules.PhaseInGreen` so both can be green. A late second tap still counts. | That foe's reserve hits 0, then the next, until the line is down. |
| **Reaction** | One draw each, on BANG. Player 1 faces Black Jack. Player 2 faces the Prima. | Both lanes are won in that round. A false start loses that lane; the other lane still plays out, then the team loses the node. No tie-break. |

**A miss is never silent.** On Sync and Volley a missed or untouched bar makes that foe fire at once and spends `StageDef.Strike` from the shared vest. Two misses in one window are two shots and two spends. Armor at 0 drops the pair in that moment. The node then costs one heart.

**Steel vs hearts.**

- `Campaign.Lives` stays 3 for this 12-node bill. Do not refill them between chapters at this length.
- One vest: `GameSettings.ArmorMax` (base 3, cap 8, STATS **REINFORCE**), full at the start of every duel. One ARMOR readout.
- One gun: both carry `Weapons.Selected`. A second locker is out of scope. Each green removes that gun's damage, so two greens on one Volley window remove it twice.
- Health bar only when `Hp` is greater than one green of that gun. Two greens can skip a bar the solo road would show: a Sniper (4) into reserve 3 removes the foe in one hit from either player; both hitting a Revolver (1) window removes 2.
- Gun damage: Revolver 1, Desert Eagle 2, Steampunk 2, Shotgun 3, Sniper 4.
- Reaction ignores `Hp`. Timing ignores `Hp`. Only Volley and Sync set reserve above 1.

| Reserve | Who feels the shared gun |
|---|---|
| 1 | No bar. One green from either player ends that foe. |
| 3 | One Sniper or Shotgun green ends them. Desert Eagle and Steampunk need 2, which a single window can do if both hit. Revolver needs a second window unless both keep hitting. Headliner, then the payroll. |
| 6 | Nobody ends them in one green. Both hitting with a Sniper (8) does. The Seconds, the dress rehearsal of the vest. |

Strike stays 1 except **The Warrant**, where each miss spends 2. Both missing that pair spends 4 and drops a vest of 3. REINFORCE is how the pair survives a sloppy warrant: armor 5 still stands after two Strike-2 misses (5 − 2 − 2 = 1).

**Bodies.** The named leader keeps the sheet `CharacterId`. The second body reuses a troupe look. No new ids are required. Jack stays procedural. The Prima at the finale uses `cabaret_dancer_8` and is not the leader of The Seconds — those two wore copies of her vest and already fell.

**Save.** Own PlayerPrefs key when this is built. A solo `Stage` index must not open a coop node, and the reverse. Inserting a node mid-chapter shifts that campaign's saved index only.

**Arcade Coop** (SETUP, one match, bot bars, closest-to-centre, tie-break) is not this road.

---

## 3. The bill

Recommended length: **5 chapters, 12 nodes.** Both players have a bar or a draw on every one.

Row: 2v2 solo → pair → line of 3 → 2v2 → four with steel → 2v2 → line of 2 with steel → 2v2 → line of 3 → costly pair → vest → two draws.

### Chapter 1 — The Frontier

**Tagline:** Two guns. The show has already left town.

They do not know yet that the scrap of paper is a playbill of pairs.

| # | Node | Arena | Diff | Mode | Steel | Leader |
|---|---|---|---|---|---|---|
| 1 | The Lookouts | Prairie | Easy | Timing | — | procedural |
| 2 | The Duet | Dusty Town | Easy | Sync × 2 | Hp 1 | `saloon_singer` |
| 3 | The Ridgeline | Red Canyon | Normal | Volley × 3 | Hp 1 | `native_archer` |

**1. The Lookouts.** Two watchers, one each. Teaches the grip and the team rule: both green, or the node is lost.

- Opponent: "Two of you. The prairie usually gets one."
- You: "We're just passin' through."
- Opponent: "Then both of you can stay in it."

**2. The Duet.** The singer and the piano man. Player 1 takes the singer, player 2 the piano. A green drops your own foe even if your partner misses. A miss, and that one shoots the vest immediately.

- Opponent: "This number is a duet. So are you."
- You: "We didn't come to sing."
- Opponent: "Left and right. Drop yours, or wear the shot."

**3. The Ridgeline.** Three riders, one at a time, two bars on each. Any green bites. Two greens bite twice. A miss still draws return fire before the window moves on.

- Opponent: "Should've watched the ridgeline."
- You: "Three of you. Two of us. Count again."
- Opponent: "One at a time. Both of you, every time."

### Chapter 2 — Dust & Bounty

**Tagline:** There's a price on both your heads.

Jack has printed the pair. The advance company fields two for every name.

| # | Node | Arena | Diff | Mode | Steel | Leader |
|---|---|---|---|---|---|---|
| 4 | The Poster | Painted Hills | Normal | Timing | — | `cabaret_singer_4` |
| 5 | The Headliner | Green Valley | Normal | Sync × 4 | Hp 3, Strike 1 | `cabaret_singer_3` |
| 6 | The Witnesses | Salt Flats | Normal | Timing | — | `cabaret_singer_2` |

**4. The Poster.** The diva and the printer. Two faces on the sheet, two bars. She wants the poster more than the town.

- Opponent: "Jack paid for both names. I just deliver the ink."
- You: "Then take the bow together."
- Opponent: "Hit the green. Both of you. The poster doesn't smudge."

**5. The Headliner.** Four of them, two shots for each player, reserve 3. First node where the shared gun decides whether a bar appears, and whether one window is enough. The three behind her are chorus, not new map names. Player 1 takes foe 1 then foe 3. Player 2 takes foe 2 then foe 4.

- Opponent: "Four of us. And you've only got two hands."
- You: "One hand each. That's the deal."
- Opponent: "The stays are steel. Don't let either of you slip."

**6. The Witnesses.** Last pair who saw a team walk into Jack's number and come out as one. They say the ending early: he will not give you a bar. He will put each of you on your own draw and wait for one to blink.

- Opponent: "We saw a pair at the crossroads. Only one name rode out."
- You: "We're not giving him the spare."
- Opponent: "He waits for the bang. He only needs one of you to miss it."

### Chapter 3 — Blood & Silver

**Tagline:** The hills pay the show. The mine is called Salazar.

Ghost Town is the cashier's office. The mine's name is on the payroll, not on a second gang.

| # | Node | Arena | Diff | Mode | Steel | Leader |
|---|---|---|---|---|---|---|
| 7 | The Payroll | Ghost Town | Hard | Volley × 2 | Hp 3, Strike 1 | `cabaret_singer` |
| 8 | The Kid | Midnight Mesa | Hard | Timing | — | `lady_1` |

**7. The Payroll.** Two guards, reserve 3, both bars on whichever guard is up. Shorter than the ridgeline, and this time the coats are lined.

- Opponent: "Salazar silver. It dresses the whole company."
- You: "Then the mine can bury them."
- Opponent: "Two windows. Both guns on each. Steel under the coats."

**8. The Kid.** She and the one Jack assigned her, so the pair cannot take a solo. She hates the bar and treats only the bang as honest. She repeats the witnesses on purpose.

- Opponent: "Fastest hand west of the river. He gave me a partner so you'd have to split."
- You: "We don't split."
- Opponent: "Jack won't give you a bar. One bang each. He only needs one miss."

### Chapter 4 — Judgement Day

**Tagline:** The dead are the pairs who let go.

One night between the mine and noon. The company buries the teams it broke and signs a warrant with two names.

| # | Node | Arena | Diff | Mode | Steel | Leader |
|---|---|---|---|---|---|---|
| 9 | The Funeral | Boot Hill | Hard | Volley × 3 | Hp 1, Strike 1 | `cabaret_dancer_6` |
| 10 | The Warrant | Salt Flats | Hard | Sync × 2 | Hp 1, Strike 2 | `cabaret_dancer_7` |

**9. The Funeral.** She danced at the funeral of a pair who took turns instead of shooting together. Three mourners, both bars on each, Hard green, no extra reserve. The pressure is the count and the narrow zone.

- Opponent: "I danced for a pair who took turns. Jack paid for the floor."
- You: "Then you can bury him next."
- Opponent: "Three windows. Both of you on every one, or join them."

**10. The Warrant.** She and a clerk. A pair again, and each miss spends 2 from the shared vest. Both missing spends 4 and drops a vest of 3. She names the last street: the Prima stands with Jack, one draw each. The vest you have not met yet was cut to stop a rifle once.

- Opponent: "The warrant has two names. Miss once and it costs you twice."
- You: "We didn't come for a show."
- Opponent: "Jack takes the left. The Prima takes the right. Her vest stopped a rifle. Yours hasn't."

### Chapter 5 — High Noon

**Tagline:** One street. Two bullets. It ends at noon.

The day is short on purpose. Two nodes. The Prima is alive for the draw because The Seconds wore the copies.

| # | Node | Arena | Diff | Mode | Steel | Leader |
|---|---|---|---|---|---|---|
| 11 | The Seconds | Gallows Hill | Hard | Volley × 2 | Hp 6, Strike 1 | `cabaret_singer_5` |
| 12 | The Crossroads | Devil's Crossroads | Hard | Reaction | ignores Hp | Jack procedural, Prima `cabaret_dancer_8` |

**11. The Seconds.** Two understudies in the Prima's vest, reserve 6 each, both bars on the one who is up. Strike returns to 1 so the fight is about staying on the green together. No gun ends either of them in a single green. Both hitting with a Sniper does.

- Opponent: "She kept the original. We got the steel."
- You: "Then the steel can fall first."
- Opponent: "Two of us. Both your guns. The gallows can wait."

**12. The Crossroads.** No bars. Player 1 draws on Black Jack. Player 2 draws on the Prima. The node is won only if both lanes are won. A false start kills that lane's fight at once; the other draw still happens, and the team still loses the heart. `Type = Reaction` is set on the stage.

- Opponent: "So. The pair that won't split."
- You: "Two names. Yours comes off first."
- Opponent: "Then draw. Left takes me. Right takes her. Let's see whose sun sets."

Jack says the last line. The Prima does not get a separate intro: she is the right-hand lane of this node, not a thirteenth stage.

---

## 4. What the pair should understand by the end

1. The bar is the company's trick. Jack refuses it, and he tries to make only one of you face that refusal.
2. A duet is two foes and two hands. Your green counts even when your partner misses, and their miss is your vest.
3. A line is one foe at a time with both guns on them. Taking turns is how the funeral pairs died.
4. The shared gun matters when reserve is 3, and it decides The Seconds at 6.
5. Three hearts are the road. The vest is this town only, and it is one vest.
6. The last street is two draws. Winning yours is not enough.

---

## 5. Chapter, victory, and defeat cards

Chapter intros use the shared Story screen: `CHAPTER n/N`, the title, the tagline in §3, then BEGIN.

Target flavor, separate from the solo lines in `StoryBootstrap`:

- **Victory:** "Both names stay. The crossroads takes his."
- **Defeat:** "Three hearts. One miss, and the bill takes you both."

PLAY AGAIN / TRY AGAIN start this coop run from chapter 1. MENU leaves. A solo victory must keep the solo sentence.

---

## 6. How this differs from the live solo bill

| Solo node | Coop node | Why it changed |
|---|---|---|
| The Drifter, one bar | The Lookouts, two bars | The team rule is the first lesson. |
| The Saloon Singer, one body, two thumbs | The Duet, one thumb each | The second player is the right hand. |
| Canyon Ambush, one bar per window | The Ridgeline, two bars per window | Nobody sits out a rider. |
| The Diva | The Poster | Two faces on the ink. |
| The Headliner | The Headliner | Same four, but the two shots belong to two people. |
| The Showgirl | The Witnesses | They saw a pair split. These two are still both alive. |
| The Chanteuse | — | The canyon echo is inside The Ridgeline. Her look moves to The Seconds. |
| The Payroll, one bar | The Payroll, two bars | Same guards, both guns. |
| The Kid, one bar | The Kid, two bars | Jack assigned her a partner so the pair would have to split. |
| The Dancer | The Funeral | She danced for a pair who took turns. |
| The Chorus Girl | The Warrant | Same costly misses. She assigns Jack left and the Prima right. |
| The Prima, Volley, Hp 6 | The Seconds, Volley, Hp 6 | The vest falls here. The Prima lives to draw. |
| Black Jack, one Reaction | The Crossroads, two Reactions | Player 2's draw is the Prima. |

---

## 7. How to lengthen or cut

Add a node between acts, after chapter 1 has taught the grip. A new node is still a two-player fight. Do not add a solo bar. Do not add a second Reaction. Do not put three Timing 2v2s in a row.

**Short road, 9 nodes.** Drop The Witnesses, The Kid, and The Funeral. Move the split-draw warning into The Warrant's intro. What remains: Frontier (3), The Poster, The Headliner, The Payroll, The Warrant, The Seconds, The Crossroads. Hearts stay 3.

**Full bill, 15 nodes.** Insert **The House** between Blood & Silver and Judgement Day, and refill hearts to 3 on that door.

| Node | Mode | Why it exists |
|---|---|---|
| The Gate | Timing, Hard | One quiet 2v2 at the winter quarters. |
| The Company | Sync × 4, Hp 4, Strike 1 | Sniper still ends a foe in one green. Everyone else needs both players or a second pass. |
| The Empty Hall | Timing, Hard | The room after the number. Then Judgement Day begins. |

---

## 8. What is not built

`Campaign.Chapters` is the solo bill only. Building this road wants:

- A second chapter list and a save key that cannot resume a solo run.
- A roster of two humans on the left, zones left-half / right-half, on every node.
- Timing: the existing PvE rule (every human lands green), and no tie-break.
- Sync: each human owns one hand, instead of two inputs on one cowboy.
- Volley: two bars per foe per window, each green applying the shared gun's damage, each miss firing that foe and spending the vest.
- Reaction: both lanes must be won in the same round. The other lane still resolves after a false start. Then one heart, together.
- Menu entry for this run. The solo campaign does not grow a 2-player switch.

Until that exists, two players who want a single match still use arcade Coop in SETUP.
