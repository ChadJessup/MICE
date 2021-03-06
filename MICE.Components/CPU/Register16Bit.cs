﻿namespace MICE.Components.CPU
{
    public class Register16Bit : Register<ushort>
    {
        public Register16Bit(string name)
            : base(name)
        {
        }

        public override void Write(ushort value) => this.Value = value;
        public override ushort Read() => this.ReadShortInsteadAction?.Invoke(null, this.Value) ?? this.Value;

        public override string ToString() => $"0x{this.Value:X} - {this.Name}";

        public ushort ReadInternal() => this.Value;
        public void WriteInternal(byte value) => this.Value = value;
    }
}