﻿using Microsoft.Xna.Framework;

namespace Engine
{
    public static class Rand
    {
        private static System.Random rand = new System.Random();
        private const int MAX_RAND = int.MaxValue / 2;

        /// <summary>
        /// Reseeds the internal random number generator with this new seed value. This can be used to create
        /// predictable random values.
        /// </summary>
        /// <param name="seed">The seed string. <see cref="string.GetHashCode"/> is used to get a seed integer.</param>
        public static void Reseed(string seedString)
        {
            Reseed(seedString.GetHashCode());
        }

        /// <summary>
        /// Reseeds the internal random number generator with this new seed value. This can be used to create
        /// predictable random values.
        /// </summary>
        /// <param name="seed">The seed number.</param>
        public static void Reseed(int seed)
        {
            rand = new System.Random(seed);
        }

        /// <summary>
        /// Gets a random number between 0.0 and 1.0, inclusive.
        /// </summary>
        public static float Value
        {
            get
            {
                return (float)rand.Next(MAX_RAND) / (MAX_RAND - 1);
            }
        }

        /// <summary>
        /// Gets a random boolean: either true or false, each having a 50% probability.
        /// </summary>
        public static bool Boolean { get { return Value >= 0.5f; } }

        /// <summary>
        /// Gets a random number within the specified range.
        /// Both a and b are inclusive.
        /// </summary>
        /// <param name="a">The first bound.</param>
        /// <param name="b">The second bound.</param>
        /// <returns>A random number between the two supplied numbers. Inclusive.</returns>
        public static float Range(float a, float b)
        {
            return MathHelper.Lerp(a, b, Value);
        }

        /// <summary>
        /// Gets a random integer within the specified range.
        /// The upper bound is excluded! For example, calling <code>Range(1, 5)</code> can return any number
        /// from 1 to 4, inclusive. Similarly, calling <code>Range(12, 2)</code> can return any number from 2
        /// to 11, inclusive.
        /// </summary>
        /// <param name="a">The first bound.</param>
        /// <param name="b">The second bound.</param>
        /// <returns>A random integer between the two bounds, excluding the upper bound.</returns>
        public static int Range(int a, int b)
        {
            if (a == b)
                return a;

            int min = a < b ? a : b;
            int max = a > b ? a : b;

            return rand.Next(min, max);
        }

        /// <summary>
        /// Gets a true of false value based on a probability to give true.
        /// For example, calling <code>Chance(0.9)</code> has a 90% chance to return true, and a 10% chance
        /// of returning false;
        /// </summary>
        /// <param name="probabilty">The probabilty value of returning true. Should be in the range 0 to 1.</param>
        /// <returns>True of false based on the random probability.</returns>
        public static bool Chance(float probabilty)
        {
            if(probabilty <= 0f)
            {
                return false;
            }
            if(probabilty >= 1f)
            {
                return true;
            }

            return Value <= probabilty;
        }

        /// <summary>
        /// Returns a random point on the circumference of a circle of radius 1.
        /// </summary>
        public static Vector2 UnitCircle()
        {
            float angle = Range(0f, (float)System.Math.PI * 2f);
            float x = (float)System.Math.Cos(angle);
            float y = (float)System.Math.Sin(angle);

            return new Vector2(x, y);
        }
    }
}
