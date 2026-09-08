# Audio samples (folder-per-sound, random variants)

`AudioBank` maps each logical sound to a **folder** of variant clips. On each play it picks a
random variant (never the same one twice in a row) so sounds don't get repetitive. If a folder
is empty/missing, a procedural fallback (`ProcAudio`) is used, so sound always works.

| logical | folder        | used for                              | notes                              |
|---------|---------------|---------------------------------------|------------------------------------|
| gunshot | `Shots/`      | a duelist fires                       | **weapon library** — for now one default single shot is chosen (distant/shell clips skipped); full choice comes with weapon selection |
| bang    | `Bell/`       | the noon "GO" signal                  | random variant                     |
| death   | `BodyFall/`   | a duelist drops                       | random variant                     |
| tension | `Hearthbeat/` | hidden countdown (loops)              | random variant, plays on a loop    |
| victory | `Win/`        | campaign victory screen               | random variant                     |
| defeat  | `Loose/`      | campaign defeat screen                | random variant                     |
| click   | `Click/`      | any UI button                         | random variant                     |
| music   | `Ambient/`    | menu / map background music           | `MusicPlayer` shuffles these; paused in duels & on Story |
| wind    | `Wind/`       | ambience bed **inside a duel / PvE**  | `MusicPlayer` loops one under the gunfight (add variants freely) |

- Add more variety by dropping more clips into a folder — no code change needed.
- Any Unity-supported format works (.wav / .ogg / .mp3). Only the containing folder matters.
- `Shots/` is reserved for the upcoming **weapon selection**; the default shot is picked by name
  (prefers a clean "single pistol", skips "distance"/"long"/"shell" clips).
- `kenney_music-jingles/` is a raw source library — not auto-played. Move a chosen jingle into
  `Win/` or `Loose/` to use it as the victory/defeat sting.
