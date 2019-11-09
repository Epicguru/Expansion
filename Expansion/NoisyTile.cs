
using Engine.Sprites;

namespace Expansion
{
    public class NoisyTile : TileDef
    {
        public NoisyTile(Sprite spr) : base(1, "Test Noisy Tile")
        {
            base.Sprite = spr;
        }
    }
}
