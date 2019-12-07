using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine
{
    public static class GameUtils
    {
        public static void DrawProgressBar(SpriteBatch spr, Vector2 position, float progress)
        {
            DrawProgressBar(spr, position, progress, 48, Color.GreenYellow, Color.DimGray);
        }

        public static void DrawProgressBar(SpriteBatch spr, Vector2 position, float progress, int width)
        {
            DrawProgressBar(spr, position, progress, width, Color.GreenYellow, Color.DimGray);
        }

        public static void DrawProgressBar(SpriteBatch spr, Vector2 position, float progress, int width, Color filled, Color notFilled)
        {
            const int BORDER = 2;
            const int HEIGHT = 8;

            Point topLeft = position.ToPoint() - new Point(width / 2, HEIGHT / 2);
            Point size = new Point(width, HEIGHT);
            int fillWidth = (int)((width - BORDER * 2) * MathHelper.Clamp(progress, 0f, 1f));

            // Border.
            spr.Draw(JEngine.Pixel, new Rectangle(topLeft, size), Color.Black);
            // Not filled.
            spr.Draw(JEngine.Pixel, new Rectangle(topLeft + new Point(BORDER, BORDER), size - new Point(BORDER * 2, BORDER * 2)), notFilled);
            // Filled.
            spr.Draw(JEngine.Pixel, new Rectangle(topLeft + new Point(BORDER, BORDER), new Point(fillWidth, HEIGHT - BORDER * 2)), filled);
        }
    }
}
