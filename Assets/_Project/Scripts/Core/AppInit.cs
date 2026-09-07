using UnityEngine;

namespace HighNoon
{
    /// <summary>App-wide runtime settings applied by every scene bootstrap.</summary>
    public static class AppInit
    {
        public static void Apply()
        {
            Application.runInBackground = true;   // keep ticking when unfocused (editor/MCP testing)
        }
    }
}
