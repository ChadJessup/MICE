using System;

namespace MICE.Common.Misc
{
    public struct Range
    {
        private readonly int min;
        private readonly int max;

        public Range(int min, int max)
        {
            this.min = min;
            this.max = max;

            if (this.min.CompareTo(this.max) > 0)
            {
                throw new InvalidOperationException($"The upper index (0x{this.max:X4}) must be greater than or equal to the lower index (0x{this.min:X4})");
            }
        }

        public int Min { get => this.min; }
        public int Max { get => this.max; }

        public bool IsOverlapped(Range other) => Min.CompareTo(other.Max) <= 0 && other.Min.CompareTo(Max) <= 0;
        public override int GetHashCode() => this.min.GetHashCode() + this.max.GetHashCode();
        public override bool Equals(object obj)
        {
            if (!(obj is Range))
            {
                return false;
            }

            var other = (Range)obj;

            return this.min.Equals(other.min) && this.max.Equals(other.max);
        }

        public override string ToString() => $"Min: 0x{this.min:X4} Max: 0x{this.max:X4}";

        public bool TryGetOffset(int index, out int offset)
        {
            if (!this.IsInRange(index))
            {
                offset = -1;
                return false;
            }

            offset = this.min == 0
                ? offset = index
                : offset = index - min;

            return true;
        }

        public bool IsInRange(int index) => this.min <= index && this.max >= index;
        public bool CompareTo(Range other) => Min.CompareTo(other.Max) <= 0 && Min.CompareTo(other.Max) <= 0 || Max.CompareTo(other.Min) >= 0 && Max.CompareTo(other.Max) <= 0;
        public bool IsInRange(Range other) => this.IsInRange(other.min) || this.IsInRange(other.max);
    }
}
