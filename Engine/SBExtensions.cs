using Engine.Sprites;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine
{
    internal static class SBExtensions
    {
        public static void Draw(this SpriteBatch spr, Sprite sprite, Vector2 position, Color color)
        {
            Draw(spr, sprite, position, color, 0f, Vector2.Zero, SpriteEffects.None, 0f);
        }

        public static void Draw(this SpriteBatch spr, Sprite sprite, Vector2 position, Color color, float rotation, Vector2 origin, SpriteEffects effect, float depth)
        {
            spr.Draw(sprite.Texture, new Rectangle(position.ToPoint(), sprite.Bounds.Size), sprite.Bounds, color, rotation, origin, effect, depth);
        }
    }
}
