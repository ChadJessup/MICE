using System;

namespace MICE.Components.Memory
{
    public class Range<T> where T : IComparable
    {
        private readonly T min;
        private readonly T max;

        public Range(T min, T max)
        {
            this.min = min;
            this.max = max;
        }

        public bool IsOverlapped(Range<T> other) => Min.CompareTo(other.Max) < 0 && other.Min.CompareTo(Max) < 0;

        public T Min { get => this.min; }
        public T Max { get => this.max; }
    }
}
