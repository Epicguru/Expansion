using Engine;
using Engine.Entities;
using Microsoft.Xna.Framework;

namespace Expansion
{
    public class TestEntity : Entity
    {
        public Vector2 Velocity;

        public TestEntity() : base("Test Entity")
        {
            Size = new Vector2(32, 32);
            DoChunkParenting = false;
        }

        public override void Update()
        {
            Position += Velocity * Time.deltaTime;
        }
    }
}
