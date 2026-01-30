using UnityEngine;

namespace FieldFriends.Palette
{
    /// <summary>
    /// Static color definitions for the Field Friends GBC palette.
    /// No pure white or black. Outlines use Muted Ink.
    /// </summary>
    public static class FieldFriendsPalette
    {
        // Core
        public static readonly Color CreamMist    = HexColor("F6F1E8"); // bg / sky
        public static readonly Color PastelMint   = HexColor("BFD8C2"); // mid environment
        public static readonly Color LavenderBlue = HexColor("9FAFD6"); // foreground / characters
        public static readonly Color MutedInk     = HexColor("4A4E69"); // outline / UI text

        // Accents
        public static readonly Color SoftPeach    = HexColor("F2B8A0");
        public static readonly Color ButterYellow = HexColor("F6E27F");
        public static readonly Color DustyRose    = HexColor("D9A5B3");
        public static readonly Color PaleTeal     = HexColor("9ED6D3");

        // Night / interiors (optional)
        public static readonly Color EveningLilac = HexColor("7D7FA6");
        public static readonly Color CoolSlate    = HexColor("5C607A");

        static Color HexColor(string hex)
        {
            ColorUtility.TryParseHtmlString("#" + hex, out Color c);
            return c;
        }
    }
}
