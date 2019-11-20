using Engine.Tiles;
using Microsoft.Xna.Framework;

namespace Engine.Entities
{
    /// <summary>
    /// An entity tile is an entity that is linked to a tile placed in the world.
    /// It must be paired with an <see cref="EntityTileDef"/> that is then placed in the world.
    /// It has the benifits of an entity (it constantly updates and keeps the chunk it is on loaded), but cannot move position
    /// and is in a fixed position with a fixed size.
    /// In order to place an entity tile in the world, you must set the tile which will then automatically spawn
    /// the entity.
    /// </summary>
    public abstract class TileEntity : Entity
    {
        public int TileX, TileY, TileZ;

        public TileEntity(string name, int x, int y, int z) : base(name, false)
        {
            TileX = x;
            TileY = y;
            TileZ = z;
            Position = new Vector2(x * Tile.SIZE, y * Tile.SIZE);
            Size = new Vector2(Tile.SIZE, Tile.SIZE);

            // Needs to be instantly registered to get an ID.
            base.InstantRegister();
        }

        public override void Destroy()
        {
            // Do not allow this entity to be destroyed unless the tile is broken.
            return;
        }
    }
}
