using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.UI
{
    public static class UI
    {
        private static SpriteBatch SB { get { return Engine.MainSpriteBatch; } }
        private static SpriteFont CurrentFont { get { return null; } } // TODO fixme, load fonts.

        public static Rectangle DrawLabel(string text, Vector2 pos, Color color, float rotation)
        {
            Vector2 size = CurrentFont.MeasureString(text);
            SB.DrawString(CurrentFont, text ?? "null", pos, color, rotation, Vector2.Zero, 1f, SpriteEffects.None, 0f);
            return new Rectangle(pos.ToPoint(), size.ToPoint());
        }
    }
}
