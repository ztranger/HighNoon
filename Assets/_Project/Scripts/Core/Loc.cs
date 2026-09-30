using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace HighNoon
{
    /// <summary>
    /// Lightweight, code-first localization. Strings live in <c>Resources/Locale/&lt;code&gt;.txt</c>
    /// as <c>key = value</c> lines (see docs/LOCALIZATION.md). Look up with <see cref="T"/>;
    /// a missing key falls back to English, then to a visible <c>⟪key⟫</c> marker so untranslated
    /// text is obvious in the editor and on device.
    /// </summary>
    public static class Loc
    {
        public const string DefaultLocale = "en";

        /// <summary>Locales that ship with the game. Add a code here + its .txt to introduce one.</summary>
        public static readonly string[] Registered = { "en", "ru" };

        static string _locale;
        static Dictionary<string, string> _current;
        static Dictionary<string, string> _fallback; // always DefaultLocale
        static bool _ready;

        public static string Locale { get { Ensure(); return _locale; } }
        public static IReadOnlyList<string> Available => Registered;

        /// <summary>Load the saved (or auto-detected) locale. Safe to call repeatedly.</summary>
        public static void Ensure()
        {
            if (_ready) return;
            _ready = true;
            _fallback = Load(DefaultLocale);
            string code = GameSettings.Locale;
            if (!IsRegistered(code)) code = Detect();
            Apply(code);
        }

        /// <summary>Switch locale and persist it. The caller must reload/rebuild the scene to
        /// redraw code-built UI (see docs/LOCALIZATION.md §9.5).</summary>
        public static void SetLocale(string code)
        {
            if (!IsRegistered(code)) code = DefaultLocale;
            _ready = true;
            if (_fallback == null) _fallback = Load(DefaultLocale);
            Apply(code);
            GameSettings.Locale = code;
        }

        public static string T(string key)
        {
            Ensure();
            return Resolve(key) ?? Marker(key);
        }

        public static string T(string key, params object[] args)
        {
            string template = T(key);
            if (args == null || args.Length == 0) return template;
            try { return string.Format(template, args); }
            catch { return template; }
        }

        /// <summary>Number-aware lookup: picks the plural form for the current locale and formats
        /// with <paramref name="n"/> as <c>{0}</c> (extra args become {1}…).</summary>
        public static string Plural(string key, int n, params object[] args)
        {
            Ensure();
            string form = PluralForm(_locale, n);
            string template = Resolve(key + "." + form)
                           ?? Resolve(key + ".other")
                           ?? Resolve(key + ".one")
                           ?? Resolve(key)
                           ?? Marker(key);

            var all = new object[(args?.Length ?? 0) + 1];
            all[0] = n;
            if (args != null) for (int i = 0; i < args.Length; i++) all[i + 1] = args[i];
            try { return string.Format(template, all); }
            catch { return template; }
        }

        static string Resolve(string key)
        {
            if (_current != null && _current.TryGetValue(key, out var v)) return v;
            if (_fallback != null && _fallback.TryGetValue(key, out var f)) return f;
            return null;
        }

        static string Marker(string key) => "⟪" + key + "⟫"; // ⟪key⟫

        /// <summary>CLDR-ish plural category for the locale/number.</summary>
        static string PluralForm(string locale, int n)
        {
            n = Mathf.Abs(n);
            if (locale == "ru")
            {
                int m10 = n % 10, m100 = n % 100;
                if (m10 == 1 && m100 != 11) return "one";
                if (m10 >= 2 && m10 <= 4 && (m100 < 12 || m100 > 14)) return "few";
                return "many";
            }
            return n == 1 ? "one" : "other"; // en + default
        }

        static void Apply(string code)
        {
            _locale = code;
            _current = code == DefaultLocale ? _fallback : Load(code);
            if (_current == null || _current.Count == 0) { _locale = DefaultLocale; _current = _fallback; }
        }

        static bool IsRegistered(string code)
        {
            if (string.IsNullOrEmpty(code)) return false;
            foreach (var c in Registered) if (c == code) return true;
            return false;
        }

        static string Detect() =>
            Application.systemLanguage == SystemLanguage.Russian ? "ru" : DefaultLocale;

        static Dictionary<string, string> Load(string code)
        {
            var table = new Dictionary<string, string>(256);
            var asset = Resources.Load<TextAsset>("Locale/" + code);
            if (asset == null) return table;
            foreach (var raw in asset.text.Split('\n'))
            {
                string line = raw.Trim();
                if (line.Length == 0 || line[0] == '#' || line.StartsWith("//")) continue;
                int eq = line.IndexOf('=');
                if (eq <= 0) continue;
                string key = line.Substring(0, eq).Trim();
                if (key.Length == 0) continue;
                table[key] = Unescape(line.Substring(eq + 1).Trim());
            }
            return table;
        }

        static string Unescape(string s)
        {
            if (s.IndexOf('\\') < 0) return s;
            var sb = new StringBuilder(s.Length);
            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];
                if (c == '\\' && i + 1 < s.Length)
                {
                    char next = s[++i];
                    sb.Append(next == 'n' ? '\n' : next == 't' ? '\t' : next); // \\ → \, \x → x
                }
                else sb.Append(c);
            }
            return sb.ToString();
        }
    }
}
