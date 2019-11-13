
using Engine.Tiles;
using Microsoft.Xna.Framework;

namespace Engine.Entities
{
    public class ActiveEntity : Entity
    {
        public static int GroupAvoidFrameSkip { get; set; } = 0;

        public Vector2 Velocity;
        public bool UseFixedSpeed = false;
        public float FixedSpeed = Tile.SIZE * 2f;
        public float GroupAvoidanceRange = Tile.SIZE * 0.5f;

        public ActiveEntity(string name, bool autoRegister = true) : base(name, autoRegister)
        {

        }

        public void AddForce(Vector2 force)
        {
            Velocity += force;
        }

        public override sealed void Update()
        {
            GroupAvoidFrameSkip = 5;
            Velocity = Vector2.Zero;

            if (GroupAvoidFrameSkip == 0 || (Time.frames + ID) % (ulong)GroupAvoidFrameSkip == 0)
            {
                const float SCALAR = 15f;
                var all = JEngine.Entities.GetAllInRange(Center, GroupAvoidanceRange, true);
                Vector2 selfCenter = Center;
                float rSquared = GroupAvoidanceRange * GroupAvoidanceRange;
                foreach (var e in all)
                {
                    if (e == this)
                        continue;

                    if (e is ActiveEntity)
                    {
                        Vector2 toCenter = selfCenter - e.Center;
                        if (toCenter == Vector2.Zero)
                            toCenter = Rand.UnitCircle() * 0.1f;
                        float sqrDst = toCenter.LengthSquared();
                        float p = 1f - (sqrDst / rSquared);

                        Vector2 force = toCenter.GetNormalized() * SCALAR * p * (GroupAvoidFrameSkip + 1);
                        Velocity += force;
                    }
                }
            }
            
            ActiveUpdate();

            Position += (UseFixedSpeed ? Velocity.GetNormalized() * FixedSpeed : Velocity) * Time.deltaTime;
        }

        public virtual void ActiveUpdate()
        {

        }
    }
}
