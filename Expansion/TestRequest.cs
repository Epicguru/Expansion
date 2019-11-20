using Engine.Threading;
using System;

namespace Expansion
{
    public static class TestRequest
    {
        public static ThreadedRequest<int[], double> Create(Action<ThreadedRequestResult, double> uponComplete, params int[] inputs)
        {
            return ThreadedRequest<int[], double>.Create(uponComplete, inputs);
        }
    }
}
