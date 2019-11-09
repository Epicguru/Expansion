using Engine.Sprites;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine
{
    public static class Extensions
    {
        public static void Draw(this SpriteBatch spr, Sprite sprite, Rectangle position, Color color)
        {
            Draw(spr, sprite, position, color, 0f, Vector2.Zero, SpriteEffects.None, 0f);
        }

        public static void Draw(this SpriteBatch spr, Sprite sprite, Vector2 position, Color color)
        {
            Draw(spr, sprite, new Rectangle((int)position.X, (int)position.Y, sprite.Bounds.Width, sprite.Bounds.Height), color, 0f, Vector2.Zero, SpriteEffects.None, 0f);
        }

        public static void Draw(this SpriteBatch spr, Sprite sprite, Rectangle position, Color color, float rotation, Vector2 origin, SpriteEffects effect, float depth)
        {
            spr.Draw(sprite.Texture, position, sprite.Bounds, color, rotation, origin, effect, depth);
        }

        /// <summary>
        /// Returns true when this rectangle has a width and height that are greater than zero.
        /// </summary>
        public static bool IsPositive(this Rectangle rect)
        {
            return rect.Width > 0 && rect.Height > 0;
        }

        /// <summary>
        /// Gets the normalized version of this vector, without modifying the actual vector.
        /// This is safer than <see cref="Vector2.Normalize"/> because it handles zero-length vectors.
        /// </summary>
        /// <returns>The normalized vector, which preserves direction but always has length of one.</returns>
        public static Vector2 GetNormalized(this Vector2 vector)
        {
            if (vector.LengthSquared() == 0)
                return Vector2.Zero;

            float length = vector.Length();

            return new Vector2(vector.X / length, vector.Y / length);
        }

        public static void SafeNormalize(this ref Vector2 vector)
        {
            vector = vector.GetNormalized();
        }

        public static Bounds ToBounds(this Rectangle rect)
        {
            return new Bounds(rect);
        }
    }
}
