using Engine.Entities;
using Engine.Tiles;

namespace Expansion
{
    public class WindTurbineTileDef : EntityTileDef
    {
        public WindTurbineTileDef() : base(4, "Wind Turbine")
        {

        }

        public override TileEntity CreateEntity(int x, int y, int z)
        {
            return new WindTurbine(x, y, z);
        }
    }
}
