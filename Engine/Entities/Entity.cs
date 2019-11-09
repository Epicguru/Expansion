
using Engine.Tiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Engine.Entities
{
    public abstract class Entity
    {
        public string Name { get; protected set; }
        public Vector2 Position = Vector2.Zero;
        public Vector2 Size = new Vector2(32, 32);
        public Bounds Bounds { get { return new Bounds(Position, Size); } }
        public ushort ID { get; internal set; }
        public EntityCullingMode CullingMode { get; set; } = EntityCullingMode.Chunk;
        public bool AreBoundsOnScreen { get { return JEngine.Camera.WorldViewBounds.ToBounds().Overlaps(Bounds); } }
        public Chunk CurrentChunk { get; internal set; }
        public bool DoChunkParenting { get; protected set; } = true;

        internal bool RemovePending { get; set; }

        public Entity(string name, bool autoRegister = true)
        {
            this.Name = name;
            if (autoRegister)
            {
                Register();
            }
        }

        protected void Register()
        {
            Register(JEngine.Entities);
        }

        protected void Register(EntityManager em)
        {
            if(ID != 0)
            {
                Debug.Warn($"ID for this entity {Name} is {ID}, already registered somewhere!");
                return;
            }
            if(em == null)
            {
                Debug.Error("Entity manager passed into method is null, cannot register.");
                return;
            }

            em.Register(this);
        }

        public virtual void Destroy()
        {
            RemovePending = true;
        }

        protected virtual void OnDestroyed()
        {

        }

        internal void InternalOnDestroyed()
        {
            this.OnDestroyed();
        }

        public virtual void Update()
        {

        }

        public virtual void Draw(SpriteBatch spr)
        {            
            spr.Draw(JEngine.Pixel, Bounds.ToRectangle(), Color.White);
        }

        public Point GetChunkCoords()
        {
            return new Point(floor(Position.X / (Chunk.SIZE * Tile.SIZE)), floor(Position.Y / (Chunk.SIZE * Tile.SIZE)));

            int floor(float x)
            {
                if (x >= 0)
                    return (int)x;
                else
                    return (int)(x - 0.9999f);
            }
        }
    }

    public enum EntityCullingMode : byte
    {
        None,
        Bounds,
        Chunk
    }
}
