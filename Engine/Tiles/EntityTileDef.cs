using Engine.Entities;

namespace Engine.Tiles
{
    public abstract class EntityTileDef : TileDef
    {
        public EntityTileDef(byte id, string name) : base(id, name)
        {
        }

        public abstract TileEntity CreateEntity(int worldX, int worldY, int worldZ);

        public override void UponPlaced(ref Tile tile, Chunk c, int localX, int localY, int z, bool fromLoad)
        {
            // When the tile is placed down in the world, it needs to spawn the corresponding entity.
            // When loading, this is not necessary since the entity is automatically loaded from disk.

            if (fromLoad)
                return;

            TileEntity spawned = CreateEntity(c.X * Chunk.SIZE + localX, c.Y * Chunk.SIZE + localY, z);
            if (spawned.ID == 0)
                throw new System.Exception($"TileEntity was spawned via tile placement, but it has id 0...");

            tile.EntityID = spawned.ID;
        }

        public override void UponRemoved(ref Tile oldTile, Chunk c, int localX, int localY, int z, bool fromUnload)
        {
            // Remove corresponding entity.
            // Not necessary in unload since all tile entities are automatically saved and loaded later.
            // TODO save and load tile entities.
            if (fromUnload)
                return;

            var e = oldTile.Entity;
            if(e == null)
            {
                Debug.Error($"Tile in chunk {c} at local {localX}, {localY} is a entity-tile of type {this.GetType().FullName}. It has been removed, but the corresponding TileEntity could not be found! Entity ID {oldTile.EntityID}.");
                return;
            }

            // Flag for destruction.
            e.RemovePending = true;
        }
    }
}
