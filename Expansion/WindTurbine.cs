using Engine;
using Engine.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Expansion
{
    public class WindTurbine : TileEntity
    {
        private float Rotation;

        public WindTurbine(int x, int y, int z) : base("Wind Turbine", x, y, z)
        {
        }

        public override void Update()
        {
            Rotation += Time.deltaTime * MathHelper.TwoPi * 2f; // Two rotations per second.
        }

        public override void Draw(SpriteBatch spr)
        {
            var sprite = BaseScreen.TreeLines;
            spr.Draw(sprite, Center, Color.White, Rotation, sprite.Bounds.Size.ToVector2() * 0.5f, SpriteEffects.None, 0f);
        }
    }
}
