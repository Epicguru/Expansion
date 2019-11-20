
using Engine;
using Engine.Sprites;
using Engine.Tiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Expansion
{
    public class TreeTile : TileDef
    {
        public TreeTile() : base(3, "Tree")
        {

        }

        public override void Draw(SpriteBatch spr, Tile tile, Chunk chunk, int localX, int localY, int z)
        {
            var treeSprite = JEngine.JContent.Get<Sprite>("NewTreeLines");
            var treeSprite2 = JEngine.JContent.Get<Sprite>("NewTreeColor");

            int x = localX * Tile.SIZE;
            int y = localY * Tile.SIZE;
            x += Tile.SIZE / 2;
            y += Tile.SIZE;

            spr.Draw(treeSprite, new Vector2(x, y), Color.White, 0f, new Vector2(treeSprite.Width / 2, treeSprite.Height), SpriteEffects.None, 0f);
            spr.Draw(treeSprite2, new Vector2(x, y), Color.DarkOliveGreen, 0f, new Vector2(treeSprite.Width / 2, treeSprite.Height), SpriteEffects.None, 0f);
        }
    }
}
