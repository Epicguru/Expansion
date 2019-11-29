using Engine.Entities;
using Engine.IO;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.Tiles
{
    public struct Tile
    {
        public static Tile Blank { get; } = new Tile(0, 0);

        /// <summary>
        /// The vertical and horizontal size of the tile in pixels. Same as <see cref="TileDef.SIZE"/>.
        /// </summary>
        public const int SIZE = TileDef.SIZE;
        public bool IsBlank { get { return ID == 0; } }
        public byte ID { get; }
        public byte ColorRef { get; set; }
        public string Name
        {
            get
            {
                if (IsBlank)
                    return "Blank";
                else
                    return Def.Name;
            }
        }
        public TileDef Def { get { return TileDef.Get(ID); } }
        public ushort EntityID;
        public TileEntity Entity { get { return JEngine.Entities.Get(EntityID) as TileEntity; } }

        public Tile(byte id, byte colorRef)
        {
            this.ID = id;
            this.ColorRef = colorRef;
            this.EntityID = 0;
        }

        public void Draw(SpriteBatch spr, Chunk chunk, int localX, int localY, int z)
        {
            Def.Draw(spr, this, chunk, localX, localY, z);
        }

        public override string ToString()
        {
            return $"ID: {ID} ({(ID == 0 ? "air" : Def.Name)}){(EntityID == 0 ? "" : ", Entity: " + Entity?.Name ?? "null" + $" ({Entity?.GetType().FullName ?? "null"})")}";
        }
    }
}
