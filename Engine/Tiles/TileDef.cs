using Engine.Sprites;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

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
        public Sprite Sprite; // TODO add sprite variants, corners, backgrounds etc.

        public TileDef(byte id, string name)
        {
            this.ID = id;
            this.Name = name;
        }

        public virtual void Draw(SpriteBatch spr, Tile tile, Chunk chunk, int localX, int localY)
        {
            spr.Draw(Sprite, new Rectangle(localX * SIZE, localY * SIZE, SIZE, SIZE), ColorCache.GetColor(tile.ColorRef));
        }
    }
}
