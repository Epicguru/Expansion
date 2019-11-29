
using Engine.IO;
using Engine.Tiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;

namespace Engine.Entities
{
    /// <summary>
    /// An entity is a object that exists within the game world that recieves an update every frame.
    /// All entities have position and size.
    /// Remember, when inheriting from  this class you MUST have a no-parameter constructor. Otherwise
    /// the IO system will not be able to save and load your entity.
    /// </summary>
    public abstract class Entity : ISerialized
    {
        public string Name { get; protected set; }
        public Vector2 Position = Vector2.Zero;
        public Vector2 Center { get { return Bounds.Center; } set { Position = value - Size * 0.5f; } }
        public Vector2 Size = new Vector2(32, 32);
        public Bounds Bounds { get { return new Bounds(Position, Size); } set { Position = value.Pos; Size = value.Size; } }
        public ushort ID { get; internal set; }
        public EntityCullingMode CullingMode { get; set; } = EntityCullingMode.Chunk;
        public bool AreBoundsOnScreen { get { return JEngine.Camera.WorldViewBounds.ToBounds().Overlaps(Bounds); } }
        public Chunk CurrentChunk { get; internal set; }
        public bool DoChunkParenting { get; protected set; } = true;
        public bool IsDestroyed { get; internal set; }
        protected bool SerializeName { get; set; } = true;

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

        internal void InstantRegister()
        {
            if (ID != 0)
            {
                Debug.Warn($"ID for this entity {Name} is {ID}, already registered somewhere!");
                return;
            }

            JEngine.Entities.InstantRegister(this);
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

            em.StartRegister(this);
        }

        /// <summary>
        /// Call to destroy this entity, removing it from the simulation and game world.
        /// Can be overriden to prevent destruction.
        /// </summary>
        public virtual void Destroy()
        {
            RemovePending = true;
        }

        protected virtual void OnDestroyed()
        {

        }

        internal void InternalOnDestroyed()
        {
            IsDestroyed = true;
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

        /// <summary>
        /// Called when the entity is being saved to disk.
        /// Override this to write all data that you want to save. Data must be written and read in the
        /// same order.
        /// This default implementation writes name and bounds, so it is important to still call it when overriding in a custom class.
        /// </summary>
        /// <param name="writer">The BinaryWriter to write data with. Import Engine.IO for many useful extension methods (such as writing Vector2).</param>
        public virtual void Serialize(IOWriter writer)
        {
            if(SerializeName)
                writer.Write(Name);
            writer.Write(Bounds);
        }

        /// <summary>
        /// Called when the entity is being loaded from disk.
        /// Override this to read any data that you saved when overriding the <see cref="Serialize(BinaryWriter)"/>
        /// method. Data must be written and read in the same order.
        /// </summary>
        /// <param name="reader"></param>
        public virtual void Deserialize(IOReader reader)
        {
            if(SerializeName)
                Name = reader.ReadString();
            Bounds = reader.ReadBounds();
        }
    }

    public enum EntityCullingMode : byte
    {
        None,
        Bounds,
        Chunk
    }
}
