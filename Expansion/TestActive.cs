
using Engine;
using Engine.Entities;
using Engine.Tiles;
using Microsoft.Xna.Framework;

namespace Expansion
{
    public class TestActive : ActiveEntity
    {
        public TestActive() : base("Test Active Entity")
        {
            Size = new Vector2(5, 5);
        }

        public override void ActiveUpdate()
        {

        }
    }
}
