using Engine;
using Engine.Sprites;
using Engine.Tiles;

namespace Expansion
{
    public class TestLinkedTile : LinkedTile
    {
        public TestLinkedTile() : base(2, "Test Linked")
        {
            base.Sprites = LoadAll("Wall_");
            base.RedrawRadius = 1;
        }

        private Sprite[] LoadAll(string prefix)
        {
            var c = JEngine.JContent;
            Sprite[] spr = new Sprite[16];
            spr[0] = c.Load<Sprite>(prefix + "0000");
            spr[1] = c.Load<Sprite>(prefix + "0001");
            spr[2] = c.Load<Sprite>(prefix + "0010");
            spr[3] = c.Load<Sprite>(prefix + "0011");
            spr[4] = c.Load<Sprite>(prefix + "0100");
            spr[5] = c.Load<Sprite>(prefix + "0101");
            spr[6] = c.Load<Sprite>(prefix + "0110");
            spr[7] = c.Load<Sprite>(prefix + "0111");
            spr[8] = c.Load<Sprite>(prefix + "1000");
            spr[9] = c.Load<Sprite>(prefix + "1001");
            spr[10] = c.Load<Sprite>(prefix + "1010");
            spr[11] = c.Load<Sprite>(prefix + "1011");
            spr[12] = c.Load<Sprite>(prefix + "1100");
            spr[13] = c.Load<Sprite>(prefix + "1101");
            spr[14] = c.Load<Sprite>(prefix + "1110");
            spr[15] = c.Load<Sprite>(prefix + "1111");

            return spr;
        }
    }
}
