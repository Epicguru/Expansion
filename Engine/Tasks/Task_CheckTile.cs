using Engine.Entities;
using Engine.Tiles;

namespace Engine.Tasks
{
    public class Task_CheckTile : Task
    {
        public delegate bool Check(Tile tile, int x, int y, int z);
        public Check CheckAction = null;
        public int X, Y, Z;
        public ushort ID;

        public Task_CheckTile(int x, int y, int z, ushort id)
        {
            X = x;
            Y = y;
            Z = z;
            ID = id;
        }

        protected override void OnStart(ActiveEntity e)
        {
            Tile tile = JEngine.TileMap.GetTile(X, Y, Z);
            bool basicCheck = tile.ID == ID;

            bool secondCheck = CheckAction == null ? true : CheckAction(tile, X, Y, Z);

            if(!basicCheck || !secondCheck)
            {
                Cancel(e);
            }
            else
            {
                Complete();
            }
        }
    }
}
