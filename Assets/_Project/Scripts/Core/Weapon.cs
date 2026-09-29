using UnityEngine;

namespace HighNoon
{
    /// <summary>One selectable weapon: a display name, how much of a foe's reserve one clean
    /// hit removes, and filename hints that pick its shot clips from <c>Resources/Audio/Shots</c>.</summary>
    public class WeaponDef
    {
        public string Key;       // stable id for saving the selection
        public string Name;      // shown in the menu
        public int Damage;       // reserve removed by one green hit (Volley / Sync)
        public string[] Match;   // lower-case filename substrings that belong to this weapon
        public WeaponDef(string key, string name, int damage, params string[] match)
        {
            Key = key; Name = name; Damage = Mathf.Max(1, damage); Match = match;
        }
    }

    /// <summary>The weapon roster (maps to the files the player dropped in <c>Shots/</c>).</summary>
    public static class Weapons
    {
        public static readonly WeaponDef[] All =
        {
            new WeaponDef("revolver",  "REVOLVER",     1, "pistol", "heathers"),
            new WeaponDef("deagle",    "DESERT EAGLE", 2, "eagle"),
            new WeaponDef("shotgun",   "SHOTGUN",      3, "shotgun"),
            new WeaponDef("sniper",    "SNIPER",       4, "sniper"),
            new WeaponDef("steampunk", "STEAMPUNK",    2, "steampunk"),
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
