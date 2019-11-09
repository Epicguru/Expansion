
using SharpNoise.Modules;
using System;
using Float = System.Single;

namespace Engine.MathUtils
{
    public class Noise : IDisposable
    {
        public int Seed { get; }

        private Perlin Perlin
        {
            get
            {
                if (_perlin == null)
                {
                    _perlin = new Perlin() { Seed = this.Seed };
                    Debug.Log($"Initialized perlin noise with Freq: {_perlin.Frequency}, Octave Count: {_perlin.OctaveCount} using persistance: {_perlin.Persistence}.");
                }

                return _perlin;
            }
            set
            {
                _perlin = value;
            }
        }
        private Perlin _perlin;

        public Noise(int seed)
        {
            Seed = seed;
        }

        public void SetPerlinPersistence(Float persistence)
        {
            Perlin.Persistence = persistence;
        }

        public Float GetPerlin(Float x, Float y)
        {
            return (Float)Perlin.GetValue(x, y, 0);
        }

        public Float GetPerlin(Float x, Float y, Float z)
        {
            return (Float)Perlin.GetValue(x, y, z);
        }

        public void Dispose()
        {
            Perlin = null;
        }
    }
}
