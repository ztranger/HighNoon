namespace HighNoon
{
    public enum StoryKind { ChapterIntro, Victory, Defeat }

    /// <summary>Which full-screen story beat the shared <c>Story</c> scene should show next.</summary>
    public static class Story
    {
        public static StoryKind Kind = StoryKind.ChapterIntro;
    }
}
