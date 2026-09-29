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

    /// <summary>
    /// PvE campaign, split into chapters — each chapter is its own map of stage nodes.
    /// Static so run state survives scene changes (Map ↔ Duel). Progress within a
    /// chapter is <see cref="Stage"/> (nodes below it are cleared, it is the current
    /// node, nodes above are locked). Clearing the last node advances to the next chapter.
    /// </summary>
    public static class Campaign
    {
        public static readonly ChapterDef[] Chapters =
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
                            new DialogLine(Speaker.Opponent, "Then don't let either one slip."),
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
                            new DialogLine(Speaker.Opponent, "Never missed a shot in my life."),
                            new DialogLine(Speaker.You,      "First time for everything."),
                        },
                    },
                },
            },
            new ChapterDef
            {
                Title = "Blood & Silver",
                Tagline = "The Salazar gang owns these hills. Time to thin them out.",
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
                        Title = "The Cabaret Singer", Arena = "Ghost Town", Difficulty = Difficulty.Hard,
                        Look = new CowboyLook
                        {
                            Shirt = new Color(0.62f, 0.12f, 0.16f),
                            Accent = new Color(0.90f, 0.72f, 0.28f),
                            CharacterId = "cabaret_singer",
                        },
                        Intro = new[]
                        {
                            new DialogLine(Speaker.Opponent, "Last show in this town. You're the encore."),
                            new DialogLine(Speaker.You,      "I don't clap."),
                            new DialogLine(Speaker.Opponent, "Then you'll take a bow."),
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
                        Look = new CowboyLook
                        {
                            Shirt = new Color(0.16f, 0.22f, 0.55f),
                            Accent = new Color(0.90f, 0.72f, 0.28f),
                            CharacterId = "cabaret_dancer_6",
                        },
                        Intro = new[]
                        {
                            new DialogLine(Speaker.Opponent, "Last waltz on Boot Hill. You're my partner."),
                            new DialogLine(Speaker.You,      "I don't dance."),
                            new DialogLine(Speaker.Opponent, "Then fall in time."),
                        },
                    },
                    new StageDef
                    {
                        Title = "The Chorus Girl", Arena = "Salt Flats", Difficulty = Difficulty.Hard,
                        Look = new CowboyLook
                        {
                            Shirt = new Color(0.22f, 0.32f, 0.72f),
                            Accent = new Color(0.70f, 0.82f, 0.95f),
                            CharacterId = "cabaret_dancer_7",
                        },
                        Intro = new[]
                        {
                            new DialogLine(Speaker.Opponent, "Salt and spotlight. Same glare."),
                            new DialogLine(Speaker.You,      "I didn't come for a show."),
                            new DialogLine(Speaker.Opponent, "Too late. You're on."),
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

        public const int StartingLives = 3;

        public static bool Active;
        public static int Chapter;
        public static int Stage;
        public static int Lives;

        /// <summary>Set when a node is tapped on the map; the next duel plays its intro banter (cleared on RETRY).</summary>
        public static bool ShowIntro;

        // ---- persistence (PlayerPrefs) ----
        const string KActive = "hn_cmp_active";
        const string KChapter = "hn_cmp_chapter";
        const string KStage = "hn_cmp_stage";
        const string KLives = "hn_cmp_lives";

        /// <summary>True when a resumable in-progress run is saved (populated by <see cref="LoadSavedRun"/>).</summary>
        public static bool HasSavedRun { get; private set; }

        public static void Save()
        {
            PlayerPrefs.SetInt(KActive, Active ? 1 : 0);
            PlayerPrefs.SetInt(KChapter, Chapter);
            PlayerPrefs.SetInt(KStage, Stage);
            PlayerPrefs.SetInt(KLives, Lives);
            SaveData.Save();
            HasSavedRun = Active;
        }

        /// <summary>Load the saved run into the static state (call once at app/menu start).</summary>
        public static void LoadSavedRun()
        {
            ShowIntro = false;
            Active = PlayerPrefs.GetInt(KActive, 0) == 1;
            if (!Active)
            {
                Chapter = 0;
                Stage = 0;
                Lives = StartingLives;
                HasSavedRun = false;
                return;
            }

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
            Active = false;
            if (victory) Records.ReportCompletion();
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
