using System;

namespace MICE.Common.Misc
{
    public class Range<T> where T : IComparable
    {
        private readonly T min;
        private readonly T max;

        public Range(T min, T max)
        {
            this.min = min;
            this.max = max;

            if (this.min.CompareTo(this.max) > 0)
            {
                throw new InvalidOperationException($"The upper index (0x{this.max:X4}) must be greater than or equal to the lower index (0x{this.min:X4})");
            }
        }

        public bool IsOverlapped(Range<T> other) => Min.CompareTo(other.Max) < 0 && other.Min.CompareTo(Max) < 0;

        public T Min { get => this.min; }
        public T Max { get => this.max; }
    }
}
