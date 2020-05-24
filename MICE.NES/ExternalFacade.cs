using MICE.Common.Interfaces;
using System;
using Range = MICE.Common.Misc.Range;

namespace MICE.Nintendo
{
    public class ExternalFacade : IExternal
    {
        private readonly IMemoryMap memoryMap;
        private readonly Range range;
        private IExternalHandler mapper;

        public ExternalFacade(IMemoryMap memoryMap, Range range, string name)
        {
            this.memoryMap = memoryMap;
            this.range = range;
            this.Name = name;
        }

        public Range Range => this.range;

        public string Name { get; private set; }

        public Action<int, byte> AfterReadAction => throw new NotImplementedException();

        public Action<int, byte> AfterWriteAction => throw new NotImplementedException();

        public void AttachHandler(IExternalHandler handler)
        {
            this.mapper = handler;
            this.mapper.AddMemorySegment(this);
        }

        public byte ReadByte(int index) => this.mapper.ReadByte(index);
        public ushort ReadShort(int index) => this.mapper.ReadShort(index);
        public void Write(int index, byte value) => this.mapper.Write(index, value);
        public void Write(int index, ushort value) => this.mapper.Write(index, value);

        public int GetOffsetInSegment(int index) => index - this.range.Min;
        public bool IsIndexInRange(int index) => this.range.IsInRange(index);

        public Range GetRange() => throw new NotImplementedException();
        public byte[] GetBytes() => throw new NotImplementedException();
        public bool ContainsIndex(int index) => throw new NotImplementedException();
        public void CopyBytes(ushort startAddress, Span<byte> destination, int destinationIndex, int length) => throw new NotImplementedException();
    }
}
