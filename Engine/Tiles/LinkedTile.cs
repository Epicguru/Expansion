using Engine.Sprites;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.Tiles
{
    public class LinkedTile : TileDef
    {
        public Sprite[] Sprites;

        public LinkedTile(byte id, string name) : base(id, name)
        {
        }

        public virtual bool ShouldConnectTo(Tile tile)
        {
            return tile.ID == this.ID;
        }

        public virtual Sprite CalculateSprite(int worldX, int worldY, int z)
        {
            if (Sprites == null || Sprites.Length != 16)
                return null;

            bool left = ShouldConnectTo(JEngine.TileMap.GetTile(worldX - 1, worldY, z, false));
            bool right = ShouldConnectTo(JEngine.TileMap.GetTile(worldX + 1, worldY, z, false));
            bool down = ShouldConnectTo(JEngine.TileMap.GetTile(worldX, worldY + 1, z, false));
            bool up = ShouldConnectTo(JEngine.TileMap.GetTile(worldX, worldY - 1, z, false));

            int index = (left ? 8 : 0) + (up ? 4 : 0) + (right ? 2 : 0) + (down ? 1 : 0);

            return Sprites[index];
        }

        public override void Draw(SpriteBatch spr, Tile tile, Chunk chunk, int localX, int localY, int z)
        {
            spr.Draw(CalculateSprite(localX + chunk.X * Chunk.SIZE, localY + chunk.Y * Chunk.SIZE, z), new Rectangle(localX * SIZE, localY * SIZE, SIZE, SIZE), ColorCache.GetColor(tile.ColorRef));
        }
    }
}
