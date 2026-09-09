using UnityEngine.SceneManagement;

namespace HighNoon
{
    /// <summary>
    /// Tiny scene navigator. Scene names live here so bootstraps and
    /// <see cref="DuelManager"/> don't each hard-code <c>LoadScene</c>.
    /// </summary>
    public static class DuelFlow
    {
        public static void Menu() => SceneManager.LoadScene("MainMenu");
        public static void Map() => SceneManager.LoadScene("Map");
        public static void Duel() => SceneManager.LoadScene("Duel");
        public static void Tutorial() => SceneManager.LoadScene("Tutorial");

        public static void Story(StoryKind kind)
        {
            HighNoon.Story.Kind = kind;
            SceneManager.LoadScene("Story");
        }
    }
}
