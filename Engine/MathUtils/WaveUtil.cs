using Microsoft.Xna.Framework;
using System;

namespace Engine.MathUtils
{
    public static class WaveUtil
    {
        /// <summary>
        /// Gets a time-based sine wave value provided a frequency, an amplitude and a phase difference.
        /// </summary>
        /// <param name="frequency">The frequency, in hertz, of the wave.</param>
        /// <param name="amplitude">The amplitude (max peak size) of the wave. Defaults to 1.</param>
        /// <param name="offset">The phase difference of the wave, measured in radians. Defaults to zero.</param>
        /// <returns>The value of the current position in the wave. Will be in the range -amplitude to amplitude.</returns>
        public static float Wave(float frequency, float amplitude = 1f, float offset = 0f)
        {
            return (float)Math.Sin(Time.time * Math.PI * 2f * frequency + offset) * amplitude;
        }
    }
}
