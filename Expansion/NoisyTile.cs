
using Engine.Sprites;
using Engine.Tiles;

namespace Expansion
{
    public class NoisyTile : TileDef
    {
        public NoisyTile(Sprite spr) : base(1, "Test Noisy Tile")
        {
            base.BaseSprite = spr;
        }
    }
}
