using Engine.Entities;
using Engine.Tiles;

namespace Engine.Tasks
{
    public class Task_SetTile : Task
    {
        private Tile tile;
        private int x, y, z;

        public Task_SetTile(int x, int y, int z, Tile tile)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.tile = tile;
        }

        protected override void OnStart(ActiveEntity e)
        {
            JEngine.TileMap.SetTile(x, y, z, tile);
            Complete();
        }
    }
}
