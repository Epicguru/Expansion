using Engine.Entities;

namespace Engine.Tasks
{
    public class Task_CompletePath : Task
    {
        public Task_CompletePath() : base("Complete Path")
        {
            Description = "Following path.";
        }

        protected override void Update(ActiveEntity e)
        {
            if (e.CurrentPath == null || e.CurrentPath.Count == 0)
                Complete();
        }

        protected override void OnCancel(ActiveEntity e)
        {
            e.CancelPathMovement();
        }
    }
}
