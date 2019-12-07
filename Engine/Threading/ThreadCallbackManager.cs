using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace Engine.Threading
{
    public abstract class ThreadCallbackManager<In, Out> : IDisposable where Out : struct
    {
        public int ThreadCount { get; }
        public bool IsRunning { get; private set; }
        public readonly ThreadStats[] Statistics;
        public int PendingCount { get { return pending.Count; } }
        public int ProcessedLastSecond { get; private set; }

        public struct ThreadStats
        {
            public readonly int HISTORY_LENGTH;

            /// <summary>
            /// The average usage percentage over the last second.
            /// A value of zero means the thread did (very close to) nothing, and a value of 1
            /// means that the thread was constantly processing one or more requests.
            /// </summary>
            public float AverageUsage;

            internal float CumulativeUsage;
            internal int CumulativeProcessed;

            /// <summary>
            /// The number of requests that this thread started to process over the last second.
            /// </summary>
            public int ProcessedLastSecond;

            /// <summary>
            /// A list of the times that it took to process the last (<see cref="HISTORY_LENGTH"/>) number of requests.
            /// The first item in the array is the most recent request, and the last item in the array was the oldest request to be
            /// processed.
            /// </summary>
            public readonly float[] ProcessTimes;

            /// <summary>
            /// The minimum (shortest) time, in seconds, that it took to process any of the last (<see cref="HISTORY_LENGTH"/>) requests.
            /// </summary>
            public float MinProcessTime
            {
                get
                {
                    float min = ProcessTimes[0];
                    for (int i = 0; i < ProcessTimes.Length; i++)
                    {
                        float value = ProcessTimes[i];
                        if (value < min)
                            min = value;
                    }

                    return min;
                }
            }

            /// <summary>
            /// The maximum (longest) time, in seconds, that it took to process any of the last (<see cref="HISTORY_LENGTH"/>) requests.
            /// </summary>
            public float MaxProcessTime
            {
                get
                {
                    float max = ProcessTimes[0];
                    for (int i = 0; i < ProcessTimes.Length; i++)
                    {
                        float value = ProcessTimes[i];
                        if (value > max)
                            max = value;
                    }

                    return max;
                }
            }

            /// <summary>
            /// The mean (average) time, in seconds, that it took to process each of the last (<see cref="HISTORY_LENGTH"/>) requests.
            /// </summary>
            public float MeanProcessTime
            {
                get
                {
                    float av = 0f;
                    for (int i = 0; i < ProcessTimes.Length; i++)
                    {
                        av += ProcessTimes[i];
                    }

                    return av / ProcessTimes.Length;
                }
            }

            public ThreadStats(int historyLength)
            {
                this.HISTORY_LENGTH = historyLength;
                this.ProcessTimes = new float[historyLength];
                this.AverageUsage = 0f;
                this.ProcessedLastSecond = 0;
                this.CumulativeProcessed = 0;
                this.CumulativeUsage = 0;
            }

            internal void AddProcessTime(float time)
            {
                for (int i = 1; i < ProcessTimes.Length; i++)
                {
                    ProcessTimes[i] = ProcessTimes[i - 1];
                }
                ProcessTimes[0] = time;
            }
        }

        private int _processedIncrement;
        private Queue<ThreadedRequest<In, Out>> pending = new Queue<ThreadedRequest<In, Out>>();
        protected readonly object KEY = new object();
        protected readonly object KEY2 = new object();
        private Thread[] threads;
        private IThreadProcessor<In, Out>[] processors;
        private Queue<Action> pendingMainThread = new Queue<Action>();
        private float timer;

        public ThreadCallbackManager(int threadCount)
        {
            if (threadCount <= 0)
                throw new System.ArgumentOutOfRangeException(nameof(threadCount), $"Invalid thread count '{threadCount}'. Must be at least 1.");

            ThreadCount = threadCount;
            threads = new Thread[threadCount];
            processors = new IThreadProcessor<In, Out>[threadCount];
            Statistics = new ThreadStats[threadCount];

            const int HISTORY_COUNT = 20;
            for (int i = 0; i < threadCount; i++)
            {
                Thread t = new Thread(RunThread);
                t.Name = $"Worker Thread {i}";
                threads[i] = t;
                Statistics[i] = new ThreadStats(HISTORY_COUNT);

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

        /// <summary>
        /// To be called from the main thread (loop thread).
        /// </summary>
        public void Update()
        {
            lock (KEY2)
            {
                while(pendingMainThread.Count > 0)
                {
                    Action a = pendingMainThread.Dequeue();

                    a.Invoke();
                }
            }

            timer += Time.unscaledDeltaTime;
            if(timer >= 1f)
            {
                timer = 0f;

                ProcessedLastSecond = _processedIncrement;
                _processedIncrement = 0;

                for (int i = 0; i < Statistics.Length; i++)
                {
                    var s = Statistics[i];
                    s.AverageUsage = s.CumulativeUsage;
                    s.CumulativeUsage = 0f;
                    s.ProcessedLastSecond = s.CumulativeProcessed;
                    s.CumulativeProcessed = 0;
                    Statistics[i] = s;
                }
            }
        }

        protected virtual void RunThread(object arg)
        {
            int threadIndex = (int)arg;
            IThreadProcessor<In, Out> processor = processors[threadIndex];
            Stopwatch watch = new Stopwatch();

            const int IDLE_TIME = 5;
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
                        Statistics[threadIndex].CumulativeProcessed++;
                        _processedIncrement++;
                        watch.Start();
                        Out output = default(Out);
                        try
                        {
                            output = processor.Process(todo.InputArgs);
                        }
                        catch (Exception)
                        {
                            PostResult(() =>
                            {
                                todo.UponProcessed?.Invoke(ThreadedRequestResult.Error, output);
                                todo.ReturnToPool();
                            });
                            continue;
                        }
                        PostResult(() =>
                        {
                            todo.UponProcessed?.Invoke(ThreadedRequestResult.Run, output);
                            todo.ReturnToPool();
                        });
                        watch.Stop();
                        Statistics[threadIndex].CumulativeUsage += (float)watch.Elapsed.TotalSeconds;
                        if (Statistics[threadIndex].CumulativeUsage > 1f)
                            Statistics[threadIndex].CumulativeUsage = 1f;
                        Statistics[threadIndex].AddProcessTime((float)watch.Elapsed.TotalSeconds);
                        watch.Reset();
                    }
                    else if (todo != null && todo.IsCancelled)
                    {
                        PostResult(() =>
                        {
                            todo.UponProcessed?.Invoke(ThreadedRequestResult.Cancelled, default(Out));
                            todo.ReturnToPool();
                        });                        
                    }
                }
                else
                {
                    Thread.Sleep(IDLE_TIME);
                }
            }
        }

        private void PostResult(Action a)
        {
            lock (KEY2)
            {
                pendingMainThread.Enqueue(a);
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
