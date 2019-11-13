using Engine;
using Engine.Entities;
using Engine.Tiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Expansion
{
    public class MissileEntity : Entity
    {
        public float Rotation;

        public MissileEntity() : base("Missile")
        {
            Size = new Vector2(48, 48);
        }

        public override void Update()
        {
            Rotation += MathHelper.TwoPi * Time.deltaTime * 1f;

            if (Rand.Chance(0.01f))
            {
                new TestEntity() { Velocity = Rand.UnitCircle() * Rand.Range(2f * Tile.SIZE, 6f * Tile.SIZE), Center = this.Center };
            }

            if (Input.KeyDown(Keys.P))
                Destroy();
        }

        public override void Draw(SpriteBatch spr)
        {
            spr.Draw(BaseScreen.MissileSprite, Bounds.Center, Color.White, Rotation, BaseScreen.MissileSprite.Bounds.Size.ToVector2() * 0.5f, SpriteEffects.None, 0f);
            base.Draw(spr);
        }
    }
}
