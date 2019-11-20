using Engine.Threading;
using System;

namespace Expansion
{
    public class TestWorkerThread : ThreadCallbackManager<int[], double>
    {
        public TestWorkerThread() : base(8)
        {

        }

        public override IThreadProcessor<int[], double> CreateProcessor(int threadIndex)
        {
            return new Processor();
        }

        private class Processor : IThreadProcessor<int[], double>
        {
            public double Process(int[] input)
            {
                // Gives the average of the square root of all input items.
                // Just to waste a bit of time.

                if (input == null || input.Length == 0)
                    return 0;

                if (input[0] == 69)
                    throw new Exception("HAHHAHAHA 69 lol");

                double sum = 0;
                for (int i = 0; i < input.Length; i++)
                {
                    double sqrt = Math.Sqrt(input[i]);
                    sum += sqrt;
                }

                return sum / input.Length;
            }
        }
    }
}
