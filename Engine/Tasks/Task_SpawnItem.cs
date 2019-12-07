using Engine.Entities;
using Engine.Items;
using Microsoft.Xna.Framework;

namespace Engine.Tasks
{
    public class Task_SpawnItem : Task
    {
        public Vector2 Offset;
        public ItemStack ItemStack;

        public Task_SpawnItem(Vector2 offset, ItemStack items) : base("Spawn Items")
        {
            Offset = offset;
            ItemStack = items;
            Description = "Spawning items";
        }

        protected override void OnStart(ActiveEntity e)
        {
            Vector2 position = e.Center + Offset;
            var dropped = new DroppedItem(ItemStack);
            dropped.Center = position;

            ItemStack.Data = null;
            Complete();
        }
    }
}
