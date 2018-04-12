using MICE.Common.Misc;
using MICE.Components.Memory;
using MICE.Nintendo.Interfaces;
using System;

namespace MICE.Nintendo.Components
{
    public class KeyboardController2 : MemorySegment, INESInput
    {
        private int readCount = 0;

        public KeyboardController2(Range memoryRange, string name)
            : base(memoryRange, null, name)
        {
        }

        public NESInputs Inputs { get; private set; }
        public bool IsReadingInputs { get; private set; }
        public bool IsStrobing { get; private set; }

        public override byte ReadByte(int index)
        {
            if (this.IsStrobing)
            {
                return this.BuildReturnValue(this.GetInput(NESInputs.A));
            }

            switch (this.readCount++)
            {
                case 0:
                    return this.BuildReturnValue(this.GetInput(NESInputs.A));
                case 1:
                    return this.BuildReturnValue(this.GetInput(NESInputs.B));
                case 2:
                    return this.BuildReturnValue(this.GetInput(NESInputs.Select));
                case 3:
                    return this.BuildReturnValue(this.GetInput(NESInputs.Start));
                case 4:
                    return this.BuildReturnValue(this.GetInput(NESInputs.Up));
                case 5:
                    return this.BuildReturnValue(this.GetInput(NESInputs.Down));
                case 6:
                    return this.BuildReturnValue(this.GetInput(NESInputs.Left));
                case 7:
                    return this.BuildReturnValue(this.GetInput(NESInputs.Right));
                default:
                    return this.BuildReturnValue(isPressed: true);
            }
        }

        private byte BuildReturnValue(bool isPressed) => (byte)(0b01000000 | (isPressed ? 1 : 0));
        private bool GetInput(NESInputs input) => this.Inputs.HasFlag(input);
        public void InputsChanged(NESInputs inputs) => this.Inputs = inputs;

        public override void Write(int index, byte value)
        {
            this.IsStrobing = (value & 1) == 1;
            this.IsReadingInputs = !this.IsStrobing;
            this.readCount = 0;
        }

        public override byte[] GetBytes() => throw new NotImplementedException();
        public override ushort ReadShort(int index) => throw new NotImplementedException();
        public override void Write(int index, ushort value) => throw new NotImplementedException();
        public override void CopyBytes(ushort startAddress, Span<byte> destination, int destinationIndex, int length) => throw new NotImplementedException();
    }
}
