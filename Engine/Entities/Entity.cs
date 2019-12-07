
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
        /// <summary>
        /// The name of this entity.
        /// </summary>
        public string Name { get; protected set; }
        /// <summary>
        /// The top left corner position of the bounds of this entity, measured in pixels. It is often more useful to read and write <see cref="Center"/> rather
        /// than using the position variable.
        /// </summary>
        public Vector2 Position = Vector2.Zero;
        /// <summary>
        /// Gets or sets the position of the center of the bounds of this entity: modifying this only changes position, not size.
        /// Measured in pixels.
        /// </summary>
        public Vector2 Center { get { return Bounds.Center; } set { Position = value - Size * 0.5f; } }
        /// <summary>
        /// This entity's current tile position: the coordinates of the tile under the center of this entity.
        /// </summary>
        public Point TilePosition { get { return JEngine.TileMap.PixelToTileCoords(Center.ToPoint()); } }
        /// <summary>
        /// The size of this entity, in pixels. The size extends from the top left corner downwards and to the right.
        /// See <see cref="Position"/> and <see cref="Center"/> for more info.
        /// </summary>
        public Vector2 Size = new Vector2(32, 32);
        /// <summary>
        /// The bounds of this entity: the rectangle that represents this entity's position and size in the game world.
        /// All measurements in pixels.
        /// </summary>
        public Bounds Bounds { get { return new Bounds(Position, Size); } set { Position = value.Pos; Size = value.Size; } }
        /// <summary>
        /// The unique ID of this entity. At any given point in the game runtime, this ID will be unique to this entity but once it is destroyed
        /// the ID is recycled.
        /// </summary>
        public ushort ID { get; internal set; }
        /// <summary>
        /// The rendering culling mode for this entity. When an entity is culled, <see cref="Draw(SpriteBatch)"/> is not called
        /// and so the entity is not drawn. This property modifies the technique used to calculate culling. It can also be 
        /// used to completely disable culling. See <see cref="EntityCullingMode"/> for more info on culling modes and
        /// <see cref="AreBoundsOnScreen"/> to work out if the bounds are currently on screen.
        /// </summary>
        public EntityCullingMode CullingMode { get; set; } = EntityCullingMode.Chunk;
        /// <summary>
        /// Returns true if the <see cref="Bounds"/> of this entity are currently visible on-screen. Note that reading this
        /// property is slightly expensive, so caching the result is ideal.
        /// </summary>
        public bool AreBoundsOnScreen { get { return JEngine.Camera.WorldViewBounds.ToBounds().Overlaps(Bounds); } }
        /// <summary>
        /// Gets the currently chunk that this entity is in. It is only ever NOT NULL if <see cref="DoChunkParenting"/>
        /// is true (it is by default). Note that even if DoChunkParenting is true, this may still return null under certain
        /// circumstances, so always perform a null check.
        /// </summary>
        public Chunk CurrentChunk { get; internal set; }
        /// <summary>
        /// When set to true (by default is true) this entity will be associated with the chunk that it is currently in.
        /// This allows for culling, faster distance checks and other advantages. However, this comes at a cost since
        /// there is an overhead created by calculating chunk parenting every frame. Disable this if you plan to have
        /// many (500+) entities that move fast and don't do pathfinding.
        /// </summary>
        public bool DoChunkParenting { get; protected set; } = true;
        /// <summary>
        /// If true it means that this entity is no longer part of the game world. It will never be updated, drawn or have
        /// any other kind of interaction with the world. If this is true, you should immediately dispose of any references to it
        /// since it needs to be garbage collected. See <see cref="Destroy"/>.
        /// </summary>
        public bool IsDestroyed { get; internal set; }
        /// <summary>
        /// When set to true the name of this entity is serialized. This should not be changed at runtime, and should remain
        /// consistent for each type of entity.
        /// </summary>
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

        /// <summary>
        /// Called once this entity has been removed from the game world. Perform any clean up or fire any events that
        /// you want, but note that this entity will no longer be updated or rendered ever again.
        /// </summary>
        protected virtual void OnDestroyed()
        {

        }

        /// <summary>
        /// Called once this entity has been added to the world simulation.
        /// </summary>
        protected virtual void OnRegistered()
        {

        }

        internal void InternalOnDestroyed()
        {
            IsDestroyed = true;
            this.OnDestroyed();
        }

        internal void InternalOnRegistered()
        {
            OnRegistered();
        }

        internal virtual void InternalUpdate()
        {
            this.Update();
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

        public override string ToString()
        {
            return $"[{ID}] {Name}: {Position}";
        }
    }

    public enum EntityCullingMode : byte
    {
        /// <summary>
        /// The entity is always drawn. No culling checks are performed. Could be bad for performance.
        /// </summary>
        None,
        /// <summary>
        /// The bounds of this entity are used to calculate weather or not an entity should be rendered.
        /// Is the most accurate for non-rotating sprites but also slowest to calculate. 
        /// Does not work well when sprites are rotated and don't match the bounds of the entity.
        /// For rotating sprites, the <see cref="EntityCullingMode.Chunk"/> mode may be a better option.
        /// </summary>
        Bounds,
        /// <summary>
        /// The entity is only rendered if the chunk it is within is being rendered. Slightly less accurate than <see cref="Bounds"/> but
        /// works much better for rotating sprites and is much faster to compute. This is the default for standard entities.
        /// </summary>
        Chunk
    }
}
