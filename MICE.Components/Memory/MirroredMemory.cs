using MICE.Common.Interfaces;
using MICE.Common.Misc;
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
                if (memorySegment.LowerIndex >= this.mirroredLowerIndex && memorySegment.UpperIndex <= this.mirroredUpperIndex)
                {
                    this.realMemorySegments.Add(memorySegment);
                }
            }
            //this.realMemorySegments = this.memoryMapper.GetSegmentsInRange(this.mirroredLowerIndex, this.mirroredUpperIndex).ToList();
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
            //return this.memoryMapper.ReadByte(newIndex + this.mirroredLowerIndex);
        }

        public override byte[] ReadBytes(ushort startAddress, int size)
        {
            throw new System.NotImplementedException();
        }

        public override ushort ReadShort(int index)
        {
            throw new System.NotImplementedException();
        }

        public override void Write(int index, byte value)
        {
            if (index == 0x3f00|| index == 0x3f20)
            {

            }

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

            var foundMemorySegment = this.realMemorySegments.First(ms => ms.IsIndexInRange(newIndex));
            this.cachedMemorySegments.Add(newIndex, foundMemorySegment);

            foundMemorySegment.Write(newIndex, value);
            //this.memoryMapper.Write(newIndex + this.mirroredLowerIndex, value);
        }

        public override void Write(int index, ushort value)
        {
            throw new System.NotImplementedException();
        }
    }
}
