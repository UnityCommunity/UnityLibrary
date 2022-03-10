using UnityEngine;
using System.Globalization;

// e.g. "#FFFFFF".ToColor()

namespace UnityLibrary
{
    public static class StringExtensions
    {
        public static Color ToColor(this string hex) {
            hex = hex.Replace("0x", "");
            hex = hex.Replace("#", "");

            byte a = 255;
            byte r = byte.Parse(hex.Substring(0,2), NumberStyles.HexNumber);
            byte g = byte.Parse(hex.Substring(2,2), NumberStyles.HexNumber);
            byte b = byte.Parse(hex.Substring(4,2), NumberStyles.HexNumber);

            if (hex.Length == 8)
                a = byte.Parse(hex.Substring(6,2), NumberStyles.HexNumber);

            return new Color32(r,g,b,a);
        }
    }
}
