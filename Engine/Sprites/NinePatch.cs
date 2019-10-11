using System;

namespace Engine.Sprites
{
    public class NinePatch
    {
        /// <summary>
        /// The parts of this NinePatch. The parts are ordered from top-left to bottom right.
        /// </summary>
        public readonly Sprite[] Parts;

        public int TopSize
        {
            get
            {
                return Parts[1].Bounds.Height;
            }
        }
        public int BottomSize
        {
            get
            {
                return Parts[7].Bounds.Height;
            }
        }
        public int LeftSize
        {
            get
            {
                return Parts[3].Bounds.Width;
            }
        }
        public int RightSize
        {
            get
            {
                return Parts[5].Bounds.Width;
            }
        }

        public NinePatch(Sprite[] parts)
        {
            if(parts.Length != 9)
            {
                throw new Exception($"Expected 9 parts for NinePatch, got {parts.Length}.");
            }
            foreach (var sprite in parts)
            {
                if(sprite == null)
                {
                    throw new 
                }
            }

            Parts = parts;
        }
    }
}
