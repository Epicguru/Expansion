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
        }

        public override void Update()
        {
            Vector2 diff = (Input.MouseWorldPos - Position);
            Velocity += diff.GetNormalized() * diff.Length() * 0.5f * Time.deltaTime;

            Position += Velocity * Time.deltaTime;
        }
    }
}
