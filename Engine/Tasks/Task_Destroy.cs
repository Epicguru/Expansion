using Engine.Entities;

namespace Engine.Tasks
{
    /// <summary>
    /// This task causes an entity to be destroyed.
    /// </summary>
    public sealed class Task_Destroy : Task
    {
        public Task_Destroy() : base("Destroy Entity")
        {
            Description = "Deleting self.";
        }

        protected override void Update(ActiveEntity e)
        {
            e.Destroy();
            if (!e.RemovePending)
            {
                Debug.Warn($"Destroy task ran on entity {e} but it's removal from world is not pending... Is the entity a destructible type, or is it in some other way prevented from being destroyed?");
            }

            Complete();
        }
    }
}
