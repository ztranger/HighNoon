using UnityEngine;

namespace HighNoon
{
    /// <summary>
    /// Short roads after <c>playbill</c>. They exist so the campaign list and the unlock chain
    /// can be played through without clearing thirteen nodes. Each is one chapter, two stages.
    /// </summary>
    public static class CampaignRoster
    {
        public static CampaignDef SaltDebt()
        {
            return new CampaignDef
            {
                Id = "salt_debt",
                Title = "Salt Debt",
                Blurb = "A warrant left over from the flats.",
                Victory = "The warrant is paid.  The hill is still open.",
                Defeat = "The flats keep the debt.",
                Chapters = new[]
                {
                    new ChapterDef
                    {
                        Title = "The Flats",
                        Tagline = "A name on a warrant, and a man who collects.",
                        Theme = new Color(0.72f, 0.70f, 0.58f),
                        Stages = new[]
                        {
                            new StageDef
                            {
                                Title = "The Clerk", Arena = "Salt Flats", Difficulty = Difficulty.Easy,
                                Look = new CowboyLook
                                {
                                    Shirt = new Color(0.22f, 0.32f, 0.72f),
                                    Accent = new Color(0.70f, 0.82f, 0.95f),
                                    CharacterId = "cabaret_dancer_7",
                                },
                                Intro = new[]
                                {
                                    new DialogLine(Speaker.Opponent, "The warrant's still warm."),
                                    new DialogLine(Speaker.You,      "Then read it and draw."),
                                },
                            },
                            new StageDef
                            {
                                Title = "The Collector", Arena = "Salt Flats", Difficulty = Difficulty.Normal,
                                Type = DuelType.Reaction,
                                Look = new CowboyLook
                                {
                                    Shirt = new Color(0.28f, 0.26f, 0.22f),
                                    HatColor = new Color(0.16f, 0.14f, 0.12f),
                                    Accent = new Color(0.55f, 0.48f, 0.32f),
                                    HatType = HatStyle.Wide,
                                    Chest = Accessory.Vest,
                                    Facial = FacialHair.Mustache,
                                },
                                Intro = new[]
                                {
                                    new DialogLine(Speaker.Opponent, "Salt doesn't bargain."),
                                    new DialogLine(Speaker.You,      "Neither do I."),
                                    new DialogLine(Speaker.Opponent, "Then draw. The flats are watching."),
                                },
                            },
                        },
                    },
                },
            };
        }

        public static CampaignDef BootHillNight()
        {
            return new CampaignDef
            {
                Id = "boot_hill",
                Title = "Boot Hill Night",
                Blurb = "What the flats bury, the hill keeps.",
                Victory = "The hill gives the name back.",
                Defeat = "Boot Hill keeps another pair of boots.",
                Chapters = new[]
                {
                    new ChapterDef
                    {
                        Title = "The Hill",
                        Tagline = "The dead still take visitors after dark.",
                        Theme = new Color(0.32f, 0.34f, 0.38f),
                        Stages = new[]
                        {
                            new StageDef
                            {
                                Title = "The Mourner", Arena = "Boot Hill", Difficulty = Difficulty.Easy,
                                Look = new CowboyLook
                                {
                                    Shirt = new Color(0.16f, 0.22f, 0.55f),
                                    Accent = new Color(0.90f, 0.72f, 0.28f),
                                    CharacterId = "cabaret_dancer_6",
                                },
                                Intro = new[]
                                {
                                    new DialogLine(Speaker.Opponent, "You paid the flats. The hill still wants a word."),
                                    new DialogLine(Speaker.You,      "Then speak."),
                                },
                            },
                            new StageDef
                            {
                                Title = "The Gravedigger", Arena = "Boot Hill", Difficulty = Difficulty.Normal,
                                Type = DuelType.Volley, Opponents = 2,
                                Look = new CowboyLook
                                {
                                    Shirt = new Color(0.24f, 0.22f, 0.20f),
                                    HatColor = new Color(0.10f, 0.10f, 0.10f),
                                    Accent = new Color(0.45f, 0.38f, 0.28f),
                                    HatType = HatStyle.Wide,
                                    Chest = Accessory.Poncho,
                                    Facial = FacialHair.Beard,
                                },
                                Intro = new[]
                                {
                                    new DialogLine(Speaker.Opponent, "Two of us. One hole."),
                                    new DialogLine(Speaker.You,      "Dig it for yourselves."),
                                },
                            },
                        },
                    },
                },
            };
        }
    }
}
