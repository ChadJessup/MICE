using MICE.Common.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MICE.Common.Misc
{
    public class MemoryMapper : ICollection<IMemorySegment>, IMemoryMap
    {
        private List<IMemorySegment> memorySegments = new List<IMemorySegment>();
        private int minimumRange = 0;
        private int maximumRange = 0;

        public bool ContainsRange(int index) => index >= this.minimumRange && index <= this.maximumRange;

        public (int min, int max) GetMaxRange() => (min: this.minimumRange, max: this.maximumRange);
        public IEnumerable<IMemorySegment> GetSegmentsInRange(int min, int max) => this.memorySegments.Where(seg => seg.IsIndexInRange(min) || seg.IsIndexInRange(max));

        public void Add(IMemorySegment item)
        {
            var existingSegment = this.memorySegments.FirstOrDefault(seg => seg.IsIndexInRange(item.LowerIndex))
                ?? this.memorySegments.FirstOrDefault(seg => seg.IsIndexInRange(item.UpperIndex));

            if (existingSegment != null)
            {
                throw new InvalidOperationException($"Cannot add memory segment {item.Name}, there would be overlap with {existingSegment.Name}");
            }

            existingSegment = this.memorySegments.FirstOrDefault(seg => seg.Name == item.Name);

            if (existingSegment != null)
            {
                throw new InvalidOperationException($"Cannot add memory segment {item.Name}, there is already a segment with that name.");
            }

            if (item.LowerIndex < this.minimumRange)
            {
                this.minimumRange = item.LowerIndex;
            }

            if (item.UpperIndex > this.maximumRange)
            {
                this.maximumRange = item.UpperIndex;
            }

            this.memorySegments.Add(item);
        }

        public T GetMemorySegment<T>(string segmentName) where T : IMemorySegment => (T)this.memorySegments.Where(ms => ms is T).FirstOrDefault(ms => ms.Name == segmentName);

        // Standard collection methods...
        public bool IsReadOnly => false;
        public int Count => this.memorySegments.Count;
        public void Clear() => this.memorySegments.Clear();
        public bool Remove(IMemorySegment item) => this.memorySegments.Remove(item);
        IEnumerator IEnumerable.GetEnumerator() => this.memorySegments.GetEnumerator();
        public bool Contains(IMemorySegment item) => this.memorySegments.Contains(item);
        public IEnumerator<IMemorySegment> GetEnumerator() => this.memorySegments.GetEnumerator();
        public void CopyTo(IMemorySegment[] array, int arrayIndex) => this.memorySegments.CopyTo(array, arrayIndex);

        public T Read<T>(int index)
        {
            for (int i = 0; i < this.memorySegments.Count; i++)
            {
                if (this.memorySegments[i].IsIndexInRange(index))
                {
                    return this.memorySegments[i].Read<T>(index);
                }
            }

            return default(T);
        }

        public void Write<T>(int index, T value)
        {
        }
    }
}