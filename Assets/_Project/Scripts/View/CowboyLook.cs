using UnityEngine;

namespace HighNoon
{
    public enum HatStyle { Cowboy, Wide, Sombrero, Bowler }
    public enum Accessory { None, Bandana, Vest, Poncho, Badge }
    public enum FacialHair { None, Mustache, Beard }

    /// <summary>
    /// Appearance spec for a cowboy sprite so different duelists look distinct
    /// (hat shape, chest accessory, facial hair, colors). Consumed by <see cref="CowboyArt"/>.
    /// </summary>
    public class CowboyLook
    {
        public Color Shirt    = new Color(0.80f, 0.60f, 0.35f);
        public Color HatColor = new Color(0.23f, 0.16f, 0.11f);
        public Color Skin     = new Color(0.87f, 0.66f, 0.44f);
        public Color Accent   = new Color(0.80f, 0.22f, 0.20f);
        public HatStyle HatType = HatStyle.Cowboy;
        public Accessory Chest  = Accessory.None;
        public FacialHair Facial = FacialHair.None;

        /// <summary>Optional id of a real illustration in <see cref="CowboyCatalog"/>. When set (and
        /// the PNG loads) <see cref="DuelistView"/> renders that sprite instead of procedural pixels;
        /// the color fields above then only tint dialog/name accents.</summary>
        public string CharacterId;

        /// <summary>Stable key for <see cref="CowboyArt"/> frame cache.</summary>
        public string CacheKey()
        {
            Color32 s = Shirt, h = HatColor, k = Skin, a = Accent;
            return $"{s.r:x2}{s.g:x2}{s.b:x2}_{h.r:x2}{h.g:x2}{h.b:x2}_{k.r:x2}{k.g:x2}{k.b:x2}_{a.r:x2}{a.g:x2}{a.b:x2}_{(int)HatType}_{(int)Chest}_{(int)Facial}";
        }

        /// <summary>Plain cowboy in the given shirt color (used for PvP/Coop team tinting).</summary>
        public static CowboyLook Basic(Color shirt) => new CowboyLook { Shirt = shirt };

        public static CowboyLook Player() => new CowboyLook
        {
            Shirt = new Color(0.82f, 0.62f, 0.36f),
            Accent = new Color(0.80f, 0.20f, 0.18f),
            Chest = Accessory.Bandana,
            CharacterId = "dusty_hart",
        };

        public static CowboyLook Player2() => new CowboyLook
        {
            Shirt = new Color(0.45f, 0.62f, 0.85f),
            Accent = new Color(0.95f, 0.82f, 0.32f),
            Chest = Accessory.Bandana,
            CharacterId = "rio_vela",
        };

        public static CowboyLook Enemy() => new CowboyLook
        {
            Shirt = new Color(0.78f, 0.32f, 0.30f),
            HatColor = new Color(0.12f, 0.12f, 0.13f),
            Accent = new Color(0.16f, 0.16f, 0.18f),
            Chest = Accessory.Bandana,
            CharacterId = "black_calhoun",
        };

        /// <summary>
        /// Same gang, different silhouette — hat / chest / facial / shirt shift so a 2v2
        /// partner is not a clone of <paramref name="source"/> (PvE co-op and Coop bots).
        /// </summary>
        public static CowboyLook Partner(CowboyLook source)
        {
            if (source == null) return Enemy();
            return new CowboyLook
            {
                Shirt = new Color(
                    Mathf.Clamp01(source.Shirt.r * 0.72f),
                    Mathf.Clamp01(source.Shirt.g * 0.88f),
                    Mathf.Clamp01(source.Shirt.b * 1.08f)),
                HatColor = Color.Lerp(source.HatColor, Color.black, 0.22f),
                Skin = source.Skin,
                Accent = source.Accent,
                HatType = (HatStyle)(((int)source.HatType + 1) % 4),
                Chest = source.Chest == Accessory.Vest ? Accessory.Bandana
                      : source.Chest == Accessory.Bandana ? Accessory.Poncho
                      : source.Chest == Accessory.Poncho ? Accessory.Vest
                      : source.Chest == Accessory.Badge ? Accessory.Vest
                      : Accessory.Vest,
                Facial = source.Facial == FacialHair.Beard ? FacialHair.Mustache
                       : source.Facial == FacialHair.Mustache ? FacialHair.None
                       : FacialHair.Mustache,
                // Different illustration from the source so a 2v2 pair isn't two identical cowboys.
                CharacterId = source.CharacterId == "doc_graves" ? "black_calhoun" : "doc_graves",
            };
        }
    }
}
