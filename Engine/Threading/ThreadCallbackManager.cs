using System;
using System.Collections.Generic;
using System.Threading;

namespace Engine.Threading
{
    public abstract class ThreadCallbackManager<In, Out> : IDisposable where Out : struct
    {
        public int ThreadCount { get; }
        public bool IsRunning { get; private set; }

        private Queue<ThreadedRequest<In, Out>> pending = new Queue<ThreadedRequest<In, Out>>();
        protected readonly object KEY = new object();
        private Thread[] threads;
        private IThreadProcessor<In, Out>[] processors;

        public ThreadCallbackManager(int threadCount)
        {
            if (threadCount <= 0)
                throw new System.ArgumentOutOfRangeException(nameof(threadCount), $"Invalid thread count '{threadCount}'. Must be at least 1.");

            ThreadCount = threadCount;
            threads = new Thread[threadCount];
            processors = new IThreadProcessor<In, Out>[threadCount];

            for (int i = 0; i < threadCount; i++)
            {
                Thread t = new Thread(RunThread);
                t.Name = $"Worker Thread {i}";
                threads[i] = t;

                processors[i] = this.CreateProcessor(i);
            }
        }

        public virtual void Start()
        {
            if (IsRunning)
            {
                Debug.Warn("Already running threads.");
                return;
            }
            IsRunning = true;

            for (int i = 0; i < ThreadCount; i++)
            {
                threads[i].Start(i);
            }
        }

        public virtual void Stop()
        {
            if (!IsRunning)
            {
                Debug.Warn("Not running, cannot stop threads.");
                return;
            }

            IsRunning = false;
        }

        protected virtual void RunThread(object arg)
        {
            int threadIndex = (int)arg;
            IThreadProcessor<In, Out> processor = processors[threadIndex];

            const int IDLE_TIME = 1;
            while (IsRunning)
            {
                if(pending.Count > 0)
                {
                    ThreadedRequest<In, Out> todo = null;
                    lock (KEY)
                    {
                        if (pending.Count > 0)
                            todo = pending.Dequeue();
                    }

                    if(todo != null && !todo.IsCancelled)
                    {
                        Out output = default(Out);
                        try
                        {
                            output = processor.Process(todo.InputArgs);
                        }
                        catch (Exception)
                        {
                            todo.UponProcessed?.Invoke(ThreadedRequestResult.Error, output);
                            todo.ReturnToPool();
                            continue;
                        }
                        todo.UponProcessed?.Invoke(ThreadedRequestResult.Run, output);
                        todo.ReturnToPool();
                    }
                    else if (todo != null && todo.IsCancelled)
                    {
                        todo.UponProcessed?.Invoke(ThreadedRequestResult.Cancelled, default(Out));
                        todo.ReturnToPool();
                    }
                }
                else
                {
                    Thread.Sleep(IDLE_TIME);
                }
            }
        }

        public abstract IThreadProcessor<In, Out> CreateProcessor(int threadIndex); 

        public ThreadedRequest<In, Out> Post(ThreadedRequest<In, Out> request)
        {
            if (request == null)
            {
                Debug.Warn("Null request passed into Post method on threaded.");
                return null;
            }
            if (request.IsInPool)
            {
                Debug.Warn("Attempt to post a request that is currently in the pool: be careful with request lifespans! A request cannot be re-used!");
                return null;
            }
            if (request.IsCancelled)
            {
                request.UponProcessed?.Invoke(ThreadedRequestResult.Cancelled, default(Out));
                request.ReturnToPool();
                return null;
            }

            lock (KEY)
            {
                pending.Enqueue(request);
            }
            return request;
        }

        public void Dispose()
        {
            if (IsRunning)
                Stop();

            if(threads != null)
            {
                threads = null;
                processors = null;
                pending.Clear();
                pending = null;
            }
        }
    }
}
