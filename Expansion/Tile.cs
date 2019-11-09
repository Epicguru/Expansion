
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Expansion
{
    public struct Tile
    {
        /// <summary>
        /// The vertical and horizontal size of the tile in pixels. Same as <see cref="TileDef.SIZE"/>.
        /// </summary>
        public const int SIZE = TileDef.SIZE;
        public byte ID { get; }
        public byte ColorRef { get; set; }
        public TileDef Def { get { return TileDef.Get(ID); } }

        public Tile(byte id, byte colorRef)
        {
            this.ID = id;
            this.ColorRef = colorRef;
        }

        public void Draw(SpriteBatch spr, Chunk chunk, int localX, int localY)
        {
            Def.Draw(spr, this, chunk, localX, localY);
        }
    }
}
