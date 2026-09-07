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

    /// <summary>One PvE encounter (a node on a chapter map): arena + opponent + difficulty + intro banter.</summary>
    public class StageDef
    {
        public string Title;
        public string Arena;
        public Difficulty Difficulty;
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
                        Intro = new[]
                        {
                            new DialogLine(Speaker.Opponent, "Long way from anywhere, stranger."),
                            new DialogLine(Speaker.You,      "Just passin' through."),
                            new DialogLine(Speaker.Opponent, "Folks who pass through end up buried here."),
                        },
                    },
                    new StageDef
                    {
                        Title = "Town Trouble", Arena = "Dusty Town", Difficulty = Difficulty.Easy,
                        Intro = new[]
                        {
                            new DialogLine(Speaker.Opponent, "This here's my street. Turn around."),
                            new DialogLine(Speaker.You,      "I don't turn around."),
                            new DialogLine(Speaker.Opponent, "Then reach for it."),
                        },
                    },
                    new StageDef
                    {
                        Title = "Canyon Ambush", Arena = "Red Canyon", Difficulty = Difficulty.Normal,
                        Intro = new[]
                        {
                            new DialogLine(Speaker.Opponent, "Should've watched the ridgeline, amigo."),
                            new DialogLine(Speaker.You,      "Should've picked a bigger gang."),
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
                        Title = "The Undertaker", Arena = "Boot Hill", Difficulty = Difficulty.Normal,
                        Intro = new[]
                        {
                            new DialogLine(Speaker.Opponent, "I already dug your grave."),
                            new DialogLine(Speaker.You,      "Then you wasted a good mornin'."),
                        },
                    },
                    new StageDef
                    {
                        Title = "The Sheriff", Arena = "Salt Flats", Difficulty = Difficulty.Hard,
                        Intro = new[]
                        {
                            new DialogLine(Speaker.Opponent, "End of the line. I'm the law out here."),
                            new DialogLine(Speaker.You,      "The law never scared me."),
                            new DialogLine(Speaker.Opponent, "It'll be the last thing you feel."),
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
        }

        /// <summary>Pins the current stage's arena + difficulty into MatchSettings for the duel.</summary>
        public static void ApplyToMatch()
        {
            MatchSettings.Mode = GameMode.PvE;
            MatchSettings.ForcedArena = CurrentStage.Arena;
            MatchSettings.BotDifficulty = CurrentStage.Difficulty;
        }
    }
}
