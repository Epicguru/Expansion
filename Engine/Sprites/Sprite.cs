using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.Sprites
{
    public class Sprite
    {
        public string Name { get; private set; }
        public Texture2D Texture { get; set; }
        public Rectangle Bounds { get; set; }

        public Sprite(string name, Texture2D texture, Rectangle bounds)
        {
            this.Name = name;
            this.Texture = texture;
            this.Bounds = bounds;
        }
    }
}
