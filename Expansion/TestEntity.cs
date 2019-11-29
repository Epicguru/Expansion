using Engine;
using Engine.Entities;
using Engine.IO;
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

        public override void Serialize(IOWriter writer)
        {
            base.Serialize(writer);

            writer.Write(Velocity);
        }

        public override void Deserialize(IOReader reader)
        {
            base.Deserialize(reader);

            Velocity = reader.ReadVector2();
        }
    }
}
