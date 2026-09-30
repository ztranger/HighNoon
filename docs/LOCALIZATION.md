# High Noon — localization

**Status:** DESIGN + Phase 0 in progress.
**Created:** 2026-09-30.
**Language:** English, to match the codebase and `CLAUDE.md`.
**Goal:** ship **English + Russian** now, and make adding a locale later a drop-in — a new file + a font, no engine rewrite.

Read `CLAUDE.md` first. This doc says how strings are stored, requested, and migrated off the current hardcode.

---

## 1. Approach (decided)

A small **custom `Loc`** system, code-first, files under `Resources/Locale/`. Unity's Localization package was considered and rejected: it is built around editor String-Table assets and prefab `LocalizeStringEvent` components, while every screen here is built in code at runtime. A custom `Loc` fits the bootstrap style, has no package dependency, and is trivial to drive via MCP. The one thing we hand-roll that the package gives for free is Russian plurals — a ~15-line helper (§6).

- **Custom `Loc`** — decided.
- **Own font with Cyrillic** — allowed; wired through `Fonts.Default` (§7).
- **Translations** — done in-repo (both `en` and `ru` authored here).
- **This doc** — the source of truth for the migration.

---

## 2. File format

One file per locale: `Assets/_Project/Resources/Locale/<code>.txt` (`en.txt`, `ru.txt`). Loaded as a `TextAsset`, parsed once, cached.

Format is flat `key = value`, one per line (Java-`.properties` style — translator-friendly, no JSON dependency, and `JsonUtility` cannot deserialize an arbitrary dictionary anyway):

```
# comment line, ignored
menu.play        = PLAY
result.rematch   = REMATCH
hud.armor        = ARMOR {0}/{1}
result.cleared   = STAGE CLEARED!\nNext: {0}
```

Rules:
- Split on the **first** `=`. Key and value are trimmed.
- `#` (or `//`) as the first non-space char → comment.
- Blank lines ignored.
- `\n` in a value becomes a newline; `\\` is a literal backslash. (Values are single-line in the file.)
- Keys are case-sensitive. A value may contain `=`.
- `{0}`, `{1}` … are `string.Format` placeholders (§5).

`en.txt` is the master: it must contain **every** key. Other locales may be partial — missing keys fall back to `en`.

---

## 3. Keys

Namespaced, dot-separated, lower-case: `area.thing[.detail]`. Examples:

| Key | English |
|---|---|
| `menu.play` | `PLAY` |
| `menu.campaign` | `PvE CAMPAIGN` |
| `result.rematch` / `result.menu` / `result.retry` / `result.map` | button labels |
| `duel.bang` | `BANG!` |
| `duel.falsestart` | `FALSE START` |
| `duel.miss` / `duel.hit` / `duel.perfect` | timing popups |
| `hud.armor` | `ARMOR {0}/{1}` |
| `hud.newdraw` / `hud.newaim` | `NEW BEST DRAW!` / `NEW BEST AIM!` |
| `weapon.revolver` … | gun names |
| `arena.prairie` … | arena **display** names (not the id, see §8) |
| `stage.<id>.title` / `stage.<id>.intro.0` | campaign content (§8) |

We request strings with **raw string keys** — `Loc.T("menu.play")` — not a constants class. One source of truth (the `.txt`), and a typo shows immediately in-game as `⟪menu.play⟫`. A `LocKeys` constants class was considered and skipped as boilerplate.

---

## 4. API

`HighNoon.Loc` (static):

```csharp
Loc.T("menu.play")                       // "PLAY"
Loc.T("hud.armor", hp, max)              // "ARMOR 2/3"
Loc.T("result.cleared", stageTitle)      // "STAGE CLEARED!\nNext: The Drifter"
Loc.Plural("hud.lives", n)               // RU-aware plural (§6)

Loc.Locale                                // "en" | "ru"  (current)
Loc.Available                             // ["en","ru"]  (registered)
Loc.SetLocale("ru")                       // load + persist; caller reloads the scene to rebuild UI
```

Resolution order for `T`: current locale → `en` → `⟪key⟫`. `T` with args runs `string.Format`; if formatting throws (bad placeholder), it returns the raw template so nothing crashes mid-duel.

Locale is stored in `GameSettings.Locale` (PlayerPrefs `hn_locale`, flushed via `SaveData.Save`). On first run, empty → auto-detect from `Application.systemLanguage` (Russian → `ru`, else `en`). Adding a locale = register its code in `Loc.Registered` + ship its `.txt` + ensure the font covers its glyphs.

---

## 5. Formatted / dynamic strings

Use positional placeholders only, and let the **translation** own word order — never concatenate a sentence from pieces. Current dynamic strings to move (from `DuelManager` / `DuelHUD`):

| Key | en template |
|---|---|
| `hud.armor` | `ARMOR {0}/{1}` |
| `duel.reaction.ms` | `{0} ms` |
| `result.cleared` | `STAGE CLEARED!\nNext: {0}` |
| `result.died` | `YOU DIED\nLives left: {0}` |
| `result.wins` | `{0} WINS!` |
| `result.teamwins` | `TEAM {0} WINS!` |
| `hud.pvestatus` | `CH{0} · {1}/{2}   ·   {3}   ·   LIVES {4}   ·   {5}` |

The PvE status line concatenates a mode tag (`SOLO`/`CO-OP`/`VOLLEY`/`SYNC`) — those become their own keys (`tag.solo` …) and are passed in as `{5}`.

---

## 6. Plurals (Russian)

Russian has three numeric forms. `Loc.Plural("hud.lives", n)` picks a key suffix and formats with `n` as `{0}`:

```
# en.txt
hud.lives.one   = {0} life left
hud.lives.other = {0} lives left
# ru.txt
hud.lives.one   = Осталась {0} жизнь
hud.lives.few   = Осталось {0} жизни
hud.lives.many  = Осталось {0} жизней
```

Form selection:
- **en**: `n == 1` → `one`, else `other`.
- **ru**: `n%10==1 && n%100!=11` → `one`; `n%10 in 2..4 && n%100 not in 12..14` → `few`; else → `many`.

Missing form falls back `few→other→one`, then to `⟪key⟫`. **MVP note:** where a plural is awkward we phrase around it (`Lives: {0}`) and skip `Plural`; the helper is there for the places that read better with it.

---

## 7. Font (Cyrillic)

Every screen builds text with `Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf")` (7 files). That is routed through one indirection:

```csharp
Fonts.Default   // Resources.Load<Font>("Fonts/AppFont") ?? built-in LegacyRuntime/Arial
```

Bundled: **PT Sans (Regular)** at `Assets/_Project/Resources/Fonts/AppFont.ttf` — SIL OFL (ParaType), license in `AppFont-OFL.txt` beside it. PT Sans was commissioned for Latin **and** Cyrillic, so `ru` renders correctly. `Fonts.Default` loads it and only falls back to the built-in font if it is missing. To change the look later (e.g. a serif "wanted-poster" feel), drop a different Cyrillic-covering OFL TTF as `AppFont.ttf` — PT Serif / Alegreya are options. Ф1 routes every screen's text through `Fonts.Default`. Still worth a **device check** once the UI is wired.

---

## 8. Content & the arena-name gotcha

- **Campaign narrative** (`Campaign.cs` + `CampaignRoster.cs`): road/chapter titles, taglines, blurbs, victory/defeat, stage titles, `DialogLine` intros. Each gets a key (`stage.drifter.title`, `stage.drifter.intro.0`, `chapter.frontier.title`, …). `docs/CAMPAIGN_STORY.md` / `COOP_CAMPAIGN.md` stay the English narrative source.
- **Arena names are identifiers, not just labels.** `StageDef.Arena == "Prairie"` is matched against `ArenaDef.Name`. **Do not** localize that string. Add a separate `arena.prairie` display key and localize *that*; the lookup id stays English.
- **Weapon names** (`Weapon.cs`): `WeaponDef.Name` is display only (`Key` is the id) → `weapon.<key>`.

---

## 9. Gotchas

1. **Font / Cyrillic** — §7. Verify on device.
2. **Arena / weapon id vs display** — §8. Never localize a lookup key.
3. **Word order** — translate whole phrases; positional args only; no string-building sentences.
4. **Text length** — RU runs ~10–30% longer. Audit buttons/labels for overflow (many already set `HorizontalWrapMode.Overflow`; fixed-width buttons are the risk).
5. **Runtime switch rebuilds UI** — screens are code-built, so `SetLocale` persists and the caller **reloads the current scene** (`DuelFlow`) to rebuild. Menu can rebuild its panels in place.
6. **Uppercasing** — `ToUpperInvariant` is correct for Cyrillic; prefer authoring the string already-cased in the `.txt` over forcing case in code.
7. **SDK layer** (`Assets/Scripts/`, Usercentrics consent, etc.) has its own localization and is out of scope here.

---

## 10. Migration phases

- **Ф0 — framework.** `Loc`, `Fonts`, `GameSettings.Locale`, `en.txt` + `ru.txt` seeded, locale toggle in the SOUND/SETTINGS tab. Game still runs in English through `Loc`; nothing else changed. *(in progress)*
- **Ф1 — UI chrome.** Route all text through `Fonts.Default`; move menu (5 tabs + `CampaignSelect`), `DuelHUD` buttons/labels, `Story`, `Tutorial`, `Map` onto keys.
- **Ф2 — dynamic/format.** `DuelManager` / `DuelHUD` templates (§5), mode tags, banners.
- **Ф3 — narrative.** `Campaign.cs` + `CampaignRoster.cs` content → keys; author `ru` translations. Largest, translation-heavy.
- **Ф4 — weapons + arena display names.**

Each phase keeps the game shippable in English; `ru` fills in as keys land.

---

## 11. How to add a locale later

1. Copy `en.txt` → `Resources/Locale/<code>.txt`, translate the values.
2. Add `<code>` to `Loc.Registered`.
3. Ensure `Fonts.Default`'s TTF covers the script (or add a per-locale font).
4. (Optional) add a plural rule for the language in `Loc.PluralForm`.

No recompile is needed for the translations themselves once the locale is registered. Shipping locales that can be added without a rebuild (LiveOps) would move the files to `StreamingAssets` / remote — deferred.
