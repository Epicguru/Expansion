using Engine.Entities;
using Microsoft.Xna.Framework;

namespace Engine.Tasks
{
    /// <summary>
    /// A task is performed by an Active Entity. Tasks can be paused, resumed and cancelled, and will update each frame
    /// when active. Tasks also have a progress property that subclasses can modify to indicate progress, although it is optional.
    /// Subclasses of Task should following the naming convention: Task_{Name} such as Task_Wait or Task_MoveTo.
    /// </summary>
    public abstract class Task
    {
        // URGTODO save task state using the usual serialize/deserialize.
        /*
         * Task is being performed by an entity:
         * Task is STARTED.
         * Task is running (active).
         * For some reason, the task is PAUSED (interrupted).
         * After some time, the task is UNPAUSED (resumed).
         * The task is now running again...
         * The task will now complete.
         * Alternatively, the task may be CANCELLED.
         */
         /// <summary>
         /// The internal debugging name of the task. Could just be the name of the class, such as "Task_Wait"
         /// or anything else that gives an idea of what the task is.
         /// </summary>
        public string Name { get; }
        /// <summary>
        /// Gets or sets a description of what the task is currently or generally doing. Should be written with the
        /// entity as the target. For example, Task_MoveTo could have description "Moving to (10, 20)" and Task_Wait could
        /// have description "Waiting for 3 seconds...". Default value is "Working...".
        /// </summary>
        public virtual string Description { get; protected set; } = "Working...";
        /// <summary>
        /// The current state of the task. See <see cref="TaskState"/> for info on each state.
        /// This value cannot be set directly; see <see cref="Start"/>, <see cref="Pause"/>, <see cref="Cancel"/> and others for more info.
        /// </summary>
        public TaskState State { get; private set; } = TaskState.Idle;
        /// <summary>
        /// The current completion progress of this task, in the range 0 to 1. May not be changed depending on the task.
        /// </summary>
        public float Progress
        {
            get
            {
                return _progress;
            }
            set
            {
                _progress = MathHelper.Clamp(value, 0f, 1f);
            }
        }
        private float _progress = 0f;

        public Task() : this(null)
        {

        }

        public Task(string name)
        {
            this.Name = string.IsNullOrWhiteSpace(name) ? "No-name" : name.Trim();
        }

        public void Start(ActiveEntity e)
        {
            if(State != TaskState.Idle)
            {
                Debug.Error($"Cannot start a task when it is in the {State} state. It must be in the Idle state.");
                return;
            }

            State = TaskState.Running;
            OnStart(e);
        }

        public void Cancel(ActiveEntity e)
        {
            if(State != TaskState.Running)
            {
                Debug.Error($"Cannot cancel task when it is in the {State} state. It must be in the Running state.");
                return;
            }

            State = TaskState.Cancelled;
            OnCancel(e);
        }

        protected void Complete()
        {
            if(State == TaskState.Cancelled || State == TaskState.Completed)
            {
                Debug.Error($"Current task state is {State} which means calling Finish() is invalid.");
                return;
            }

            State = TaskState.Completed;
        }

        internal void InternalUpdate(ActiveEntity e)
        {
            this.Update(e);
        }

        protected virtual void Update(ActiveEntity e)
        {

        }

        protected virtual void OnStart(ActiveEntity e)
        {

        }

        protected virtual void OnCancel(ActiveEntity e)
        {

        }

        public override string ToString()
        {
            return $"{Name}{(Progress == 0f ? "" : $": {Progress * 100f:F0}%")}";
        }
    }
}
