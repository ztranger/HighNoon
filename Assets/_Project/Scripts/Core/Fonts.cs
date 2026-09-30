using UnityEngine;

namespace HighNoon
{
    /// <summary>
    /// One place every UI text gets its font, so a Cyrillic-capable TTF can replace the built-in
    /// font in a single drop. Put an open-license TTF at <c>Resources/Fonts/AppFont.ttf</c>
    /// (e.g. Noto Sans / PT Sans / Roboto); until then this falls back to the built-in font, which
    /// may render Cyrillic via OS fallback but is not guaranteed on Android — verify on device.
    /// See docs/LOCALIZATION.md §7.
    /// </summary>
    public static class Fonts
    {
        static Font _default;

        public static Font Default
        {
            get
            {
                if (_default != null) return _default;
                _default = Resources.Load<Font>("Fonts/AppFont");
                if (_default == null) _default = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                if (_default == null) _default = Resources.GetBuiltinResource<Font>("Arial.ttf");
                return _default;
            }
        }
    }
}
