using MICE.Common.Interfaces;
using MICE.Common.Misc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MICE.Components.Memory
{
    public class MirroredMemory : MemorySegment
    {
        private readonly int mirroredLowerIndex;
        private readonly int mirroredUpperIndex;
        private readonly MemoryMapper memoryMapper;
        private readonly int moduloValue;
        private List<IMemorySegment> realMemorySegments = new List<IMemorySegment>();
        private Dictionary<int, IMemorySegment> cachedMemorySegments = new Dictionary<int, IMemorySegment>();

        public MirroredMemory(int lowerIndex, int upperIndex, int mirroredLowerIndex, int mirroredUpperIndex, MemoryMapper memoryMapper, string name)
            : base(lowerIndex, upperIndex, name)
        {
            this.mirroredLowerIndex = mirroredLowerIndex;
            this.mirroredUpperIndex = mirroredUpperIndex;

            this.moduloValue = this.mirroredUpperIndex - this.mirroredLowerIndex;

            this.memoryMapper = memoryMapper;

            foreach (var memorySegment in this.memoryMapper)
            {
                if (this.mirroredLowerIndex >= memorySegment.LowerIndex && memorySegment.UpperIndex <= this.mirroredUpperIndex)
                {
                    this.realMemorySegments.Add(memorySegment);
                }
            }

            if (!this.realMemorySegments.Any())
            {
                throw new InvalidOperationException($"Mirrored memory ({this.Name}) setup, but it can't find another memory segment to link to.");
            }
        }

        public override byte ReadByte(int index)
        {
            var newIndex = (index - this.LowerIndex) + this.mirroredLowerIndex;

            // Still in our own mirrored memory (can repeatedly loop)...
            if (this.IsIndexInRange(newIndex))
            {
                return this.ReadByte(newIndex);
            }

            if (this.cachedMemorySegments.TryGetValue(newIndex, out IMemorySegment memorySegment))
            {
                return memorySegment.ReadByte(newIndex);
            }

            var foundMemorySegment = this.realMemorySegments.First(ms => ms.IsIndexInRange(newIndex));
            this.cachedMemorySegments.Add(newIndex, foundMemorySegment);

            return foundMemorySegment.ReadByte(newIndex);
        }

        public override void Write(int index, byte value)
        {
            var newIndex = (index - this.LowerIndex) + this.mirroredLowerIndex;

            // Still in our own mirrored memory (can repeatedly loop)...
            if (this.IsIndexInRange(newIndex))
            {
                this.Write(newIndex, value);
                return;
            }

            if (this.cachedMemorySegments.TryGetValue(newIndex, out IMemorySegment memorySegment))
            {
                memorySegment.Write(newIndex, value);
                return;
            }

            try
            {
                var foundMemorySegment = this.realMemorySegments.First(ms => ms.IsIndexInRange(newIndex));
                this.cachedMemorySegments.Add(newIndex, foundMemorySegment);
                foundMemorySegment.Write(newIndex, value);
            }
            catch (Exception e)
            {

            }
        }

        public override void CopyBytes(ushort startAddress, Array destination, int destinationIndex, int length)
        {
            var newIndex = (startAddress - this.LowerIndex) + this.mirroredLowerIndex;

            // Still in our own mirrored memory (can repeatedly loop)...
            if (this.IsIndexInRange(newIndex))
            {
                this.CopyBytes(startAddress, destination, destinationIndex, length);
            }

            if (this.cachedMemorySegments.TryGetValue(newIndex, out IMemorySegment memorySegment))
            {
                memorySegment.CopyBytes(startAddress, destination, destinationIndex, length);
            }

            var foundMemorySegment = this.realMemorySegments.First(ms => ms.IsIndexInRange(newIndex));
            this.cachedMemorySegments.Add(newIndex, foundMemorySegment);

            foundMemorySegment.CopyBytes(startAddress, destination, destinationIndex, length);
        }

        public override ushort ReadShort(int index) => throw new NotImplementedException();
        public override void Write(int index, ushort value) => throw new NotImplementedException();
    }
}
