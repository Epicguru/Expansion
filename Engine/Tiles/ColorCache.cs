using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Engine.Tiles
{
    public static class ColorCache
    {
        public static Color DefaultColor = Color.TransparentBlack;

        public static int MaxColorRef { get { return MAX_COLOURS - 1; } }
        public static int UsedColorRefCount { get { return usedCount; } }
        public static int RemainingColorRefCount { get { return MAX_COLOURS - usedCount - 1; } }

        private const int MAX_COLOURS = 256;
        private static Color[] colours = new Color[MAX_COLOURS];
        private static int usedCount = 0;
        private static Dictionary<Color, byte> colorToRef = new Dictionary<Color, byte>();

        public static Color GetColor(byte colorRef)
        {
            if (colorRef == 0)
                return DefaultColor;

            return colours[colorRef];
        }

        public static byte GetRef(Color color)
        {
            if (colorToRef.ContainsKey(color))
            {
                return colorToRef[color];
            }
            else
            {
                Debug.Warn("Color [{color}] is not stored in the cache. To get a color (and add if not present, use EnsureColor).");
                return 0;
            }
        }

        public static bool HasColor(Color color)
        {
            return colorToRef.ContainsKey(color);
        }

        public static byte EnsureColor(Color color)
        {
            if (colorToRef.ContainsKey(color))
                return colorToRef[color];
            else
                return AddColor(color);
        }

        private static byte AddColor(Color c)
        {
            if (RemainingColorRefCount == 0)
            {
                Debug.Error($"Cannot add new color {c} to cache, the max number of colors ({MaxColorRef - 1}) has already been used!");
                return 0;
            }

            byte newID = (byte)(usedCount + 1);

            colorToRef.Add(c, newID);
            colours[newID] = c;

            usedCount++;
            return newID;
        }
    }
}
