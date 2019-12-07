using System.Collections;
using System.Collections.Generic;
using Engine.Entities;

namespace Engine.Tasks
{
    /// <summary>
    /// A compound class is a task that contains other tasks: all of these 'sub-tasks' are completed
    /// sequentially in the order that they were added using <see cref="AddTask(Task)"/>.
    /// A compound task exposes the description and
    /// overall progress of it's sub tasks.
    /// </summary>
    public class CompoundTask : Task
    {
        public int TotalTaskCount { get; private set; }
        public int CompletedTaskCount { get; private set; }
        public int QueuedTaskCount { get { return subTasks.Count; } }
        public int RemainingTaskCount
        {
            get
            {
                int current = 0;
                if (CurrentSubTask != null && (CurrentSubTask.State != TaskState.Cancelled && CurrentSubTask.State != TaskState.Completed))
                    current++;

                return subTasks.Count + current;
            }
        }

        public Task CurrentSubTask { get; private set; }
        public override string Description { get { return CurrentSubTask != null ? CurrentSubTask.Description : "No task running."; } }

        private List<Task> subTasks = new List<Task>();

        public CompoundTask(string name, params Task[] tasks) : base(string.IsNullOrWhiteSpace(name) ? "Compound Task" : name)
        {
            if(tasks != null)
            {
                foreach (var t in tasks)
                {
                    AddTask(t);
                }
            }
        }

        public CompoundTask(params Task[] tasks) : this("Compound Task", tasks)
        {

        }

        /// <summary>
        /// Adds a new sub-task to be run.
        /// </summary>
        /// <param name="t">The task to add. May not be null and must be in the Idle state.</param>
        public CompoundTask AddTask(Task t)
        {
            if(t == null)
            {
                Debug.Error("Cannot add null subtask.");
                return this;
            }

            if(t.State != TaskState.Idle)
            {
                Debug.Error($"Cannot add subtask {t} because it is not in the idle state.");
                return this;
            }

            subTasks.Add(t);
            TotalTaskCount++;

            return this;
        }

        /// <summary>
        /// Important to call!
        /// </summary>
        protected override void OnCancel(ActiveEntity e)
        {
            // Cancel currently active task.
            if(CurrentSubTask != null)
            {
                CurrentSubTask.Cancel(e);
            }
        }

        /// <summary>
        /// Important to call!
        /// </summary>
        /// <param name="e"></param>
        protected override void Update(ActiveEntity e)
        {
            if (CurrentSubTask != null)
            {
                switch (CurrentSubTask.State)
                {
                    case TaskState.Idle:
                        CurrentSubTask.Start(e);
                        CurrentSubTask.InternalUpdate(e);
                        break;
                    case TaskState.Running:
                        CurrentSubTask.InternalUpdate(e);
                        break;
                    case TaskState.Cancelled:
                        // Since all tasks need to be completed in order, this global task must also cancel.
                        this.Cancel(e);
                        break;
                    case TaskState.Completed:
                        // Set as no longer active and don't place it back into the queue.
                        CurrentSubTask = null;
                        CompletedTaskCount++;
                        break;
                }
            }
            if (CurrentSubTask == null)
            {
                // Try to take a new one from the queue.
                if (QueuedTaskCount != 0)
                {
                    CurrentSubTask = subTasks[0];
                    subTasks.RemoveAt(0);
                }
                else
                {
                    Complete();
                }
            }

            // Update progress.
            float perTask = 1f / TotalTaskCount;
            float progress = perTask * CompletedTaskCount;
            progress += CurrentSubTask == null ? 0f : CurrentSubTask.Progress * perTask;
            this.Progress = progress;
        }

        public override string ToString()
        {
            return $"{Name}; Current: ({CurrentSubTask}), Completed {CompletedTaskCount} of {TotalTaskCount}, {Progress * 100f:F1}%";
        }
    }
}
