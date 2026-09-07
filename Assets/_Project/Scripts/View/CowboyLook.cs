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

        /// <summary>Plain cowboy in the given shirt color (used for PvP/Coop team tinting).</summary>
        public static CowboyLook Basic(Color shirt) => new CowboyLook { Shirt = shirt };

        public static CowboyLook Player() => new CowboyLook
        {
            Shirt = new Color(0.82f, 0.62f, 0.36f),
            Accent = new Color(0.80f, 0.20f, 0.18f),
            Chest = Accessory.Bandana,
        };

        public static CowboyLook Player2() => new CowboyLook
        {
            Shirt = new Color(0.45f, 0.62f, 0.85f),
            Accent = new Color(0.95f, 0.82f, 0.32f),
            Chest = Accessory.Bandana,
        };

        public static CowboyLook Enemy() => new CowboyLook
        {
            Shirt = new Color(0.78f, 0.32f, 0.30f),
            HatColor = new Color(0.12f, 0.12f, 0.13f),
            Accent = new Color(0.16f, 0.16f, 0.18f),
            Chest = Accessory.Bandana,
        };
    }
}
