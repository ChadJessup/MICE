﻿using MICE.Common.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MICE.Common.Misc
{
    public class MemoryMapper : IMemoryMap
    {
        private Dictionary<int, IMemorySegment> memorySegmentCache = new Dictionary<int, IMemorySegment>();

        private List<IMemorySegment> memorySegments = new List<IMemorySegment>();
        private int minimumRange = 0;
        private int maximumRange = 0;

        public bool ContainsRange(int index) => index >= this.minimumRange && index <= this.maximumRange;

        public (int min, int max) GetMaxRange() => (min: this.minimumRange, max: this.maximumRange);
        public IEnumerable<IMemorySegment> GetSegmentsInRange(int min, int max)
        {
            return this.memorySegments.Where(seg => seg.Range.IsOverlapped(new Range(min, max)));
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

        public T GetMemorySegment<T>(string segmentName) where T : IMemorySegment
        {
            return (T)this.memorySegments.First(ms => ms is T && ms.Name == segmentName);
        }

        // Standard collection methods...
        public bool IsReadOnly => false;
        public int Count => this.memorySegments.Count;

        public byte[] Data => throw new NotImplementedException();

        public void Clear() => this.memorySegments.Clear();
        public bool Remove(IMemorySegment item) => this.memorySegments.Remove(item);
        IEnumerator IEnumerable.GetEnumerator() => this.memorySegments.GetEnumerator();
        public bool Contains(IMemorySegment item) => this.memorySegments.Contains(item);
        public IEnumerator<IMemorySegment> GetEnumerator() => this.memorySegments.GetEnumerator();
        public void CopyTo(IMemorySegment[] array, int arrayIndex) => this.memorySegments.CopyTo(array, arrayIndex);

        public virtual ushort ReadShort(int index) => (ushort)(this.ReadByte(index + 1) << 8 | this.ReadByte(index));

        public virtual byte ReadByte(int index)
        {
            if (this.memorySegmentCache.TryGetValue(index, out IMemorySegment cachedSegment))
            {
                return cachedSegment.ReadByte(index);
            }
            else
            {
                IMemorySegment segment = null;
                for (int i = 0; i < this.memorySegments.Count; i++)
                {
                    if (this.memorySegments[i].IsIndexInRange(index))
                    {
                        segment = this.memorySegments[i];
                        break;
                    }
                }

                this.memorySegmentCache.Add(index, segment);
                return segment.ReadByte(index);
            }
        }

        public virtual void Write(int index, byte value)
        {
            if (this.memorySegmentCache.TryGetValue(index, out IMemorySegment cachedSegment))
            {
                cachedSegment.Write(index, value);
                return;
            }

            for (int i = 0; i < this.memorySegments.Count; i++)
            {
                if (this.memorySegments[i].IsIndexInRange(index))
                {
                    this.memorySegmentCache.Add(index, this.memorySegments[i]);
                    this.memorySegments[i].Write(index, value);
                    return;
                }
            }
        }

        public virtual void BulkTransfer(ushort startAddress, Span<byte> destinationArray, int destinationIndex, int size)
        {
            var segments = this.GetSegmentsInRange(startAddress, startAddress + size);

            if (segments.Count() > 1)
            {
                throw new NotImplementedException("Sorry, don't handle bulk transfer across multiple segments...hope it's not actually needed...");
            }

            segments.First().CopyBytes(startAddress, destinationArray, destinationIndex, size);
        }

        public IEnumerable<IMemorySegment> GetMemorySegments() => this.memorySegments;

        public IEnumerable<T> GetMemorySegments<T>() where T : IMemorySegment => this.memorySegments.OfType<T>();
    }
}