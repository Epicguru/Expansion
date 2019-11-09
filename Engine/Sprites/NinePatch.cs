using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
        public int MinWidth { get { return LeftSize + RightSize; } }
        public int MinHeight { get { return BottomSize + TopSize; } }

        public NinePatch(Sprite[] parts)
        {
            if(parts.Length != 9)
            {
                throw new Exception($"Expected 9 parts for NinePatch, got {parts.Length}.");
            }
            for(int i  = 0; i < 9; i++)
            {
                Sprite sprite = parts[i];

                if(sprite == null)
                {
                    throw new SpriteException($"NinePatch part {i} is null."); 
                }
            }

            Parts = parts;
        }

        public NinePatch(Sprite sprite, Rectangle middleBounds) : this(sprite.Texture, sprite.Bounds, new Rectangle(middleBounds.Location + sprite.Bounds.Location, middleBounds.Size))
        {

        }

        public NinePatch(Texture2D texture, Rectangle totalBounds, Rectangle middleBounds)
        {
            if (texture == null)
                throw new SpriteException("Cannot create new NinePatch using texture consutructor if the texture is null.");

            if (!totalBounds.IsPositive() || !middleBounds.IsPositive())
                throw new SpriteException("Invalid bounds passed into NinePatch consutructor.");

            Debug.Assert(middleBounds.Right == middleBounds.X + middleBounds.Width, "Rect check has failed...");

            Sprite[] parts = new Sprite[9];
            parts[0] = new Sprite($"{texture.Name}_NP_TopLeft", texture, new Rectangle(totalBounds.X, totalBounds.Y, middleBounds.X - totalBounds.X, middleBounds.Y - totalBounds.Y));
            parts[1] = new Sprite($"{texture.Name}_NP_TopMiddle", texture, new Rectangle(middleBounds.X, totalBounds.Y, middleBounds.Width, middleBounds.Y - totalBounds.Y));
            parts[2] = new Sprite($"{texture.Name}_NP_TopRight", texture, new Rectangle(middleBounds.Right, totalBounds.Y, totalBounds.Right - middleBounds.Right, middleBounds.Y - totalBounds.Y));

            parts[3] = new Sprite($"{texture.Name}_NP_MiddleLeft", texture, new Rectangle(totalBounds.X, middleBounds.Y, middleBounds.X - totalBounds.X, middleBounds.Height));
            parts[4] = new Sprite($"{texture.Name}_NP_MiddleMiddle", texture, new Rectangle(middleBounds.X, middleBounds.Y, middleBounds.Width, middleBounds.Height));
            parts[5] = new Sprite($"{texture.Name}_NP_MiddleRight", texture, new Rectangle(middleBounds.Right, middleBounds.Y, totalBounds.Right - middleBounds.Right, middleBounds.Height));

            parts[6] = new Sprite($"{texture.Name}_NP_BottomLeft", texture, new Rectangle(totalBounds.X, middleBounds.Bottom, middleBounds.X - totalBounds.X, totalBounds.Bottom - middleBounds.Bottom));
            parts[7] = new Sprite($"{texture.Name}_NP_BottomMiddle", texture, new Rectangle(middleBounds.X, middleBounds.Bottom, middleBounds.Width, totalBounds.Bottom - middleBounds.Bottom));
            parts[8] = new Sprite($"{texture.Name}_NP_BottomRight", texture, new Rectangle(middleBounds.Right, middleBounds.Bottom, totalBounds.Right - middleBounds.Right, totalBounds.Bottom - middleBounds.Bottom));

            this.Parts = parts;
        }

        public void Draw(SpriteBatch spr, Rectangle position, Color color)
        {
            if (position.Width < MinWidth)
                position.Width = MinWidth;
            if (position.Height < MinHeight)
                position.Height = MinHeight;

            int midWidth = position.Width - MinWidth;
            int midHeight = position.Height - MinHeight;

            Vector2 pos = position.Location.ToVector2();
            Point posI = pos.ToPoint();

            // Top left.
            spr.Draw(Parts[0], pos, color);

            // Top center.
            if (midWidth != 0)
                spr.Draw(Parts[1], new Rectangle(posI.X + LeftSize, posI.Y, midWidth, TopSize), color);

            // Top right.
            spr.Draw(Parts[2], pos + new Vector2(midWidth + LeftSize, 0), color);

            // Mid left.
            if(midHeight != 0)
                spr.Draw(Parts[3], new Rectangle(posI.X, posI.Y + TopSize, LeftSize, midHeight), color);

            // Mid mid.
            if(midWidth != 0 && midHeight != 0)
                spr.Draw(Parts[4], new Rectangle(posI.X + LeftSize, posI.Y + TopSize, midWidth, midHeight), color);

            // Mid right.
            if(midHeight != 0)
                spr.Draw(Parts[5], new Rectangle(posI.X + LeftSize + midWidth, posI.Y + TopSize, RightSize, midHeight), color);

            // Bottom left.
            spr.Draw(Parts[6], pos + new Vector2(0, TopSize + midHeight), color);

            // Bottom mid.
            if(midWidth != 0)
                spr.Draw(Parts[7], new Rectangle(posI.X + LeftSize, posI.Y + midHeight + TopSize, midWidth, BottomSize), color);

            // Bottom right.
            spr.Draw(Parts[8], pos + new Vector2(LeftSize + midWidth, TopSize + midHeight), color);
        }
    }
}
