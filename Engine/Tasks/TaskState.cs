using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Tasks
{
    public enum TaskState
    {
        /// <summary>
        /// The task has not been started yet.
        /// </summary>
        Idle,
        /// <summary>
        /// The task is currently active.
        /// </summary>
        Running,
        /// <summary>
        /// The task was running, but has been cancelled. It will never be resumed.
        /// </summary>
        Cancelled,
        /// <summary>
        /// The task was running and has now finished it's job.
        /// </summary>
        Completed
    }
}
