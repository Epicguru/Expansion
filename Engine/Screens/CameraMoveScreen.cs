using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Engine.Screens
{
    public class CameraMoveScreen : GameScreen
    {
        public CameraMoveScreen() : base("Camera Movement Screen") { }

        public override void Update()
        {
            Vector2 dir = Vector2.Zero;
            if (Input.KeyPressed(Keys.A))
            {
                dir.X -= 1;
            }
            if (Input.KeyPressed(Keys.D))
            {
                dir.X += 1;
            }
            if (Input.KeyPressed(Keys.W))
            {
                dir.Y -= 1;
            }
            if (Input.KeyPressed(Keys.S))
            {
                dir.Y += 1;
            }
            dir.SafeNormalize();

            if (Input.KeyDown(Keys.E))
            {
                JEngine.Camera.Zoom *= 1.1f;
            }
            if (Input.KeyDown(Keys.Q))
            {
                JEngine.Camera.Zoom /= 1.1f;
            }

            JEngine.Camera.Position += dir * Time.unscaledDeltaTime * (Input.KeyPressed(Keys.LeftShift) ? 4096 : 512f);
        }
    }
}
