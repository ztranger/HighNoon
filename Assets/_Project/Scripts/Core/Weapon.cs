using UnityEngine;

namespace HighNoon
{
    /// <summary>One selectable weapon: a display name plus filename hints that pick its shot
    /// clips from the <c>Resources/Audio/Shots</c> library. (For now a weapon = its gunshot;
    /// visuals/stats can hang off this later.)</summary>
    public class WeaponDef
    {
        public string Key;       // stable id for saving the selection
        public string Name;      // shown in the menu
        public string[] Match;   // lower-case filename substrings that belong to this weapon
        public WeaponDef(string key, string name, params string[] match) { Key = key; Name = name; Match = match; }
    }

    /// <summary>The weapon roster (maps to the files the player dropped in <c>Shots/</c>).</summary>
    public static class Weapons
    {
        public static readonly WeaponDef[] All =
        {
            new WeaponDef("revolver",  "REVOLVER",     "pistol", "heathers"),
            new WeaponDef("deagle",    "DESERT EAGLE", "eagle"),
            new WeaponDef("shotgun",   "SHOTGUN",      "shotgun"),
            new WeaponDef("sniper",    "SNIPER",       "sniper"),
            new WeaponDef("steampunk", "STEAMPUNK",    "steampunk"),
        };

        public static int Count => All.Length;
        public static WeaponDef Get(int index) => All[Mathf.Clamp(index, 0, All.Length - 1)];

        /// <summary>Roster index of a weapon (0 if not found) — e.g. to fetch its <see cref="WeaponArt"/> icon.</summary>
        public static int IndexOf(WeaponDef def)
        {
            for (int i = 0; i < All.Length; i++) if (All[i] == def) return i;
            return 0;
        }

        /// <summary>The weapon the player picked in the menu (default = revolver).</summary>
        public static WeaponDef Selected => Get(GameSettings.SelectedWeapon);

        /// <summary>Default gun for bots / fallback.</summary>
        public static WeaponDef Default => All[0];
    }
}
