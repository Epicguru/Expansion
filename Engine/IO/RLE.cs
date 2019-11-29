using System.Collections.Generic;

namespace Engine.IO
{
    public static class RLE
    {
        public delegate bool IsSameAs<T>(T current, T next);

        public static List<(int count, T data)> Compress<T>(IEnumerable<T> all, IsSameAs<T> comparer, out int squashed)
        {
            List<(int, T)> output = new List<(int, T)>();
            T current = default;
            int streak = 0;
            bool first = true;
            squashed = 0;
            foreach (var item in all)
            {
                if (first)
                {
                    first = false;
                    current = item;
                    streak = 1;
                    continue;
                }

                bool sameAsLast = comparer.Invoke(current, item);
                if (sameAsLast)
                {
                    streak++;
                    squashed++;
                }
                else
                {
                    int finalStreak = streak;
                    T type = current;

                    output.Add((finalStreak, type));

                    current = item;
                    streak = 1;
                }
            }
            if(streak != 0)
            {
                output.Add((streak, current));
            }

            return output;
        }

        public static List<T> Decompress<T>(IEnumerable<(int count, T data)> compressed)
        {
            List<T> output = new List<T>();
            foreach (var pair in compressed)
            {
                int count = pair.count;
                T item = pair.data;

                for (int i = 0; i < count; i++)
                {
                    output.Add(item);
                }
            }

            return output;
        }
    }
}
