using System;
using MICE.Common.Interfaces;

namespace MICE.Components.Memory
{
    /// <summary>
    /// Memory segment that is actually mapped to a Register of a component.
    /// Reading/writing to this MemoryMappedSegment will directly reference the Register that is being mapped.
    /// </summary>
    public class MemoryMappedRegister<T> : MemoryMappedSegment, IRegister<T> where T : struct
    {
        private IRegister<T> mappedRegister;

        public MemoryMappedRegister(int lowerIndex, int upperIndex, IRegister<T> mappedRegister, string name)
            : base(lowerIndex, upperIndex, name)
        {
            this.mappedRegister = mappedRegister;
        }

        public new Action<int?, T> AfterReadAction => this.mappedRegister.AfterReadAction;
        public new Action<int?, T> AfterWriteAction => this.mappedRegister.AfterWriteAction;

        public Func<int?, T, byte> ReadByteInsteadAction { get; set; }
        public Func<int?, T, ushort> ReadShortInsteadAction { get; set; }

        public override void CopyBytes(ushort startAddress, Array destination, int destinationIndex, int length) => throw new NotImplementedException();

        public override int GetOffsetInSegment(int index) => throw new System.NotImplementedException();

        public T Read() => this.mappedRegister.Read();
        public override byte ReadByte(int index) => (byte)(object)this.mappedRegister.Read();
        public override ushort ReadShort(int index) => (ushort)(object)this.mappedRegister.Read();

        public override void Write(int index, byte value) => this.InternalWrite(value);
        public override void Write(int index, ushort value) => this.InternalWrite(value);

        public void Write(byte value) => this.InternalWrite(value);
        public void Write(ushort value) => this.InternalWrite(value);

        public void Write(T value) => this.InternalWrite(value);

        private void InternalWrite(object value)
        {
            // TODO: Not the fastest, could do something better here.  Need to rethink the entire Generics setup.
            var convertedType = (T)Convert.ChangeType(value, typeof(T));
            this.mappedRegister.Write(convertedType);
            /*
            switch (value)
            {
                case ushort newValue:
                    this.Write(newValue);
                    break;
                case byte newValue:
                    this.Write(newValue);
                    break;
                default:
                    throw new NotImplementedException($"Trying to write as unknown value type for MemoryMappedRegister.{Environment.NewLine}Type: {typeof(T)}{Environment.NewLine}UnexpectedType: {value.GetType()}");
            }
            */
        }

    }
}
