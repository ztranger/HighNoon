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

        /// <summary>
        /// Telegraph road. Arenas are procedural (no painted Background). Looks have no
        /// CharacterId, so cowboys stay generated pixels. Final-art notes: docs/CAMPAIGN_ART.md.
        /// </summary>
        public static CampaignDef TheWire()
        {
            return new CampaignDef
            {
                Id = "the_wire",
                Title = "The Wire",
                Blurb = "The line went quiet. Ride it to Mile 90.",
                Victory = "The line hums.  Mile 90 answers.",
                Defeat = "Three hearts.  The wire stays cut.",
                Chapters = new[]
                {
                    new ChapterDef
                    {
                        Title = "The Railhead",
                        Tagline = "The line hummed this morning. It doesn't now.",
                        Theme = new Color(0.72f, 0.55f, 0.32f),
                        Stages = new[]
                        {
                            new StageDef
                            {
                                Title = "The Tapper", Arena = "Railhead", Difficulty = Difficulty.Easy,
                                Look = new CowboyLook
                                {
                                    Shirt = new Color(0.62f, 0.70f, 0.74f),
                                    HatColor = new Color(0.28f, 0.22f, 0.16f),
                                    Skin = new Color(0.90f, 0.74f, 0.56f),
                                    Accent = new Color(0.72f, 0.55f, 0.28f),
                                    HatType = HatStyle.Bowler,
                                    Chest = Accessory.None,
                                    Facial = FacialHair.None,
                                },
                                Intro = new[]
                                {
                                    new DialogLine(Speaker.Opponent, "It sang all morning. Then it didn't."),
                                    new DialogLine(Speaker.You,      "A line doesn't just quit."),
                                    new DialogLine(Speaker.Opponent, "Then tap it true. The green is a clean dot."),
                                },
                            },
                            new StageDef
                            {
                                Title = "The Night Desk", Arena = "Dry Creek", Difficulty = Difficulty.Easy,
                                Type = DuelType.Sync, Opponents = 2,
                                Look = new CowboyLook
                                {
                                    Shirt = new Color(0.18f, 0.28f, 0.26f),
                                    HatColor = new Color(0.10f, 0.10f, 0.12f),
                                    Skin = new Color(0.72f, 0.52f, 0.38f),
                                    Accent = new Color(0.85f, 0.62f, 0.22f),
                                    HatType = HatStyle.Bowler,
                                    Chest = Accessory.Vest,
                                    Facial = FacialHair.Mustache,
                                },
                                Intro = new[]
                                {
                                    new DialogLine(Speaker.Opponent, "Two keys. Two words. Don't cross them."),
                                    new DialogLine(Speaker.You,      "I didn't come to send a greeting."),
                                    new DialogLine(Speaker.Opponent, "Then don't miss. The spark jumps back."),
                                },
                            },
                            new StageDef
                            {
                                Title = "The First Cut", Arena = "Pole 12", Difficulty = Difficulty.Normal,
                                Type = DuelType.Volley, Opponents = 3,
                                Look = new CowboyLook
                                {
                                    Shirt = new Color(0.48f, 0.36f, 0.24f),
                                    HatColor = new Color(0.22f, 0.16f, 0.12f),
                                    Skin = new Color(0.62f, 0.44f, 0.32f),
                                    Accent = new Color(0.72f, 0.42f, 0.22f),
                                    HatType = HatStyle.Wide,
                                    Chest = Accessory.Bandana,
                                    Facial = FacialHair.Beard,
                                },
                                Intro = new[]
                                {
                                    new DialogLine(Speaker.Opponent, "Pole twelve. We started at the easy end."),
                                    new DialogLine(Speaker.You,      "You'll finish on the ground."),
                                    new DialogLine(Speaker.Opponent, "Three of us. One pole at a time. If your hand can."),
                                },
                            },
                        },
                    },
                    new ChapterDef
                    {
                        Title = "The Empty Stretch",
                        Tagline = "Forty miles of wire and nobody on it.",
                        Theme = new Color(0.70f, 0.66f, 0.52f),
                        Stages = new[]
                        {
                            new StageDef
                            {
                                Title = "The Relief", Arena = "Alkali Tank", Difficulty = Difficulty.Normal,
                                Look = new CowboyLook
                                {
                                    Shirt = new Color(0.78f, 0.68f, 0.42f),
                                    HatColor = new Color(0.36f, 0.26f, 0.16f),
                                    Skin = new Color(0.84f, 0.64f, 0.46f),
                                    Accent = new Color(0.55f, 0.32f, 0.18f),
                                    HatType = HatStyle.Cowboy,
                                    Chest = Accessory.Vest,
                                    Facial = FacialHair.Mustache,
                                },
                                Intro = new[]
                                {
                                    new DialogLine(Speaker.Opponent, "Coils are worth more cut than strung."),
                                    new DialogLine(Speaker.You,      "Not to the man at the far end."),
                                    new DialogLine(Speaker.Opponent, "Tap. He isn't listening anyway."),
                                },
                            },
                            new StageDef
                            {
                                Title = "The Splicers", Arena = "The Cut", Difficulty = Difficulty.Normal,
                                Type = DuelType.Sync, Opponents = 4, Hp = 3, Strike = 1,
                                Look = new CowboyLook
                                {
                                    Shirt = new Color(0.16f, 0.22f, 0.18f),
                                    HatColor = new Color(0.08f, 0.10f, 0.08f),
                                    Skin = new Color(0.55f, 0.40f, 0.30f),
                                    Accent = new Color(0.62f, 0.55f, 0.28f),
                                    HatType = HatStyle.Wide,
                                    Chest = Accessory.Poncho,
                                    Facial = FacialHair.Mustache,
                                },
                                Intro = new[]
                                {
                                    new DialogLine(Speaker.Opponent, "Four of us. Two hands. The coats are lined."),
                                    new DialogLine(Speaker.You,      "Each hand's good for two."),
                                    new DialogLine(Speaker.Opponent, "Steel under the rubber. Don't slip."),
                                },
                            },
                            new StageDef
                            {
                                Title = "The Listener", Arena = "Mile 70", Difficulty = Difficulty.Normal,
                                Look = new CowboyLook
                                {
                                    Shirt = new Color(0.55f, 0.42f, 0.48f),
                                    HatColor = new Color(0.30f, 0.22f, 0.24f),
                                    Skin = new Color(0.86f, 0.68f, 0.52f),
                                    Accent = new Color(0.40f, 0.55f, 0.48f),
                                    HatType = HatStyle.Cowboy,
                                    Chest = Accessory.Bandana,
                                    Facial = FacialHair.None,
                                },
                                Intro = new[]
                                {
                                    new DialogLine(Speaker.Opponent, "I heard them cut it. West, then further west."),
                                    new DialogLine(Speaker.You,      "Who's at the last station?"),
                                    new DialogLine(Speaker.Opponent, "The chief. He doesn't use a key. He waits for the click."),
                                },
                            },
                        },
                    },
                    new ChapterDef
                    {
                        Title = "Mile 90",
                        Tagline = "The last station answers once.",
                        Theme = new Color(0.22f, 0.26f, 0.36f),
                        Stages = new[]
                        {
                            new StageDef
                            {
                                Title = "The Last Poles", Arena = "Mile 90 Yard", Difficulty = Difficulty.Hard,
                                Type = DuelType.Volley, Opponents = 2, Hp = 3, Strike = 1,
                                Look = new CowboyLook
                                {
                                    Shirt = new Color(0.22f, 0.24f, 0.28f),
                                    HatColor = new Color(0.08f, 0.08f, 0.10f),
                                    Skin = new Color(0.50f, 0.38f, 0.30f),
                                    Accent = new Color(0.55f, 0.58f, 0.48f),
                                    HatType = HatStyle.Wide,
                                    Chest = Accessory.Poncho,
                                    Facial = FacialHair.Beard,
                                },
                                Intro = new[]
                                {
                                    new DialogLine(Speaker.Opponent, "These two still sing. That's the point."),
                                    new DialogLine(Speaker.You,      "Then I'll cut the singers."),
                                    new DialogLine(Speaker.Opponent, "Both guns. One pole, then the other. Steel under the coats."),
                                },
                            },
                            new StageDef
                            {
                                Title = "The Ground", Arena = "Mile 90 Shed", Difficulty = Difficulty.Hard,
                                Type = DuelType.Sync, Opponents = 2, Strike = 2,
                                Look = new CowboyLook
                                {
                                    Shirt = new Color(0.20f, 0.26f, 0.22f),
                                    HatColor = new Color(0.12f, 0.14f, 0.12f),
                                    Skin = new Color(0.66f, 0.48f, 0.36f),
                                    Accent = new Color(0.72f, 0.88f, 0.35f),
                                    HatType = HatStyle.Cowboy,
                                    Chest = Accessory.Badge,
                                    Facial = FacialHair.None,
                                },
                                Intro = new[]
                                {
                                    new DialogLine(Speaker.Opponent, "Hold the ground or it holds you."),
                                    new DialogLine(Speaker.You,      "I didn't ride ninety miles to spark."),
                                    new DialogLine(Speaker.Opponent, "Miss once. It costs you twice. Then you meet him."),
                                },
                            },
                            new StageDef
                            {
                                Title = "The Chief", Arena = "Mile 90", Difficulty = Difficulty.Hard,
                                Type = DuelType.Reaction,
                                Look = new CowboyLook
                                {
                                    Shirt = new Color(0.14f, 0.14f, 0.16f),
                                    HatColor = new Color(0.06f, 0.06f, 0.07f),
                                    Skin = new Color(0.78f, 0.66f, 0.54f),
                                    Accent = new Color(0.78f, 0.62f, 0.28f),
                                    HatType = HatStyle.Bowler,
                                    Chest = Accessory.Vest,
                                    Facial = FacialHair.Beard,
                                },
                                Intro = new[]
                                {
                                    new DialogLine(Speaker.Opponent, "The line is quiet because I said so."),
                                    new DialogLine(Speaker.You,      "Then hear this."),
                                    new DialogLine(Speaker.Opponent, "No key. Draw on the click."),
                                },
                            },
                        },
                    },
                },
            };
        }

        /// <summary>
        /// Lumen drowning street by street. Arenas are procedural. Looks have no CharacterId.
        /// Final-art notes: docs/CAMPAIGN_ART.md.
        /// </summary>
        public static CampaignDef TheFlood()
        {
            return new CampaignDef
            {
                Id = "the_flood",
                Title = "The Flood",
                Blurb = "Lumen is drowning. Climb to the roof.",
                Victory = "The roof holds.  Pell doesn't.",
                Defeat = "Three hearts.  Lumen keeps the rest.",
                Chapters = new[]
                {
                    new ChapterDef
                    {
                        Title = "Ankles",
                        Tagline = "The market is still a market. It won't be by noon.",
                        Theme = new Color(0.55f, 0.48f, 0.38f),
                        Stages = new[]
                        {
                            new StageDef
                            {
                                Title = "The Grocer", Arena = "Lower Market", Difficulty = Difficulty.Easy,
                                Look = new CowboyLook
                                {
                                    Shirt = new Color(0.86f, 0.82f, 0.72f),
                                    HatColor = new Color(0.32f, 0.24f, 0.16f),
                                    Skin = new Color(0.84f, 0.66f, 0.50f),
                                    Accent = new Color(0.55f, 0.40f, 0.24f),
                                    HatType = HatStyle.Bowler,
                                    Chest = Accessory.Vest,
                                    Facial = FacialHair.Mustache,
                                },
                                Intro = new[]
                                {
                                    new DialogLine(Speaker.Opponent, "Shop's closed. The river can wait outside."),
                                    new DialogLine(Speaker.You,      "The river isn't asking."),
                                    new DialogLine(Speaker.Opponent, "Then hit the green before it hits the step."),
                                },
                            },
                            new StageDef
                            {
                                Title = "The Sisters", Arena = "Cork Lane", Difficulty = Difficulty.Easy,
                                Type = DuelType.Sync, Opponents = 2,
                                Look = new CowboyLook
                                {
                                    Shirt = new Color(0.42f, 0.50f, 0.58f),
                                    HatColor = new Color(0.24f, 0.22f, 0.26f),
                                    Skin = new Color(0.86f, 0.70f, 0.56f),
                                    Accent = new Color(0.28f, 0.36f, 0.40f),
                                    HatType = HatStyle.Cowboy,
                                    Chest = Accessory.Bandana,
                                    Facial = FacialHair.None,
                                },
                                Intro = new[]
                                {
                                    new DialogLine(Speaker.Opponent, "One boat. Two of us. Don't swamp it."),
                                    new DialogLine(Speaker.You,      "I need the lane."),
                                    new DialogLine(Speaker.Opponent, "Then take your half. Miss, and we fire wet."),
                                },
                            },
                            new StageDef
                            {
                                Title = "The Stairs", Arena = "Tanner's", Difficulty = Difficulty.Normal,
                                Type = DuelType.Volley, Opponents = 3,
                                Look = new CowboyLook
                                {
                                    Shirt = new Color(0.40f, 0.28f, 0.20f),
                                    HatColor = new Color(0.16f, 0.12f, 0.10f),
                                    Skin = new Color(0.55f, 0.40f, 0.32f),
                                    Accent = new Color(0.36f, 0.22f, 0.14f),
                                    HatType = HatStyle.Wide,
                                    Chest = Accessory.Bandana,
                                    Facial = FacialHair.Beard,
                                },
                                Intro = new[]
                                {
                                    new DialogLine(Speaker.Opponent, "We started at the bottom. You're the top."),
                                    new DialogLine(Speaker.You,      "Then come up and fall down."),
                                    new DialogLine(Speaker.Opponent, "Three steps. One at a time. The water's behind us."),
                                },
                            },
                        },
                    },
                    new ChapterDef
                    {
                        Title = "Waist",
                        Tagline = "Chapel Street is a canal. The bank still thinks it's a bank.",
                        Theme = new Color(0.40f, 0.48f, 0.46f),
                        Stages = new[]
                        {
                            new StageDef
                            {
                                Title = "The Bellringer", Arena = "Chapel Street", Difficulty = Difficulty.Normal,
                                Look = new CowboyLook
                                {
                                    Shirt = new Color(0.16f, 0.16f, 0.18f),
                                    HatColor = new Color(0.10f, 0.10f, 0.12f),
                                    Skin = new Color(0.70f, 0.58f, 0.48f),
                                    Accent = new Color(0.42f, 0.30f, 0.20f),
                                    HatType = HatStyle.Cowboy,
                                    Chest = Accessory.Poncho,
                                    Facial = FacialHair.Mustache,
                                },
                                Intro = new[]
                                {
                                    new DialogLine(Speaker.Opponent, "The bell is for the living. You're late."),
                                    new DialogLine(Speaker.You,      "Then stop ringing and move."),
                                    new DialogLine(Speaker.Opponent, "Hit it clean. The water's at the pews."),
                                },
                            },
                            new StageDef
                            {
                                Title = "The Tellers", Arena = "Bank Steps", Difficulty = Difficulty.Normal,
                                Type = DuelType.Sync, Opponents = 4, Hp = 3, Strike = 1,
                                Look = new CowboyLook
                                {
                                    Shirt = new Color(0.28f, 0.32f, 0.38f),
                                    HatColor = new Color(0.12f, 0.12f, 0.14f),
                                    Skin = new Color(0.80f, 0.66f, 0.52f),
                                    Accent = new Color(0.72f, 0.62f, 0.32f),
                                    HatType = HatStyle.Bowler,
                                    Chest = Accessory.Vest,
                                    Facial = FacialHair.Mustache,
                                },
                                Intro = new[]
                                {
                                    new DialogLine(Speaker.Opponent, "Four of us. The belts are lined."),
                                    new DialogLine(Speaker.You,      "Each hand's good for two."),
                                    new DialogLine(Speaker.Opponent, "Steel under the wool. The notes stay dry."),
                                },
                            },
                            new StageDef
                            {
                                Title = "The Sexton", Arena = "Chapel Roof", Difficulty = Difficulty.Normal,
                                Look = new CowboyLook
                                {
                                    Shirt = new Color(0.48f, 0.46f, 0.42f),
                                    HatColor = new Color(0.28f, 0.26f, 0.24f),
                                    Skin = new Color(0.72f, 0.60f, 0.48f),
                                    Accent = new Color(0.36f, 0.32f, 0.26f),
                                    HatType = HatStyle.Wide,
                                    Chest = Accessory.None,
                                    Facial = FacialHair.Beard,
                                },
                                Intro = new[]
                                {
                                    new DialogLine(Speaker.Opponent, "I dug the dry graves. He's on the roof."),
                                    new DialogLine(Speaker.You,      "Pell."),
                                    new DialogLine(Speaker.Opponent, "He won't give you a bar. The bars are under the river."),
                                },
                            },
                        },
                    },
                    new ChapterDef
                    {
                        Title = "The Roof",
                        Tagline = "One roof left. The rest is river.",
                        Theme = new Color(0.55f, 0.58f, 0.60f),
                        Stages = new[]
                        {
                            new StageDef
                            {
                                Title = "The Current", Arena = "Flood Street", Difficulty = Difficulty.Hard,
                                Type = DuelType.Sync, Opponents = 2, Strike = 2,
                                Look = new CowboyLook
                                {
                                    Shirt = new Color(0.55f, 0.60f, 0.62f),
                                    HatColor = new Color(0.18f, 0.20f, 0.22f),
                                    Skin = new Color(0.62f, 0.50f, 0.42f),
                                    Accent = new Color(0.30f, 0.42f, 0.48f),
                                    HatType = HatStyle.Cowboy,
                                    Chest = Accessory.None,
                                    Facial = FacialHair.None,
                                },
                                Intro = new[]
                                {
                                    new DialogLine(Speaker.Opponent, "Miss in this current and it takes two."),
                                    new DialogLine(Speaker.You,      "I didn't climb this far to swim."),
                                    new DialogLine(Speaker.Opponent, "Left and right. Then the boats. Then him."),
                                },
                            },
                            new StageDef
                            {
                                Title = "The Last Boats", Arena = "Courthouse Steps", Difficulty = Difficulty.Hard,
                                Type = DuelType.Volley, Opponents = 2, Hp = 6, Strike = 1,
                                Look = new CowboyLook
                                {
                                    Shirt = new Color(0.22f, 0.28f, 0.36f),
                                    HatColor = new Color(0.14f, 0.12f, 0.10f),
                                    Skin = new Color(0.78f, 0.52f, 0.38f),
                                    Accent = new Color(0.70f, 0.58f, 0.28f),
                                    HatType = HatStyle.Wide,
                                    Chest = Accessory.Vest,
                                    Facial = FacialHair.Mustache,
                                },
                                Intro = new[]
                                {
                                    new DialogLine(Speaker.Opponent, "He kept the roof. We kept the vests."),
                                    new DialogLine(Speaker.You,      "Then the vests can sink."),
                                    new DialogLine(Speaker.Opponent, "Two of us. Thick as the judge's door."),
                                },
                            },
                            new StageDef
                            {
                                Title = "Judge Pell", Arena = "Courthouse Roof", Difficulty = Difficulty.Hard,
                                Type = DuelType.Reaction,
                                Look = new CowboyLook
                                {
                                    Shirt = new Color(0.12f, 0.12f, 0.14f),
                                    HatColor = new Color(0.78f, 0.78f, 0.76f),
                                    Skin = new Color(0.86f, 0.76f, 0.66f),
                                    Accent = new Color(0.82f, 0.82f, 0.80f),
                                    HatType = HatStyle.Wide,
                                    Chest = Accessory.Vest,
                                    Facial = FacialHair.Beard,
                                },
                                Intro = new[]
                                {
                                    new DialogLine(Speaker.Opponent, "I sentenced this town at dawn."),
                                    new DialogLine(Speaker.You,      "The river beat you to it."),
                                    new DialogLine(Speaker.Opponent, "No bar. Draw. Let's see who the roof keeps."),
                                },
                            },
                        },
                    },
                },
            };
        }

        /// <summary>
        /// Elbow Pass in a closed winter. Arenas are procedural. Looks have no CharacterId.
        /// The red tail of the bar is the cold. Final-art notes: docs/CAMPAIGN_ART.md.
        /// </summary>
        public static CampaignDef WhiteSeason()
        {
            return new CampaignDef
            {
                Id = "white_season",
                Title = "White Season",
                Blurb = "Elbow Pass is shut. Bring the silver down.",
                Victory = "The pass stays shut.  The silver comes down.",
                Defeat = "Three hearts.  Elbow keeps the sled.",
                Chapters = new[]
                {
                    new ChapterDef
                    {
                        Title = "The Gate",
                        Tagline = "The pass is shut. The silver doesn't care.",
                        Theme = new Color(0.72f, 0.74f, 0.76f),
                        Stages = new[]
                        {
                            new StageDef
                            {
                                Title = "The Tollman", Arena = "Elbow Gate", Difficulty = Difficulty.Easy,
                                Look = new CowboyLook
                                {
                                    Shirt = new Color(0.42f, 0.36f, 0.32f),
                                    HatColor = new Color(0.22f, 0.18f, 0.16f),
                                    Skin = new Color(0.78f, 0.48f, 0.42f),
                                    Accent = new Color(0.55f, 0.22f, 0.20f),
                                    HatType = HatStyle.Bowler,
                                    Chest = Accessory.Bandana,
                                    Facial = FacialHair.Mustache,
                                },
                                Intro = new[]
                                {
                                    new DialogLine(Speaker.Opponent, "Pass is shut. Toll's the same."),
                                    new DialogLine(Speaker.You,      "I'm not staying till thaw."),
                                    new DialogLine(Speaker.Opponent, "Then hit the green before your hand sleeps."),
                                },
                            },
                            new StageDef
                            {
                                Title = "The Mittens", Arena = "Switchback", Difficulty = Difficulty.Easy,
                                Type = DuelType.Sync, Opponents = 2,
                                Look = new CowboyLook
                                {
                                    Shirt = new Color(0.36f, 0.28f, 0.22f),
                                    HatColor = new Color(0.18f, 0.14f, 0.12f),
                                    Skin = new Color(0.70f, 0.56f, 0.44f),
                                    Accent = new Color(0.48f, 0.36f, 0.24f),
                                    HatType = HatStyle.Wide,
                                    Chest = Accessory.Poncho,
                                    Facial = FacialHair.Beard,
                                },
                                Intro = new[]
                                {
                                    new DialogLine(Speaker.Opponent, "Two rifles. Two mittens. Don't mix them."),
                                    new DialogLine(Speaker.You,      "The silver isn't yours."),
                                    new DialogLine(Speaker.Opponent, "Miss, and the cold collects with us."),
                                },
                            },
                            new StageDef
                            {
                                Title = "The First White", Arena = "The Bowl", Difficulty = Difficulty.Normal,
                                Type = DuelType.Volley, Opponents = 3,
                                Look = new CowboyLook
                                {
                                    Shirt = new Color(0.62f, 0.66f, 0.70f),
                                    HatColor = new Color(0.28f, 0.30f, 0.34f),
                                    Skin = new Color(0.78f, 0.74f, 0.70f),
                                    Accent = new Color(0.70f, 0.78f, 0.84f),
                                    HatType = HatStyle.Wide,
                                    Chest = Accessory.Bandana,
                                    Facial = FacialHair.Beard,
                                },
                                Intro = new[]
                                {
                                    new DialogLine(Speaker.Opponent, "You won't count us. The white won't let you."),
                                    new DialogLine(Speaker.You,      "I don't need the count."),
                                    new DialogLine(Speaker.Opponent, "One, then the next. If the hand still works."),
                                },
                            },
                        },
                    },
                    new ChapterDef
                    {
                        Title = "The Bowl",
                        Tagline = "The silver was buried where the wind can't find it. They can.",
                        Theme = new Color(0.48f, 0.52f, 0.56f),
                        Stages = new[]
                        {
                            new StageDef
                            {
                                Title = "The Cache", Arena = "Cache Bowl", Difficulty = Difficulty.Normal,
                                Type = DuelType.Sync, Opponents = 4, Hp = 3, Strike = 1,
                                Look = new CowboyLook
                                {
                                    Shirt = new Color(0.32f, 0.30f, 0.28f),
                                    HatColor = new Color(0.16f, 0.14f, 0.12f),
                                    Skin = new Color(0.66f, 0.54f, 0.44f),
                                    Accent = new Color(0.62f, 0.52f, 0.28f),
                                    HatType = HatStyle.Wide,
                                    Chest = Accessory.Vest,
                                    Facial = FacialHair.Beard,
                                },
                                Intro = new[]
                                {
                                    new DialogLine(Speaker.Opponent, "Four of us. The coats are plated."),
                                    new DialogLine(Speaker.You,      "Each hand's good for two."),
                                    new DialogLine(Speaker.Opponent, "Steel under the fur. Don't slip."),
                                },
                            },
                            new StageDef
                            {
                                Title = "The Smoke", Arena = "Bowl Rim", Difficulty = Difficulty.Normal,
                                Look = new CowboyLook
                                {
                                    Shirt = new Color(0.48f, 0.42f, 0.40f),
                                    HatColor = new Color(0.24f, 0.20f, 0.18f),
                                    Skin = new Color(0.80f, 0.66f, 0.54f),
                                    Accent = new Color(0.28f, 0.40f, 0.28f),
                                    HatType = HatStyle.Cowboy,
                                    Chest = Accessory.Bandana,
                                    Facial = FacialHair.None,
                                },
                                Intro = new[]
                                {
                                    new DialogLine(Speaker.Opponent, "He's been up there since the first snow."),
                                    new DialogLine(Speaker.You,      "Then he's tired."),
                                    new DialogLine(Speaker.Opponent, "He won't give you a bar. He waits for the ice to crack."),
                                },
                            },
                        },
                    },
                    new ChapterDef
                    {
                        Title = "The Saddle",
                        Tagline = "One cabin. The wind spends what the bullets don't.",
                        Theme = new Color(0.28f, 0.34f, 0.48f),
                        Stages = new[]
                        {
                            new StageDef
                            {
                                Title = "The Drift", Arena = "Saddle Drift", Difficulty = Difficulty.Hard,
                                Type = DuelType.Volley, Opponents = 2, Hp = 3, Strike = 1,
                                Look = new CowboyLook
                                {
                                    Shirt = new Color(0.26f, 0.30f, 0.36f),
                                    HatColor = new Color(0.12f, 0.14f, 0.18f),
                                    Skin = new Color(0.62f, 0.58f, 0.56f),
                                    Accent = new Color(0.55f, 0.62f, 0.70f),
                                    HatType = HatStyle.Wide,
                                    Chest = Accessory.Vest,
                                    Facial = FacialHair.Mustache,
                                },
                                Intro = new[]
                                {
                                    new DialogLine(Speaker.Opponent, "He sent us down. The drift hid the rest."),
                                    new DialogLine(Speaker.You,      "Then the drift can keep you."),
                                    new DialogLine(Speaker.Opponent, "Two windows. Plates under the coats."),
                                },
                            },
                            new StageDef
                            {
                                Title = "The Wind", Arena = "Saddle Stair", Difficulty = Difficulty.Hard,
                                Type = DuelType.Sync, Opponents = 2, Strike = 2,
                                Look = new CowboyLook
                                {
                                    Shirt = new Color(0.55f, 0.60f, 0.66f),
                                    HatColor = new Color(0.20f, 0.22f, 0.26f),
                                    Skin = new Color(0.74f, 0.68f, 0.64f),
                                    Accent = new Color(0.40f, 0.50f, 0.60f),
                                    HatType = HatStyle.Cowboy,
                                    Chest = Accessory.None,
                                    Facial = FacialHair.None,
                                },
                                Intro = new[]
                                {
                                    new DialogLine(Speaker.Opponent, "The wind takes two for every miss."),
                                    new DialogLine(Speaker.You,      "Then I won't miss."),
                                    new DialogLine(Speaker.Opponent, "Left and right. Then the door. He likes the crack."),
                                },
                            },
                            new StageDef
                            {
                                Title = "The Tenant", Arena = "Saddle Cabin", Difficulty = Difficulty.Hard,
                                Type = DuelType.Reaction,
                                Look = new CowboyLook
                                {
                                    Shirt = new Color(0.22f, 0.16f, 0.14f),
                                    HatColor = new Color(0.14f, 0.12f, 0.10f),
                                    Skin = new Color(0.78f, 0.70f, 0.62f),
                                    Accent = new Color(0.72f, 0.36f, 0.18f),
                                    HatType = HatStyle.Cowboy,
                                    Chest = Accessory.Poncho,
                                    Facial = FacialHair.Beard,
                                },
                                Intro = new[]
                                {
                                    new DialogLine(Speaker.Opponent, "I wintered on your silver."),
                                    new DialogLine(Speaker.You,      "It was never mine. It isn't yours."),
                                    new DialogLine(Speaker.Opponent, "No bar. Draw when the ice speaks."),
                                },
                            },
                        },
                    },
                },
            };
        }

        /// <summary>
        /// The mission at San Isidro. Arenas are procedural. Looks have no CharacterId.
        /// The novice who left is not a node. Final-art notes: docs/CAMPAIGN_ART.md.
        /// </summary>
        public static CampaignDef SanIsidro()
        {
            return new CampaignDef
            {
                Id = "san_isidro",
                Title = "San Isidro",
                Blurb = "The saint is hollow. The girl is not.",
                Victory = "The saint is empty.  The girl walks out.",
                Defeat = "Three hearts.  San Isidro keeps the name.",
                Chapters = new[]
                {
                    new ChapterDef
                    {
                        Title = "The Yard",
                        Tagline = "The gate is open. The saint is not.",
                        Theme = new Color(0.72f, 0.58f, 0.36f),
                        Stages = new[]
                        {
                            new StageDef
                            {
                                Title = "The Porter", Arena = "Mission Gate", Difficulty = Difficulty.Easy,
                                Look = new CowboyLook
                                {
                                    Shirt = new Color(0.78f, 0.70f, 0.56f),
                                    HatColor = new Color(0.36f, 0.24f, 0.14f),
                                    Skin = new Color(0.72f, 0.52f, 0.36f),
                                    Accent = new Color(0.62f, 0.50f, 0.22f),
                                    HatType = HatStyle.Sombrero,
                                    Chest = Accessory.Vest,
                                    Facial = FacialHair.Beard,
                                },
                                Intro = new[]
                                {
                                    new DialogLine(Speaker.Opponent, "The yard is for prayer. Yours is late."),
                                    new DialogLine(Speaker.You,      "I'm looking for a woman who left armed."),
                                    new DialogLine(Speaker.Opponent, "Then hit the green. The bell only rings clean."),
                                },
                            },
                            new StageDef
                            {
                                Title = "The Two Candles", Arena = "Candle Court", Difficulty = Difficulty.Easy,
                                Type = DuelType.Sync, Opponents = 2,
                                Look = new CowboyLook
                                {
                                    Shirt = new Color(0.82f, 0.78f, 0.68f),
                                    HatColor = new Color(0.28f, 0.22f, 0.16f),
                                    Skin = new Color(0.84f, 0.66f, 0.52f),
                                    Accent = new Color(0.90f, 0.72f, 0.28f),
                                    HatType = HatStyle.Cowboy,
                                    Chest = Accessory.Bandana,
                                    Facial = FacialHair.None,
                                },
                                Intro = new[]
                                {
                                    new DialogLine(Speaker.Opponent, "Two candles. Two hands. Don't cross the flames."),
                                    new DialogLine(Speaker.You,      "She came through here."),
                                    new DialogLine(Speaker.Opponent, "Miss either, and that one fires."),
                                },
                            },
                            new StageDef
                            {
                                Title = "The Procession", Arena = "West Cloister", Difficulty = Difficulty.Normal,
                                Type = DuelType.Volley, Opponents = 3,
                                Look = new CowboyLook
                                {
                                    Shirt = new Color(0.40f, 0.28f, 0.18f),
                                    HatColor = new Color(0.22f, 0.16f, 0.12f),
                                    Skin = new Color(0.62f, 0.46f, 0.34f),
                                    Accent = new Color(0.45f, 0.32f, 0.18f),
                                    HatType = HatStyle.Wide,
                                    Chest = Accessory.Poncho,
                                    Facial = FacialHair.Beard,
                                },
                                Intro = new[]
                                {
                                    new DialogLine(Speaker.Opponent, "We don't break step for a guest."),
                                    new DialogLine(Speaker.You,      "You'll break it for a bullet."),
                                    new DialogLine(Speaker.Opponent, "Three of us. One at a time. The saint can wait."),
                                },
                            },
                        },
                    },
                    new ChapterDef
                    {
                        Title = "The Nave",
                        Tagline = "The rifles were never blessed. The steel was.",
                        Theme = new Color(0.36f, 0.28f, 0.22f),
                        Stages = new[]
                        {
                            new StageDef
                            {
                                Title = "The Choir", Arena = "The Nave", Difficulty = Difficulty.Normal,
                                Look = new CowboyLook
                                {
                                    Shirt = new Color(0.16f, 0.14f, 0.16f),
                                    HatColor = new Color(0.10f, 0.10f, 0.12f),
                                    Skin = new Color(0.78f, 0.62f, 0.50f),
                                    Accent = new Color(0.55f, 0.42f, 0.22f),
                                    HatType = HatStyle.Cowboy,
                                    Chest = Accessory.None,
                                    Facial = FacialHair.None,
                                },
                                Intro = new[]
                                {
                                    new DialogLine(Speaker.Opponent, "The hymn is a count. You missed the start."),
                                    new DialogLine(Speaker.You,      "I didn't come to sing."),
                                    new DialogLine(Speaker.Opponent, "Then land the green. The note won't wait."),
                                },
                            },
                            new StageDef
                            {
                                Title = "The Vestry", Arena = "Vestry", Difficulty = Difficulty.Normal,
                                Type = DuelType.Sync, Opponents = 4, Hp = 3, Strike = 1,
                                Look = new CowboyLook
                                {
                                    Shirt = new Color(0.78f, 0.74f, 0.66f),
                                    HatColor = new Color(0.20f, 0.18f, 0.16f),
                                    Skin = new Color(0.70f, 0.54f, 0.42f),
                                    Accent = new Color(0.55f, 0.58f, 0.60f),
                                    HatType = HatStyle.Bowler,
                                    Chest = Accessory.Vest,
                                    Facial = FacialHair.Mustache,
                                },
                                Intro = new[]
                                {
                                    new DialogLine(Speaker.Opponent, "Four of us. The cloth is lined."),
                                    new DialogLine(Speaker.You,      "Each hand's good for two."),
                                    new DialogLine(Speaker.Opponent, "Steel under the linen. The saint paid for it."),
                                },
                            },
                            new StageDef
                            {
                                Title = "The Novice", Arena = "Side Chapel", Difficulty = Difficulty.Normal,
                                Look = new CowboyLook
                                {
                                    Shirt = new Color(0.62f, 0.56f, 0.50f),
                                    HatColor = new Color(0.30f, 0.26f, 0.22f),
                                    Skin = new Color(0.86f, 0.72f, 0.60f),
                                    Accent = new Color(0.50f, 0.44f, 0.40f),
                                    HatType = HatStyle.Cowboy,
                                    Chest = Accessory.Bandana,
                                    Facial = FacialHair.None,
                                },
                                Intro = new[]
                                {
                                    new DialogLine(Speaker.Opponent, "She took a rifle and a dead girl's name."),
                                    new DialogLine(Speaker.You,      "Where did she take them?"),
                                    new DialogLine(Speaker.Opponent, "To the cells. The superior won't give you a bar. She threw it out."),
                                },
                            },
                        },
                    },
                    new ChapterDef
                    {
                        Title = "The Cells",
                        Tagline = "The hollow saint is empty. The woman who filled it is not.",
                        Theme = new Color(0.78f, 0.76f, 0.70f),
                        Stages = new[]
                        {
                            new StageDef
                            {
                                Title = "The Confession", Arena = "Confession Alley", Difficulty = Difficulty.Hard,
                                Type = DuelType.Sync, Opponents = 2, Strike = 2,
                                Look = new CowboyLook
                                {
                                    Shirt = new Color(0.22f, 0.20f, 0.24f),
                                    HatColor = new Color(0.14f, 0.12f, 0.12f),
                                    Skin = new Color(0.68f, 0.54f, 0.44f),
                                    Accent = new Color(0.12f, 0.14f, 0.22f),
                                    HatType = HatStyle.Wide,
                                    Chest = Accessory.Vest,
                                    Facial = FacialHair.Beard,
                                },
                                Intro = new[]
                                {
                                    new DialogLine(Speaker.Opponent, "Confess the miss. It costs you twice."),
                                    new DialogLine(Speaker.You,      "I'm not here for pardon."),
                                    new DialogLine(Speaker.Opponent, "Left and right. Then the saint. Then her."),
                                },
                            },
                            new StageDef
                            {
                                Title = "The Reliquary", Arena = "Hollow Saint", Difficulty = Difficulty.Hard,
                                Type = DuelType.Volley, Opponents = 2, Hp = 6, Strike = 1,
                                Look = new CowboyLook
                                {
                                    Shirt = new Color(0.28f, 0.26f, 0.24f),
                                    HatColor = new Color(0.72f, 0.64f, 0.48f),
                                    Skin = new Color(0.76f, 0.52f, 0.38f),
                                    Accent = new Color(0.70f, 0.72f, 0.74f),
                                    HatType = HatStyle.Wide,
                                    Chest = Accessory.Vest,
                                    Facial = FacialHair.Mustache,
                                },
                                Intro = new[]
                                {
                                    new DialogLine(Speaker.Opponent, "She kept the rifle. We kept the plates."),
                                    new DialogLine(Speaker.You,      "Then the plates come off."),
                                    new DialogLine(Speaker.Opponent, "Two of us. Thicker than the statue was."),
                                },
                            },
                            new StageDef
                            {
                                Title = "The Superior", Arena = "The Cells", Difficulty = Difficulty.Hard,
                                Type = DuelType.Reaction,
                                Look = new CowboyLook
                                {
                                    Shirt = new Color(0.18f, 0.16f, 0.18f),
                                    HatColor = new Color(0.12f, 0.10f, 0.12f),
                                    Skin = new Color(0.82f, 0.70f, 0.60f),
                                    Accent = new Color(0.20f, 0.18f, 0.18f),
                                    HatType = HatStyle.Bowler,
                                    Chest = Accessory.None,
                                    Facial = FacialHair.None,
                                },
                                Intro = new[]
                                {
                                    new DialogLine(Speaker.Opponent, "God does not sweep a bar for you."),
                                    new DialogLine(Speaker.You,      "I didn't ask him to."),
                                    new DialogLine(Speaker.Opponent, "No bar. Draw. The girl stays behind me."),
                                },
                            },
                        },
                    },
                },
            };
        }

        /// <summary>
        /// County fair at Bright's Meadow. Arenas are procedural. Looks have no CharacterId.
        /// The horse is not a node. Final-art notes: docs/CAMPAIGN_ART.md.
        /// </summary>
        public static CampaignDef SundayHorses()
        {
            return new CampaignDef
            {
                Id = "sunday_horses",
                Title = "Sunday Horses",
                Blurb = "You came for a horse. The track kept a gang.",
                Victory = "The pistol is down.  The horse is yours.",
                Defeat = "Three hearts.  Bright's Meadow keeps the odds.",
                Chapters = new[]
                {
                    new ChapterDef
                    {
                        Title = "The Rail",
                        Tagline = "The ribbon is a game until it isn't.",
                        Theme = new Color(0.86f, 0.62f, 0.28f),
                        Stages = new[]
                        {
                            new StageDef
                            {
                                Title = "The Ribbon", Arena = "Bright's Rail", Difficulty = Difficulty.Easy,
                                Look = new CowboyLook
                                {
                                    Shirt = new Color(0.86f, 0.28f, 0.24f),
                                    HatColor = new Color(0.22f, 0.16f, 0.12f),
                                    Skin = new Color(0.86f, 0.68f, 0.52f),
                                    Accent = new Color(0.92f, 0.78f, 0.28f),
                                    HatType = HatStyle.Cowboy,
                                    Chest = Accessory.Bandana,
                                    Facial = FacialHair.None,
                                },
                                Intro = new[]
                                {
                                    new DialogLine(Speaker.Opponent, "Ribbon's up. You blink, you lose."),
                                    new DialogLine(Speaker.You,      "I'm not racing."),
                                    new DialogLine(Speaker.Opponent, "Everybody's racing. Hit the green."),
                                },
                            },
                            new StageDef
                            {
                                Title = "The Pair", Arena = "The Paddock", Difficulty = Difficulty.Easy,
                                Type = DuelType.Sync, Opponents = 2,
                                Look = new CowboyLook
                                {
                                    Shirt = new Color(0.22f, 0.55f, 0.36f),
                                    HatColor = new Color(0.16f, 0.14f, 0.12f),
                                    Skin = new Color(0.78f, 0.58f, 0.42f),
                                    Accent = new Color(0.90f, 0.72f, 0.22f),
                                    HatType = HatStyle.Cowboy,
                                    Chest = Accessory.None,
                                    Facial = FacialHair.None,
                                },
                                Intro = new[]
                                {
                                    new DialogLine(Speaker.Opponent, "Two of us. We ride abreast. So do your hands."),
                                    new DialogLine(Speaker.You,      "I came for a horse, not a comedy."),
                                    new DialogLine(Speaker.Opponent, "Miss either and that one shoots. The crowd likes a fall."),
                                },
                            },
                            new StageDef
                            {
                                Title = "The Field", Arena = "Starting Gate", Difficulty = Difficulty.Normal,
                                Type = DuelType.Volley, Opponents = 3,
                                Look = new CowboyLook
                                {
                                    Shirt = new Color(0.42f, 0.32f, 0.22f),
                                    HatColor = new Color(0.18f, 0.14f, 0.10f),
                                    Skin = new Color(0.70f, 0.52f, 0.38f),
                                    Accent = new Color(0.32f, 0.24f, 0.16f),
                                    HatType = HatStyle.Wide,
                                    Chest = Accessory.Vest,
                                    Facial = FacialHair.Mustache,
                                },
                                Intro = new[]
                                {
                                    new DialogLine(Speaker.Opponent, "They think it's a start. It's a line."),
                                    new DialogLine(Speaker.You,      "Then the line can stop."),
                                    new DialogLine(Speaker.Opponent, "Three of us. One gate at a time."),
                                },
                            },
                        },
                    },
                    new ChapterDef
                    {
                        Title = "The Box",
                        Tagline = "The odds were written before you arrived.",
                        Theme = new Color(0.78f, 0.70f, 0.42f),
                        Stages = new[]
                        {
                            new StageDef
                            {
                                Title = "The Tout", Arena = "Betting Rail", Difficulty = Difficulty.Normal,
                                Look = new CowboyLook
                                {
                                    Shirt = new Color(0.72f, 0.68f, 0.58f),
                                    HatColor = new Color(0.20f, 0.16f, 0.12f),
                                    Skin = new Color(0.78f, 0.48f, 0.34f),
                                    Accent = new Color(0.90f, 0.86f, 0.70f),
                                    HatType = HatStyle.Bowler,
                                    Chest = Accessory.Vest,
                                    Facial = FacialHair.Mustache,
                                },
                                Intro = new[]
                                {
                                    new DialogLine(Speaker.Opponent, "I liked you better as a long shot."),
                                    new DialogLine(Speaker.You,      "Tear the ticket."),
                                    new DialogLine(Speaker.Opponent, "Hit the green. The price doesn't move."),
                                },
                            },
                            new StageDef
                            {
                                Title = "The Stable", Arena = "Cade's Stable", Difficulty = Difficulty.Normal,
                                Type = DuelType.Sync, Opponents = 4, Hp = 3, Strike = 1,
                                Look = new CowboyLook
                                {
                                    Shirt = new Color(0.40f, 0.30f, 0.18f),
                                    HatColor = new Color(0.16f, 0.12f, 0.08f),
                                    Skin = new Color(0.66f, 0.50f, 0.36f),
                                    Accent = new Color(0.62f, 0.52f, 0.22f),
                                    HatType = HatStyle.Wide,
                                    Chest = Accessory.Vest,
                                    Facial = FacialHair.Beard,
                                },
                                Intro = new[]
                                {
                                    new DialogLine(Speaker.Opponent, "Four of us. The vests are stuffed and lined."),
                                    new DialogLine(Speaker.You,      "Each hand's good for two."),
                                    new DialogLine(Speaker.Opponent, "Steel under the silk. The horse stays."),
                                },
                            },
                            new StageDef
                            {
                                Title = "The Bugler", Arena = "Winners' Circle", Difficulty = Difficulty.Normal,
                                Look = new CowboyLook
                                {
                                    Shirt = new Color(0.36f, 0.32f, 0.28f),
                                    HatColor = new Color(0.22f, 0.18f, 0.14f),
                                    Skin = new Color(0.72f, 0.58f, 0.46f),
                                    Accent = new Color(0.55f, 0.42f, 0.22f),
                                    HatType = HatStyle.Cowboy,
                                    Chest = Accessory.None,
                                    Facial = FacialHair.Mustache,
                                },
                                Intro = new[]
                                {
                                    new DialogLine(Speaker.Opponent, "He doesn't keep a bar. He keeps the pistol that starts a race."),
                                    new DialogLine(Speaker.You,      "Then I'll be there when he lifts it."),
                                    new DialogLine(Speaker.Opponent, "He waits for that one shot. He only needs you to flinch."),
                                },
                            },
                        },
                    },
                    new ChapterDef
                    {
                        Title = "The Gun",
                        Tagline = "The ribbon is in the mud. The pistol is in his hand.",
                        Theme = new Color(0.55f, 0.48f, 0.32f),
                        Stages = new[]
                        {
                            new StageDef
                            {
                                Title = "The Weigh-In", Arena = "Scale House", Difficulty = Difficulty.Hard,
                                Type = DuelType.Sync, Opponents = 2, Strike = 2,
                                Look = new CowboyLook
                                {
                                    Shirt = new Color(0.48f, 0.46f, 0.42f),
                                    HatColor = new Color(0.14f, 0.12f, 0.10f),
                                    Skin = new Color(0.74f, 0.60f, 0.48f),
                                    Accent = new Color(0.20f, 0.22f, 0.28f),
                                    HatType = HatStyle.Bowler,
                                    Chest = Accessory.Vest,
                                    Facial = FacialHair.None,
                                },
                                Intro = new[]
                                {
                                    new DialogLine(Speaker.Opponent, "You come in heavy. A miss costs double."),
                                    new DialogLine(Speaker.You,      "I'm not on the scale."),
                                    new DialogLine(Speaker.Opponent, "Left and right. Then his two. Then him."),
                                },
                            },
                            new StageDef
                            {
                                Title = "The Favorite", Arena = "Owners' Box", Difficulty = Difficulty.Hard,
                                Type = DuelType.Volley, Opponents = 2, Hp = 6, Strike = 1,
                                Look = new CowboyLook
                                {
                                    Shirt = new Color(0.78f, 0.22f, 0.28f),
                                    HatColor = new Color(0.12f, 0.10f, 0.10f),
                                    Skin = new Color(0.70f, 0.52f, 0.40f),
                                    Accent = new Color(0.72f, 0.74f, 0.76f),
                                    HatType = HatStyle.Cowboy,
                                    Chest = Accessory.Vest,
                                    Facial = FacialHair.Mustache,
                                },
                                Intro = new[]
                                {
                                    new DialogLine(Speaker.Opponent, "He named us the favorite. The silks are a joke."),
                                    new DialogLine(Speaker.You,      "The plates aren't."),
                                    new DialogLine(Speaker.Opponent, "Two of us. Thicker than a purse."),
                                },
                            },
                            new StageDef
                            {
                                Title = "Cade", Arena = "The Gun", Difficulty = Difficulty.Hard,
                                Type = DuelType.Reaction,
                                Look = new CowboyLook
                                {
                                    Shirt = new Color(0.86f, 0.82f, 0.72f),
                                    HatColor = new Color(0.78f, 0.74f, 0.62f),
                                    Skin = new Color(0.84f, 0.70f, 0.58f),
                                    Accent = new Color(0.72f, 0.74f, 0.78f),
                                    HatType = HatStyle.Bowler,
                                    Chest = Accessory.Vest,
                                    Facial = FacialHair.Mustache,
                                },
                                Intro = new[]
                                {
                                    new DialogLine(Speaker.Opponent, "I start every race in this county."),
                                    new DialogLine(Speaker.You,      "This one ends."),
                                    new DialogLine(Speaker.Opponent, "No ribbon. Draw on the pistol."),
                                },
                            },
                        },
                    },
                },
            };
        }

        /// <summary>
        /// Judge Mather's wagon season. Arenas are procedural. Looks have no CharacterId.
        /// Reaction is only the bench. Final-art notes: docs/CAMPAIGN_ART.md.
        /// </summary>
        public static CampaignDef TheCircuit()
        {
            return new CampaignDef
            {
                Id = "the_circuit",
                Title = "The Circuit",
                Blurb = "Five towns. Your name is already in the book.",
                Victory = "The docket is blank.  The wagon can go home.",
                Defeat = "Three hearts.  Mather still has the name.",
                Chapters = new[]
                {
                    new ChapterDef
                    {
                        Title = "Red Ankle",
                        Tagline = "The gallows came in on the morning wagon.",
                        Theme = new Color(0.72f, 0.55f, 0.32f),
                        Stages = new[]
                        {
                            new StageDef
                            {
                                Title = "The Bailiff", Arena = "Red Ankle", Difficulty = Difficulty.Easy,
                                Look = new CowboyLook
                                {
                                    Shirt = new Color(0.55f, 0.52f, 0.46f),
                                    HatColor = new Color(0.16f, 0.14f, 0.12f),
                                    Skin = new Color(0.74f, 0.58f, 0.44f),
                                    Accent = new Color(0.72f, 0.58f, 0.28f),
                                    HatType = HatStyle.Bowler,
                                    Chest = Accessory.Vest,
                                    Facial = FacialHair.Mustache,
                                },
                                Intro = new[]
                                {
                                    new DialogLine(Speaker.Opponent, "You're on today's card. The judge likes a clock."),
                                    new DialogLine(Speaker.You,      "I didn't ask for a time."),
                                    new DialogLine(Speaker.Opponent, "You have one. Hit the green or be late."),
                                },
                            },
                            new StageDef
                            {
                                Title = "The Deputies", Arena = "Red Ankle Yard", Difficulty = Difficulty.Easy,
                                Type = DuelType.Sync, Opponents = 2,
                                Look = new CowboyLook
                                {
                                    Shirt = new Color(0.32f, 0.34f, 0.38f),
                                    HatColor = new Color(0.18f, 0.16f, 0.14f),
                                    Skin = new Color(0.78f, 0.62f, 0.48f),
                                    Accent = new Color(0.14f, 0.16f, 0.28f),
                                    HatType = HatStyle.Cowboy,
                                    Chest = Accessory.Badge,
                                    Facial = FacialHair.None,
                                },
                                Intro = new[]
                                {
                                    new DialogLine(Speaker.Opponent, "We read it together. We shoot the same way."),
                                    new DialogLine(Speaker.You,      "Read it to the man who wrote it."),
                                    new DialogLine(Speaker.Opponent, "Miss either of us and that one fires."),
                                },
                            },
                            new StageDef
                            {
                                Title = "The Jury", Arena = "Red Ankle Square", Difficulty = Difficulty.Normal,
                                Type = DuelType.Volley, Opponents = 3,
                                Look = new CowboyLook
                                {
                                    Shirt = new Color(0.48f, 0.40f, 0.30f),
                                    HatColor = new Color(0.22f, 0.16f, 0.12f),
                                    Skin = new Color(0.72f, 0.56f, 0.42f),
                                    Accent = new Color(0.40f, 0.32f, 0.22f),
                                    HatType = HatStyle.Bowler,
                                    Chest = Accessory.Vest,
                                    Facial = FacialHair.Mustache,
                                },
                                Intro = new[]
                                {
                                    new DialogLine(Speaker.Opponent, "We voted at breakfast. This is the carrying out."),
                                    new DialogLine(Speaker.You,      "You voted without me."),
                                    new DialogLine(Speaker.Opponent, "Three of us. One at a time. The clerk hates a mess."),
                                },
                            },
                        },
                    },
                    new ChapterDef
                    {
                        Title = "Miller's Ford",
                        Tagline = "The ford charges a toll. The court charges you.",
                        Theme = new Color(0.45f, 0.40f, 0.32f),
                        Stages = new[]
                        {
                            new StageDef
                            {
                                Title = "The Widow", Arena = "Miller's Ford", Difficulty = Difficulty.Normal,
                                Look = new CowboyLook
                                {
                                    Shirt = new Color(0.36f, 0.30f, 0.26f),
                                    HatColor = new Color(0.20f, 0.16f, 0.14f),
                                    Skin = new Color(0.70f, 0.54f, 0.42f),
                                    Accent = new Color(0.42f, 0.32f, 0.24f),
                                    HatType = HatStyle.Wide,
                                    Chest = Accessory.Poncho,
                                    Facial = FacialHair.None,
                                },
                                Intro = new[]
                                {
                                    new DialogLine(Speaker.Opponent, "He paid your crossing. Both ways, he said."),
                                    new DialogLine(Speaker.You,      "I'll pay my own."),
                                    new DialogLine(Speaker.Opponent, "Hit the green. The ford doesn't do credit."),
                                },
                            },
                            new StageDef
                            {
                                Title = "The Clerks", Arena = "Ford Office", Difficulty = Difficulty.Normal,
                                Type = DuelType.Sync, Opponents = 4, Hp = 3, Strike = 1,
                                Look = new CowboyLook
                                {
                                    Shirt = new Color(0.42f, 0.44f, 0.46f),
                                    HatColor = new Color(0.12f, 0.12f, 0.14f),
                                    Skin = new Color(0.76f, 0.64f, 0.52f),
                                    Accent = new Color(0.55f, 0.58f, 0.60f),
                                    HatType = HatStyle.Bowler,
                                    Chest = Accessory.Vest,
                                    Facial = FacialHair.Mustache,
                                },
                                Intro = new[]
                                {
                                    new DialogLine(Speaker.Opponent, "Four of us. The boxes are lined."),
                                    new DialogLine(Speaker.You,      "Each hand's good for two."),
                                    new DialogLine(Speaker.Opponent, "Steel under the paper. Don't smudge it."),
                                },
                            },
                            new StageDef
                            {
                                Title = "The Boy", Arena = "Ford Chapel", Difficulty = Difficulty.Normal,
                                Look = new CowboyLook
                                {
                                    Shirt = new Color(0.28f, 0.24f, 0.22f),
                                    HatColor = new Color(0.14f, 0.12f, 0.10f),
                                    Skin = new Color(0.82f, 0.68f, 0.54f),
                                    Accent = new Color(0.72f, 0.48f, 0.22f),
                                    HatType = HatStyle.Cowboy,
                                    Chest = Accessory.None,
                                    Facial = FacialHair.None,
                                },
                                Intro = new[]
                                {
                                    new DialogLine(Speaker.Opponent, "He looks at you, not at a watch."),
                                    new DialogLine(Speaker.You,      "Every town so far had a clock."),
                                    new DialogLine(Speaker.Opponent, "Those were the deputies. He waits. No bar. Not at the end."),
                                },
                            },
                        },
                    },
                    new ChapterDef
                    {
                        Title = "Glass Hill",
                        Tagline = "From the hill the whole circuit is one road.",
                        Theme = new Color(0.55f, 0.58f, 0.62f),
                        Stages = new[]
                        {
                            new StageDef
                            {
                                Title = "The Glass", Arena = "Glass Hill", Difficulty = Difficulty.Hard,
                                Type = DuelType.Volley, Opponents = 2, Hp = 3, Strike = 1,
                                Look = new CowboyLook
                                {
                                    Shirt = new Color(0.30f, 0.32f, 0.34f),
                                    HatColor = new Color(0.16f, 0.16f, 0.18f),
                                    Skin = new Color(0.72f, 0.48f, 0.40f),
                                    Accent = new Color(0.62f, 0.64f, 0.66f),
                                    HatType = HatStyle.Wide,
                                    Chest = Accessory.Vest,
                                    Facial = FacialHair.Beard,
                                },
                                Intro = new[]
                                {
                                    new DialogLine(Speaker.Opponent, "We see every town from here. Including you."),
                                    new DialogLine(Speaker.You,      "Then look away."),
                                    new DialogLine(Speaker.Opponent, "Two windows. Plates under the coats."),
                                },
                            },
                            new StageDef
                            {
                                Title = "The Notary", Arena = "Hill Notary", Difficulty = Difficulty.Hard,
                                Type = DuelType.Sync, Opponents = 2, Strike = 2,
                                Look = new CowboyLook
                                {
                                    Shirt = new Color(0.38f, 0.32f, 0.30f),
                                    HatColor = new Color(0.18f, 0.14f, 0.12f),
                                    Skin = new Color(0.78f, 0.64f, 0.52f),
                                    Accent = new Color(0.62f, 0.18f, 0.16f),
                                    HatType = HatStyle.Bowler,
                                    Chest = Accessory.Vest,
                                    Facial = FacialHair.None,
                                },
                                Intro = new[]
                                {
                                    new DialogLine(Speaker.Opponent, "A bad stamp costs double. So does a miss."),
                                    new DialogLine(Speaker.You,      "Don't stamp me."),
                                    new DialogLine(Speaker.Opponent, "Left and right. The elm is next. He is after that."),
                                },
                            },
                        },
                    },
                    new ChapterDef
                    {
                        Title = "Hanging Elm",
                        Tagline = "The tree has more rope than leaves.",
                        Theme = new Color(0.42f, 0.28f, 0.32f),
                        Stages = new[]
                        {
                            new StageDef
                            {
                                Title = "The Panel", Arena = "Hanging Elm", Difficulty = Difficulty.Hard,
                                Type = DuelType.Volley, Opponents = 3,
                                Look = new CowboyLook
                                {
                                    Shirt = new Color(0.40f, 0.34f, 0.28f),
                                    HatColor = new Color(0.22f, 0.18f, 0.14f),
                                    Skin = new Color(0.64f, 0.50f, 0.40f),
                                    Accent = new Color(0.36f, 0.28f, 0.22f),
                                    HatType = HatStyle.Wide,
                                    Chest = Accessory.None,
                                    Facial = FacialHair.Beard,
                                },
                                Intro = new[]
                                {
                                    new DialogLine(Speaker.Opponent, "We served before. We came to serve again."),
                                    new DialogLine(Speaker.You,      "You served him."),
                                    new DialogLine(Speaker.Opponent, "Three windows. Fall in order, or don't."),
                                },
                            },
                            new StageDef
                            {
                                Title = "The Elm", Arena = "Elm Shade", Difficulty = Difficulty.Hard,
                                Look = new CowboyLook
                                {
                                    Shirt = new Color(0.32f, 0.34f, 0.30f),
                                    HatColor = new Color(0.16f, 0.18f, 0.14f),
                                    Skin = new Color(0.72f, 0.60f, 0.50f),
                                    Accent = new Color(0.42f, 0.30f, 0.20f),
                                    HatType = HatStyle.Cowboy,
                                    Chest = Accessory.Bandana,
                                    Facial = FacialHair.None,
                                },
                                Intro = new[]
                                {
                                    new DialogLine(Speaker.Opponent, "I cut the rope after. I'd like to stop."),
                                    new DialogLine(Speaker.You,      "Then let me pass."),
                                    new DialogLine(Speaker.Opponent, "Hit it clean. The shade won't judge you. He will."),
                                },
                            },
                        },
                    },
                    new ChapterDef
                    {
                        Title = "The Seat",
                        Tagline = "No town name. The bench is the town.",
                        Theme = new Color(0.32f, 0.30f, 0.28f),
                        Stages = new[]
                        {
                            new StageDef
                            {
                                Title = "The Robe", Arena = "Judges' Seat", Difficulty = Difficulty.Hard,
                                Type = DuelType.Volley, Opponents = 2, Hp = 6, Strike = 1,
                                Look = new CowboyLook
                                {
                                    Shirt = new Color(0.14f, 0.14f, 0.16f),
                                    HatColor = new Color(0.10f, 0.10f, 0.12f),
                                    Skin = new Color(0.68f, 0.54f, 0.42f),
                                    Accent = new Color(0.58f, 0.60f, 0.62f),
                                    HatType = HatStyle.Wide,
                                    Chest = Accessory.Poncho,
                                    Facial = FacialHair.Mustache,
                                },
                                Intro = new[]
                                {
                                    new DialogLine(Speaker.Opponent, "He has robes to spare. The plates were measured on him."),
                                    new DialogLine(Speaker.You,      "Then they won't fit you."),
                                    new DialogLine(Speaker.Opponent, "Two of us. Thick as the bench."),
                                },
                            },
                            new StageDef
                            {
                                Title = "Judge Mather", Arena = "The Bench", Difficulty = Difficulty.Hard,
                                Type = DuelType.Reaction,
                                Look = new CowboyLook
                                {
                                    Shirt = new Color(0.16f, 0.16f, 0.18f),
                                    HatColor = new Color(0.12f, 0.11f, 0.10f),
                                    Skin = new Color(0.70f, 0.58f, 0.48f),
                                    Accent = new Color(0.36f, 0.32f, 0.26f),
                                    HatType = HatStyle.Bowler,
                                    Chest = Accessory.Vest,
                                    Facial = FacialHair.Mustache,
                                },
                                Intro = new[]
                                {
                                    new DialogLine(Speaker.Opponent, "You kept every appointment but this one."),
                                    new DialogLine(Speaker.You,      "This one isn't on your clock."),
                                    new DialogLine(Speaker.Opponent, "I don't keep a clock. Draw."),
                                },
                            },
                        },
                    },
                },
            };
        }
    }
}
