using System;
using System.Collections.Generic;

namespace Engine.Threading
{
    public class ThreadedRequest<In, Out>
    {
        public static int PooledCount { get { return pool.Count; } }

        public static ThreadedRequest<In, Out> Create(Action<ThreadedRequestResult, Out> uponProcessed, In input)
        {
            var created = GetNew(uponProcessed);
            created.InputArgs = input;

            return created;
        }

        private static Queue<ThreadedRequest<In, Out>> pool = new Queue<ThreadedRequest<In, Out>>();

        private static ThreadedRequest<In, Out> GetNew(Action<ThreadedRequestResult, Out> onProcessed)
        {
            if(pool.Count == 0)
            {
                return new ThreadedRequest<In, Out>(onProcessed);
            }
            else
            {
                var fromPool = pool.Dequeue();
                fromPool.UponProcessed = onProcessed;
                fromPool.IsCancelled = false;
                fromPool.IsInPool = false;
                return fromPool;
            }
        }

        public In InputArgs;
        public Action<ThreadedRequestResult, Out> UponProcessed; 
        public bool IsCancelled { get; private set; }
        public bool IsInPool { get; private set; }

        private ThreadedRequest(Action<ThreadedRequestResult, Out> onProcessed)
        {
            this.UponProcessed = onProcessed;
        }

        /// <summary>
        /// Flags this request as being cancelled. If the <see cref="UponProcessed"/> action is not null,
        /// it will recieve a call with the <see cref="ThreadedRequestResult.Cancelled"/> argument at some point in the future, after which the
        /// request should be immediately discarded as it will be recycled later.
        /// </summary>
        public void Cancel()
        {
            IsCancelled = true;
        }

        /// <summary>
        /// This should only be called on a safe thread.
        /// </summary>
        internal void ReturnToPool()
        {
            IsInPool = true;
            IsCancelled = false;
            UponProcessed = null;
            pool.Enqueue(this);
        }
    }

    public enum ThreadedRequestResult
    {
        Run,
        Cancelled,
        Error
    }
}
