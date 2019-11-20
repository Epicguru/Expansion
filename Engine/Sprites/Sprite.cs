using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.Sprites
{
    public class Sprite
    {
        public string Name { get; private set; }
        public bool HasTexture { get { return Texture != null; } }
        public Texture2D Texture { get; set; }
        public Rectangle Bounds
        {
            get
            {
                return _bounds;
            }
            set
            {
                if (value.Width <= 0)
                    throw new SpriteException($"Bounds width cannot be zero or less. ({value.Width})");
                if(value.Height <= 0)
                    throw new SpriteException($"Bounds height cannot be zero or less. ({value.Height})");

                if(Texture != null)
                {
                    if (!Texture.Bounds.Contains(value))
                    {
                        throw new SpriteException($"Bounds [{value}] does not lie within the texture bounds [{Texture.Bounds}].");
                    }
                }

                _bounds = value;
            }
        }
        public Vector2 BoundsCenter { get { return Bounds.Center.ToVector2(); } }

        private Rectangle _bounds;
        public int Width { get { return Bounds.Width; } }
        public int Height { get { return Bounds.Height; } }

        public Sprite(string name, Texture2D texture, Rectangle bounds)
        {
            this.Name = name;
            if (Name == null)
                Name = string.Empty;
            this.Texture = texture;
            this.Bounds = bounds;
        }

        public Sprite(Texture2D texture) : this(texture, texture?.Name)
        {

        }

        public Sprite(Texture2D texture, string name)
        {
            this.Name = name;
            this.Texture = texture;
            this.Bounds = texture.Bounds;
        }

        public Sprite(Rectangle bounds, string name)
        {
            this.Name = name;
            this.Bounds = bounds;
        }
    }
}
