using System;
using System.Collections.Generic;
using UnityEngine;

namespace HighNoon
{
    /// <summary>
    /// Central audio lookup. Each logical sound maps to a FOLDER of variant clips under
    /// <c>Assets/_Project/Resources/Audio/…</c>; <see cref="GetRandom"/> plays a random variant
    /// (never the same one twice in a row) so repeated sounds don't get stale. Missing folders
    /// fall back to a procedural clip from <see cref="ProcAudio"/>, so the game always has sound.
    ///
    /// Gunshots are special: the <c>Shots</c> folder is the future weapon library, so for now
    /// <see cref="GunshotDefault"/> picks ONE clean single shot (skipping distant / shell clips)
    /// instead of randomising across sniper/shotgun/etc.
    /// </summary>
    public static class AudioBank
    {
        static readonly Dictionary<string, string> Folders = new Dictionary<string, string>
        {
            { "bang",    "Audio/Bell" },
            { "death",   "Audio/BodyFall" },
            { "tension", "Audio/Hearthbeat" },
            { "victory", "Audio/Win" },
            { "defeat",  "Audio/Loose" },
            { "click",   "Audio/Click" },
            { "ambient", "Audio/Ambient" },   // reserved for future background music
        };

        static readonly Dictionary<string, AudioClip[]> _variants = new Dictionary<string, AudioClip[]>();
        static readonly Dictionary<string, int> _last = new Dictionary<string, int>();
        static readonly Dictionary<string, AudioClip> _proc = new Dictionary<string, AudioClip>();
        static readonly Dictionary<string, AudioClip[]> _weaponClips = new Dictionary<string, AudioClip[]>();
        static AudioClip _gunDefault;
        static bool _gunResolved;

        static AudioClip[] Variants(string name)
        {
            if (_variants.TryGetValue(name, out var arr)) return arr;
            arr = Folders.TryGetValue(name, out var folder)
                ? Resources.LoadAll<AudioClip>(folder)
                : Array.Empty<AudioClip>();
            _variants[name] = arr;
            return arr;
        }

        static AudioClip Cached(string name, Func<AudioClip> fallback)
        {
            if (!_proc.TryGetValue(name, out var p)) { p = fallback(); _proc[name] = p; }
            return p;
        }

        /// <summary>A random variant for <paramref name="name"/>, avoiding an immediate repeat; null if none.</summary>
        public static AudioClip Random(string name)
        {
            var arr = Variants(name);
            if (arr == null || arr.Length == 0) return null;
            if (arr.Length == 1) return arr[0];
            int last = _last.TryGetValue(name, out var l) ? l : -1;
            int i;
            do { i = UnityEngine.Random.Range(0, arr.Length); } while (i == last);
            _last[name] = i;
            return arr[i];
        }

        /// <summary>Random real variant if any exist, otherwise the (cached) procedural fallback.</summary>
        public static AudioClip GetRandom(string name, Func<AudioClip> fallback)
        {
            var c = Random(name);
            return c != null ? c : Cached(name, fallback);
        }

        /// <summary>A shot for the given weapon: a random variant of its matching clips in Shots
        /// (no immediate repeat); falls back to the procedural gunshot if the weapon has none.</summary>
        public static AudioClip GunshotFor(WeaponDef weapon, Func<AudioClip> fallback)
        {
            if (weapon == null) weapon = Weapons.Default;
            if (!_weaponClips.TryGetValue(weapon.Key, out var arr))
            {
                var all = Resources.LoadAll<AudioClip>("Audio/Shots");
                var list = new System.Collections.Generic.List<AudioClip>();
                foreach (var c in all)
                {
                    var n = c.name.ToLowerInvariant();
                    foreach (var m in weapon.Match) if (n.Contains(m)) { list.Add(c); break; }
                }
                arr = list.ToArray();
                _weaponClips[weapon.Key] = arr;
            }
            if (arr.Length == 0) return Cached("gunshot", fallback);
            if (arr.Length == 1) return arr[0];
            string key = "w:" + weapon.Key;
            int last = _last.TryGetValue(key, out var l) ? l : -1;
            int i;
            do { i = UnityEngine.Random.Range(0, arr.Length); } while (i == last);
            _last[key] = i;
            return arr[i];
        }

        /// <summary>The default weapon's single shot (one clean close shot from the Shots library).</summary>
        public static AudioClip GunshotDefault(Func<AudioClip> fallback)
        {
            if (!_gunResolved)
            {
                _gunResolved = true;
                _gunDefault = PickClose(Resources.LoadAll<AudioClip>("Audio/Shots"));
            }
            return _gunDefault != null ? _gunDefault : Cached("gunshot", fallback);
        }

        static AudioClip PickClose(AudioClip[] arr)
        {
            if (arr == null || arr.Length == 0) return null;
            foreach (var pref in new[] { "single-pistol", "pistol", "single", "eagle" })
                foreach (var c in arr)
                {
                    var n = c.name.ToLowerInvariant();
                    if (n.Contains(pref) && !IsAmbientShot(n)) return c;
                }
            foreach (var c in arr)
                if (!IsAmbientShot(c.name.ToLowerInvariant())) return c;
            return arr[0];
        }

        static bool IsAmbientShot(string n) => n.Contains("distance") || n.Contains("long") || n.Contains("shell");

        public static int VariantCount(string name) => Variants(name)?.Length ?? 0;

        /// <summary>Forget cached lookups (after importing/changing samples in the editor).</summary>
        public static void Clear()
        {
            _variants.Clear(); _last.Clear(); _weaponClips.Clear(); _gunResolved = false; _gunDefault = null;
        }
    }
}
