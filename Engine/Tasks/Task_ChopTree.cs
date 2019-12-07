using Engine.Entities;
using Engine.Items;
using Engine.Tiles;
using Microsoft.Xna.Framework;

namespace Engine.Tasks
{
    public class Task_ChopTree : CompoundTask
    {
        public int X, Y;

        public Task_ChopTree(int tx, int ty) : base("Chop Tree")
        {
            Description = "Chopping tree down";
            X = tx;
            Y = ty;

            Tile tile = JEngine.TileMap.GetTile(tx, ty, 1);
            if (tile.IsBlank || tile.Name != "Tree")
            {
                Cancel(null);
                return;
            }

            base.AddTask(new Task_MoveTo(tx, ty));
            base.AddTask(new Task_Wait(5f));
            base.AddTask(new Task_SetTile(tx, ty, 1, Tile.Blank));
            base.AddTask(new Task_SpawnItem(Vector2.Zero, new ItemStack(1, Rand.Range(10, 21))));
        }

        protected override void Update(ActiveEntity e)
        {
            base.Update(e);

            if(CurrentSubTask != null && CurrentSubTask is Task_Wait)
            {
                Loop.AddDrawAction((spr) =>
                {
                    GameUtils.DrawProgressBar(spr, e.Center + new Vector2(0, -16), CurrentSubTask.Progress);
                });
            }

            Tile tile = JEngine.TileMap.GetTile(X, Y, 1, false, false);
            if (tile.ID != 3)
            {
                Cancel(e);
            }
        }
    }
}
