using UnityEngine;

namespace HighNoon
{
    public enum Speaker { You, Opponent }

    /// <summary>A single line of pre-duel banter.</summary>
    public class DialogLine
    {
        public Speaker Speaker;
        public string Text;
        public DialogLine(Speaker speaker, string text) { Speaker = speaker; Text = text; }
    }

    /// <summary>One PvE encounter (a node on a chapter map): arena + opponent + difficulty + how the duel is fought.</summary>
    public class StageDef
    {
        public string Title;
        public string Arena;
        public Difficulty Difficulty;

        /// <summary>
        /// Rules for this mission. Defaults to <see cref="DuelType.Timing"/>.
        /// Set <see cref="DuelType.Reaction"/> for a quick-draw (the final boss),
        /// <see cref="DuelType.Volley"/> for one gunslinger against <see cref="Opponents"/> foes, and
        /// <see cref="DuelType.Sync"/> for two bars at once. <see cref="Opponents"/> is the line
        /// (even, 2 or 4): each pistol fires <c>Opponents / 2</c> shots.
        /// </summary>
        public DuelType Type = DuelType.Timing;

        /// <summary>
        /// How many foes stand on the right. Volley: clamped to 2–3 by <see cref="Volley.FoeCount"/>
        /// (0 means a full line of 3). Each window repeats until that foe's reserve is empty.
        /// Sync: even 2 or 4 via <see cref="SyncRules.FoeCount"/>
        /// (0 means 2). Two foes = one shot per pistol; four foes = two shots per pistol.
        /// Ignored for Reaction and Timing.
        /// </summary>
        public int Opponents;

        /// <summary>
        /// How many points of reserve each foe on this stage has. 1 (the default) dies to any
        /// single hit. Higher values need several green taps in Volley and Sync; Reaction and
        /// Timing ignore it. A health bar shows only when this is more than the player's weapon damage.
        /// </summary>
        public int Hp = 1;

        /// <summary>Armor the player loses on a miss against these foes. Default 1. Volley and Sync only.</summary>
        public int Strike = 1;

        /// <summary>
        /// Slider window length as the number of pointer passes (edge-to-edge sweeps) before an untapped
        /// bar is a forced miss. 1 = the marker crosses once and reaching the far end is a loss; higher =
        /// more sweeps. The last <see cref="TimingRules.RedFromPasses"/> passes redden the bar (final pass
        /// = red + shake). Leave 0 to use the difficulty default (<see cref="TimingRules.PassesFor"/>:
        /// Easy 4 / Normal 3 / Hard 2); set a positive value to override it on this stage. Used by Timing,
        /// Volley (each foe window) and Sync (each pair wave); ignored by Reaction.
        /// </summary>
        public int Passes; // 0 = auto (by difficulty)

        public CowboyLook Look;
        public DialogLine[] Intro;
    }

    /// <summary>A PvE chapter: a titled map made of ordered stage nodes.</summary>
    public class ChapterDef
    {
        public string Title;
        public string Tagline;  // shown on the chapter intro screen
        public Color Theme;     // map background tint
        public StageDef[] Stages;
    }

    /// <summary>One selectable PvE road. Clearing it unlocks the next entry in <see cref="Campaign.All"/>.</summary>
    public sealed class CampaignDef
    {
        public string Id;
        public string Title;
        public string Blurb;
        public string Victory;
        public string Defeat;
        public ChapterDef[] Chapters;
    }

    /// <summary>
    /// PvE roads. <see cref="Campaign.All"/> is the chain the menu lists; <see cref="Campaign.Chapters"/>
    /// is the road currently in progress. Static so run state survives scene changes (Map ↔ Duel).
    /// </summary>
    public static class Campaign
    {
        static readonly ChapterDef[] FrontierChapters =
        {
            new ChapterDef
            {
                Title = "The Frontier",
                Tagline = "Dust, heat, and men with nothing to lose.",
                Theme = new Color(0.72f, 0.58f, 0.34f),
                Stages = new[]
                {
                    new StageDef
                    {
                        Title = "The Drifter", Arena = "Prairie", Difficulty = Difficulty.Easy,
                        Look = new CowboyLook { Shirt = new Color(0.55f, 0.50f, 0.35f), HatColor = new Color(0.30f, 0.24f, 0.16f), Accent = new Color(0.66f, 0.34f, 0.24f), HatType = HatStyle.Wide, Chest = Accessory.Poncho, Facial = FacialHair.Mustache },
                        Intro = new[]
                        {
                            new DialogLine(Speaker.Opponent, "Long way from anywhere, stranger."),
                            new DialogLine(Speaker.You,      "Just passin' through."),
                            new DialogLine(Speaker.Opponent, "Folks who pass through end up buried here."),
                        },
                    },
                    new StageDef
                    {
                        Title = "The Saloon Singer", Arena = "Dusty Town", Difficulty = Difficulty.Easy,
                        Type = DuelType.Sync, Opponents = 2,
                        Look = new CowboyLook
                        {
                            Shirt = new Color(0.72f, 0.18f, 0.22f),
                            Accent = new Color(0.90f, 0.72f, 0.28f),
                            CharacterId = "saloon_singer",
                        },
                        Intro = new[]
                        {
                            new DialogLine(Speaker.Opponent, "This saloon's mine. Turn around, cowboy."),
                            new DialogLine(Speaker.You,      "I don't turn around."),
                            new DialogLine(Speaker.Opponent, "Left and right. Miss either one and you're done."),
                        },
                    },
                    new StageDef
                    {
                        Title = "Canyon Ambush", Arena = "Red Canyon", Difficulty = Difficulty.Normal,
                        Type = DuelType.Volley, Opponents = 3,
                        Look = new CowboyLook
                        {
                            Shirt = new Color(0.72f, 0.58f, 0.38f),
                            Accent = new Color(0.55f, 0.22f, 0.16f),
                            CharacterId = "native_archer",
                        },
                        Intro = new[]
                        {
                            new DialogLine(Speaker.Opponent, "Should've watched the ridgeline."),
                            new DialogLine(Speaker.You,      "Three of you. Still not enough."),
                            new DialogLine(Speaker.Opponent, "Then drop every one of us. If your hand can."),
                        },
                    },
                },
            },
            new ChapterDef
            {
                Title = "Dust & Bounty",
                Tagline = "There's a price on your head — and hungry men to collect it.",
                Theme = new Color(0.66f, 0.50f, 0.30f),
                Stages = new[]
                {
                    new StageDef
                    {
                        Title = "The Diva", Arena = "Painted Hills", Difficulty = Difficulty.Normal,
                        Look = new CowboyLook
                        {
                            Shirt = new Color(0.62f, 0.12f, 0.16f),
                            Accent = new Color(0.90f, 0.72f, 0.28f),
                            CharacterId = "cabaret_singer_4",
                        },
                        Intro = new[]
                        {
                            new DialogLine(Speaker.Opponent, "This town already has a star. You ain't it."),
                            new DialogLine(Speaker.You,      "Then take a bow."),
                            new DialogLine(Speaker.Opponent, "Jack paid for the ink. I just deliver it."),
                        },
                    },
                    new StageDef
                    {
                        Title = "The Headliner", Arena = "Green Valley", Difficulty = Difficulty.Normal,
                        Type = DuelType.Sync, Opponents = 4, Hp = 3, Strike = 1,
                        Look = new CowboyLook
                        {
                            Shirt = new Color(0.72f, 0.22f, 0.18f),
                            Accent = new Color(0.90f, 0.70f, 0.28f),
                            CharacterId = "cabaret_singer_3",
                        },
                        Intro = new[]
                        {
                            new DialogLine(Speaker.Opponent, "Four of us. And you've only got two hands."),
                            new DialogLine(Speaker.You,      "Each hand's good for two."),
                            new DialogLine(Speaker.Opponent, "The stays are steel. Don't let either hand slip."),
                        },
                    },
                    new StageDef
                    {
                        Title = "The Showgirl", Arena = "Salt Flats", Difficulty = Difficulty.Normal,
                        Look = new CowboyLook
                        {
                            Shirt = new Color(0.55f, 0.14f, 0.16f),
                            Accent = new Color(0.90f, 0.70f, 0.28f),
                            CharacterId = "cabaret_singer_2",
                        },
                        Intro = new[]
                        {
                            new DialogLine(Speaker.Opponent, "I saw your partner at the crossroads. Jack didn't use a bar."),
                            new DialogLine(Speaker.You,      "Then I'll walk there myself."),
                            new DialogLine(Speaker.Opponent, "He waits for the bang. You'll hear it."),
                        },
                    },
                },
            },
            new ChapterDef
            {
                Title = "Blood & Silver",
                Tagline = "The hills pay the show. The mine is called Salazar.",
                Theme = new Color(0.55f, 0.30f, 0.26f),
                Stages = new[]
                {
                    new StageDef
                    {
                        Title = "The Chanteuse", Arena = "Red Canyon", Difficulty = Difficulty.Normal,
                        Look = new CowboyLook
                        {
                            Shirt = new Color(0.55f, 0.10f, 0.14f),
                            Accent = new Color(0.90f, 0.70f, 0.28f),
                            CharacterId = "cabaret_singer_5",
                        },
                        Intro = new[]
                        {
                            new DialogLine(Speaker.Opponent, "This canyon keeps every encore. Even the last."),
                            new DialogLine(Speaker.You,      "Then I won't sing."),
                            new DialogLine(Speaker.Opponent, "Draw. The rocks will do the chorus."),
                        },
                    },
                    new StageDef
                    {
                        Title = "The Payroll", Arena = "Ghost Town", Difficulty = Difficulty.Hard,
                        Type = DuelType.Volley, Opponents = 2, Hp = 3, Strike = 1,
                        Look = new CowboyLook
                        {
                            Shirt = new Color(0.62f, 0.12f, 0.16f),
                            Accent = new Color(0.90f, 0.72f, 0.28f),
                            CharacterId = "cabaret_singer",
                        },
                        Intro = new[]
                        {
                            new DialogLine(Speaker.Opponent, "Salazar silver. It dresses the whole company."),
                            new DialogLine(Speaker.You,      "Then the mine can bury them."),
                            new DialogLine(Speaker.Opponent, "Two of us. Steel under the coats. Take the windows."),
                        },
                    },
                    new StageDef
                    {
                        Title = "The Kid", Arena = "Midnight Mesa", Difficulty = Difficulty.Hard,
                        Look = new CowboyLook
                        {
                            Shirt = new Color(0.28f, 0.14f, 0.14f),
                            Accent = new Color(0.82f, 0.62f, 0.28f),
                            CharacterId = "lady_1",
                        },
                        Intro = new[]
                        {
                            new DialogLine(Speaker.Opponent, "Fastest hand west of the river. That's me."),
                            new DialogLine(Speaker.You,      "You talk faster than you draw."),
                            new DialogLine(Speaker.Opponent, "Jack won't give you a bar. He waits for the bang. So will I, when he's done with you."),
                        },
                    },
                },
            },
            new ChapterDef
            {
                Title = "Judgement Day",
                Tagline = "The dead don't forgive. Neither does the law.",
                Theme = new Color(0.40f, 0.42f, 0.40f),
                Stages = new[]
                {
                    new StageDef
                    {
                        Title = "The Dancer", Arena = "Boot Hill", Difficulty = Difficulty.Hard,
                        Type = DuelType.Volley, Opponents = 3,
                        Look = new CowboyLook
                        {
                            Shirt = new Color(0.16f, 0.22f, 0.55f),
                            Accent = new Color(0.90f, 0.72f, 0.28f),
                            CharacterId = "cabaret_dancer_6",
                        },
                        Intro = new[]
                        {
                            new DialogLine(Speaker.Opponent, "I danced at the funeral. Jack paid for the floor."),
                            new DialogLine(Speaker.You,      "Then you can bury him next."),
                            new DialogLine(Speaker.Opponent, "Three of us. Fall in time, or don't fall at all."),
                        },
                    },
                    new StageDef
                    {
                        Title = "The Chorus Girl", Arena = "Salt Flats", Difficulty = Difficulty.Hard,
                        Type = DuelType.Sync, Opponents = 2, Strike = 2,
                        Look = new CowboyLook
                        {
                            Shirt = new Color(0.22f, 0.32f, 0.72f),
                            Accent = new Color(0.70f, 0.82f, 0.95f),
                            CharacterId = "cabaret_dancer_7",
                        },
                        Intro = new[]
                        {
                            new DialogLine(Speaker.Opponent, "The warrant's signed. Miss once and it costs you twice."),
                            new DialogLine(Speaker.You,      "I didn't come for a show."),
                            new DialogLine(Speaker.Opponent, "The Prima's vest stopped a rifle. You'll meet it before you meet him."),
                        },
                    },
                },
            },
            new ChapterDef
            {
                Title = "High Noon",
                Tagline = "One street. One bullet. It ends at noon.",
                Theme = new Color(0.34f, 0.26f, 0.30f),
                Stages = new[]
                {
                    new StageDef
                    {
                        Title = "The Prima", Arena = "Gallows Hill", Difficulty = Difficulty.Hard,
                        Type = DuelType.Volley, Opponents = 2, Hp = 6, Strike = 1,
                        Look = new CowboyLook
                        {
                            Shirt = new Color(0.18f, 0.28f, 0.62f),
                            Accent = new Color(0.55f, 0.78f, 0.95f),
                            CharacterId = "cabaret_dancer_8",
                        },
                        Intro = new[]
                        {
                            new DialogLine(Speaker.Opponent, "I've buried better men than you. And this vest has stopped worse."),
                            new DialogLine(Speaker.You,      "Then I'll keep shootin' till it doesn't."),
                            new DialogLine(Speaker.Opponent, "Two of us. The gallows can wait."),
                        },
                    },
                    new StageDef
                    {
                        // Final stage of the final chapter → the BOSS. Type is data, not "because it is last".
                        Title = "Black Jack", Arena = "Devil's Crossroads", Difficulty = Difficulty.Hard,
                        Type = DuelType.Reaction,
                        Look = new CowboyLook { Shirt = new Color(0.12f, 0.11f, 0.13f), HatColor = new Color(0.06f, 0.06f, 0.07f), Skin = new Color(0.82f, 0.78f, 0.72f), Accent = new Color(0.70f, 0.10f, 0.10f), HatType = HatStyle.Wide, Chest = Accessory.Poncho, Facial = FacialHair.Beard },
                        Intro = new[]
                        {
                            new DialogLine(Speaker.Opponent, "So. You're the one who won't stay dead."),
                            new DialogLine(Speaker.You,      "And you're the last name on my list."),
                            new DialogLine(Speaker.Opponent, "Then draw, and let's see whose sun sets."),
                        },
                    },
                },
            },
        };

        /// <summary>Play order. Index 0 is always open; each later road unlocks when the one before it is cleared.</summary>
        public static readonly CampaignDef[] All =
        {
            new CampaignDef
            {
                Id = "playbill",
                Title = "The Playbill",
                Blurb = "One list. The last name is Black Jack.",
                Victory = "The bill is done.  The crossroads keeps the name you came for.",
                Defeat = "Three hearts.  The list keeps the last name.",
                Chapters = FrontierChapters,
            },
            CampaignRoster.SaltDebt(),
            CampaignRoster.BootHillNight(),
            CampaignRoster.TheWire(),
            CampaignRoster.TheFlood(),
            CampaignRoster.WhiteSeason(),
            CampaignRoster.SanIsidro(),
            CampaignRoster.SundayHorses(),
            CampaignRoster.TheCircuit(),
        };

        public static int Index { get; private set; }

        public static CampaignDef Current => All[Mathf.Clamp(Index, 0, All.Length - 1)];

        /// <summary>Chapters of the road in progress. Call <see cref="Select"/> before starting a different road.</summary>
        public static ChapterDef[] Chapters => Current.Chapters;

        public const int StartingLives = 3;

        /// <summary>
        /// Debug pick. Every road opens, and the map can start any node in any chapter.
        /// Turn off to restore the clear-the-previous-road chain.
        /// </summary>
        public const bool FreePick = true;

        public static bool Active;
        public static int Chapter;
        public static int Stage;
        public static int Lives;

        /// <summary>Set when a node is tapped on the map; the next duel plays its intro banter (cleared on RETRY).</summary>
        public static bool ShowIntro;

        // ---- persistence (PlayerPrefs) ----
        const string KActive = "hn_cmp_active";
        const string KId = "hn_cmp_id";
        const string KCleared = "hn_cmp_cleared";
        const string KChapter = "hn_cmp_chapter";
        const string KStage = "hn_cmp_stage";
        const string KLives = "hn_cmp_lives";

        /// <summary>True when a resumable in-progress run is saved (populated by <see cref="LoadSavedRun"/>).</summary>
        public static bool HasSavedRun { get; private set; }

        public static void Select(int index)
        {
            Index = Mathf.Clamp(index, 0, All.Length - 1);
        }

        public static bool IsCleared(string id)
        {
            if (string.IsNullOrEmpty(id)) return false;
            var raw = PlayerPrefs.GetString(KCleared, "");
            if (string.IsNullOrEmpty(raw)) return false;
            var parts = raw.Split(',');
            for (int i = 0; i < parts.Length; i++)
                if (parts[i] == id) return true;
            return false;
        }

        /// <summary>The first road is open. Each later road opens when the previous one has been cleared.</summary>
        public static bool IsUnlocked(int index)
        {
            if (FreePick) return index >= 0 && index < All.Length;
            if (index <= 0) return true;
            if (index >= All.Length) return false;
            return IsCleared(All[index - 1].Id);
        }

        public static string UnlockHint(int index)
        {
            if (index <= 0 || index >= All.Length) return "";
            return "Finish " + All[index - 1].Title + " first.";
        }

        public static bool IsSavedRun(int index)
        {
            if (!HasSavedRun || index < 0 || index >= All.Length) return false;
            return All[index].Id == PlayerPrefs.GetString(KId, All[0].Id);
        }

        public static void Save()
        {
            PlayerPrefs.SetInt(KActive, Active ? 1 : 0);
            PlayerPrefs.SetString(KId, Current.Id);
            PlayerPrefs.SetInt(KChapter, Chapter);
            PlayerPrefs.SetInt(KStage, Stage);
            PlayerPrefs.SetInt(KLives, Lives);
            SaveData.Save();
            HasSavedRun = Active;
        }

        static void MarkCleared(string id)
        {
            if (IsCleared(id)) return;
            var raw = PlayerPrefs.GetString(KCleared, "");
            PlayerPrefs.SetString(KCleared, string.IsNullOrEmpty(raw) ? id : raw + "," + id);
        }

        static int IndexOf(string id)
        {
            for (int i = 0; i < All.Length; i++)
                if (All[i].Id == id) return i;
            return -1;
        }

        /// <summary>Load the saved run into the static state (call once at app/menu start).</summary>
        public static void LoadSavedRun()
        {
            ShowIntro = false;
            Active = PlayerPrefs.GetInt(KActive, 0) == 1;
            if (!Active)
            {
                Index = 0;
                Chapter = 0;
                Stage = 0;
                Lives = StartingLives;
                HasSavedRun = false;
                return;
            }

            int found = IndexOf(PlayerPrefs.GetString(KId, All[0].Id));
            if (found < 0)
            {
                Active = false;
                Index = 0;
                Chapter = 0;
                Stage = 0;
                Lives = StartingLives;
                HasSavedRun = false;
                Save();
                return;
            }

            Index = found;
            Chapter = Mathf.Clamp(PlayerPrefs.GetInt(KChapter, 0), 0, Chapters.Length - 1);
            Stage = Mathf.Clamp(PlayerPrefs.GetInt(KStage, 0), 0, CurrentChapter.Stages.Length - 1);
            Lives = PlayerPrefs.GetInt(KLives, StartingLives);
            if (Lives <= 0)
            {
                // Crash between LoseLife and EndRun — do not resurrect a free life.
                EndRun(false);
                Chapter = 0;
                Stage = 0;
                Lives = StartingLives;
                return;
            }
            HasSavedRun = true;
        }

        public static ChapterDef CurrentChapter => Chapters[Mathf.Clamp(Chapter, 0, Chapters.Length - 1)];
        public static StageDef CurrentStage => CurrentChapter.Stages[Mathf.Clamp(Stage, 0, CurrentChapter.Stages.Length - 1)];

        public static bool IsLastStageOfChapter => Stage >= CurrentChapter.Stages.Length - 1;
        public static bool IsLastChapter => Chapter >= Chapters.Length - 1;
        public static bool IsFinalStage => IsLastChapter && IsLastStageOfChapter;

        public static void StartRun()
        {
            Active = true;
            Chapter = 0;
            Stage = 0;
            Lives = StartingLives;
            ShowIntro = false;
            Save();
        }

        /// <summary>Advance one node; wraps to the next chapter's first node. Not for the final stage.</summary>
        public static void AdvanceStage()
        {
            Stage++;
            if (Stage >= CurrentChapter.Stages.Length)
            {
                Chapter++;
                Stage = 0;
            }
            Records.ReportProgress(Chapter, Stage);
            Save();
        }

        /// <summary>Lose one shared team life and persist.</summary>
        public static void LoseLife()
        {
            Lives--;
            Save();
        }

        /// <summary>End the run (victory or defeat): clears the resumable save; a win counts a completion.</summary>
        public static void EndRun(bool victory)
        {
            string id = Current.Id;
            Active = false;
            if (victory)
            {
                MarkCleared(id);
                Records.ReportCompletion();
            }
            Save();
        }

        /// <summary>Pins the current stage's arena + difficulty into MatchSettings for the duel.</summary>
        public static void ApplyToMatch()
        {
            MatchSettings.Mode = GameMode.PvE;
            MatchSettings.ForcedArena = CurrentStage.Arena;
            MatchSettings.BotDifficulty = CurrentStage.Difficulty;
            MatchSettings.Type = CurrentStage.Type;
        }
    }
}
