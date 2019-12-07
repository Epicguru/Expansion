using Engine.Entities;
using System.Collections.Generic;

namespace Engine.Tasks
{
    public class TaskManager
    {
        public Task CurrentTask { get; private set; }
        public int TaskQueueLength { get { return TaskQueue?.Count ?? 0; } }

        private List<Task> TaskQueue = new List<Task>();

        public void AddTask(Task t)
        {
            Debug.Assert(t != null);

            if (TaskQueue.Contains(t))
            {
                Debug.Error($"Task {t} is already in the current task queue. Cannot add it again.");
                return;
            }
            if(t.State != TaskState.Idle)
            {
                Debug.Error($"Cannot add a task that is not in the idle state. Task: '{t}', state: {t.State}.");
                return;
            }

            TaskQueue.Add(t);
        }

        public void Update(ActiveEntity e)
        {
            if(CurrentTask != null)
            {
                switch (CurrentTask.State)
                {
                    case TaskState.Idle:
                        CurrentTask.Start(e);
                        CurrentTask.InternalUpdate(e);
                        break;
                    case TaskState.Running:
                        CurrentTask.InternalUpdate(e);
                        break;
                    case TaskState.Cancelled:
                        // Set as no longer active, and don't place it back into queue.
                        CurrentTask = null;
                        break;
                    case TaskState.Completed:
                        // Set as no longer active and don't place it back into the queue.
                        CurrentTask = null;
                        break;
                }
            }
            if(CurrentTask == null)
            {
                // Try to take a new one from the queue.
                if(TaskQueueLength != 0)
                {
                    CurrentTask = TaskQueue[0];
                    TaskQueue.RemoveAt(0);
                }
            }
        }
    }
}
