using MICE.Common.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MICE.Common.Misc
{
    public class MemoryMapper : ICollection<IMemorySegment>, IMemoryMap
    {
        private StreamWriter sw;
        public MemoryMapper(StreamWriter sw)
        {
            this.sw = sw;
        }

        private Dictionary<int, IMemorySegment> memorySegmentCache = new Dictionary<int, IMemorySegment>();

        private List<IMemorySegment> memorySegments = new List<IMemorySegment>();
        private int minimumRange = 0;
        private int maximumRange = 0;

        public bool ContainsRange(int index) => index >= this.minimumRange && index <= this.maximumRange;

        public (int min, int max) GetMaxRange() => (min: this.minimumRange, max: this.maximumRange);
        public IEnumerable<IMemorySegment> GetSegmentsInRange(int min, int max)
        {
            var range = new Range<int>(min, max);
            return this.memorySegments.Where(seg => seg.Range.IsOverlapped(range));
        }

        public void Add(IMemorySegment item)
        {
            var existingSegment = this.memorySegments.FirstOrDefault(seg => seg.IsIndexInRange(item.Range.Min))
                ?? this.memorySegments.FirstOrDefault(seg => seg.IsIndexInRange(item.Range.Max));

            if (existingSegment != null)
            {
                throw new InvalidOperationException($"Cannot add memory segment {item.Name}, there would be overlap with {existingSegment.Name}");
            }

            existingSegment = this.memorySegments.FirstOrDefault(seg => seg.Name == item.Name);

            if (existingSegment != null)
            {
                throw new InvalidOperationException($"Cannot add memory segment {item.Name}, there is already a segment with that name.");
            }

            if (item.Range.Min < this.minimumRange)
            {
                this.minimumRange = item.Range.Min;
            }

            if (item.Range.Max > this.maximumRange)
            {
                this.maximumRange = item.Range.Max;
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

        public virtual ushort ReadShort(int index) => (ushort)(this.ReadByte(index + 1) << 8 | this.ReadByte(index));

        public virtual byte ReadByte(int index)
        {
            byte value = 0x00;

            if (this.memorySegmentCache.TryGetValue(index, out IMemorySegment cachedSegment))
            {
                value = cachedSegment.ReadByte(index);
            }
            else
            {
                foreach (var segment in this.memorySegments.Where(seg => seg.IsIndexInRange(index)))
                {
                    this.memorySegmentCache.Add(index, segment);
                    value = segment.ReadByte(index);
                    break;
                }
            }

            return value;

            throw new InvalidOperationException($"Address was requested that hasn't been mapped (0x{index:X})");
        }

        public virtual void Write(int index, byte value)
        {
            if (index == 0x2340)
            {

            }
            if (this.memorySegmentCache.TryGetValue(index, out IMemorySegment cachedSegment))
            {
                cachedSegment.Write(index, value);
                return;
            }

            foreach (var segment in this.memorySegments.Where(seg => seg.IsIndexInRange(index)))
            {
                this.memorySegmentCache.Add(index, segment);
                segment.Write(index, value);
                return;
            }

            throw new InvalidOperationException($"Address was requested that hasn't been mapped (0x{index:X})");
        }

        public virtual void BulkTransfer(ushort startAddress, Array destinationArray, int destinationIndex, int size)
        {
            var segments = this.GetSegmentsInRange(startAddress, startAddress + size);

            if (segments.Count() > 1)
            {
                throw new NotImplementedException("Sorry, don't handle bulk transfer across multiple segments...hope it's not actually needed...");
            }

            segments.First().CopyBytes(startAddress, destinationArray, destinationIndex, size);
        }

        public IEnumerable<IMemorySegment> GetMemorySegments() => this.memorySegments;
    }
}