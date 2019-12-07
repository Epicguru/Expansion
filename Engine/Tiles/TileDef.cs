using Engine.Sprites;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Engine.Tiles
{
    public class TileDef
    {
        public static TileDef Get(byte ID)
        {
            if (ID == 0)
                return null;

            return defs[ID];
        }

        public static void Register(TileDef def)
        {
            if (def == null)
            {
                Debug.Error("Tile def is null in register.");
                return;
            }

            if (def.ID == 0)
            {
                Debug.Error("Tile Def ID cannot be zero.");
                return;
            }

            defs[def.ID] = def;
            Debug.Trace($"Registered tile {def.Name} for ID {def.ID}.");
        }

        private static TileDef[] defs = new TileDef[byte.MaxValue + 1];

        /// <summary>
        /// The vertical and horizontal size of the tile in pixels. Same as <see cref="Tile.SIZE"/>.
        /// </summary>
        public const int SIZE = 32;
        public byte ID { get; }
        public string Name { get; protected set; }
        /// <summary>
        /// Default value 0.
        /// Gets or sets the radius, in tiles, in which placement or removal of this tile will result in a
        /// chunk redraw. For example, when set to zero (default) then when this tile updates only the chunk that it
        /// is in is redrawn. When set to 1, if the tile is on the edge of the chunk the neighbouring chunk is also redrawn.
        /// If set to 2, if the tile is on the edge or 1 tile away from the edge the neighbouring chunk is also redrawn.
        /// </summary>
        public byte RedrawRadius { get; protected set; } = 0;
        public Sprite BaseSprite;

        public TileDef(byte id, string name)
        {
            this.ID = id;
            this.Name = name;
        }

        public virtual bool IsWalkable(Tile tile, int worldX, int worldY)
        {
            return false;
        }

        public virtual float GetWalkCost(Tile tile, int worldX, int worldY)
        {
            return 1f;
        }

        public virtual void Draw(SpriteBatch spr, Tile tile, Chunk chunk, int localX, int localY, int z)
        {
            if(BaseSprite != null)
                spr.Draw(BaseSprite, new Rectangle(localX * SIZE, localY * SIZE, SIZE, SIZE), ColorCache.GetColor(tile.ColorRef));
        }

        public virtual void UponPlaced(ref Tile tile, Chunk c, int localX, int localY, int z, bool fromLoad)
        {

        }

        public virtual void UponRemoved(ref Tile oldTile, Chunk c, int localX, int localY, int z, bool fromUnload)
        {

        }
    }
}
