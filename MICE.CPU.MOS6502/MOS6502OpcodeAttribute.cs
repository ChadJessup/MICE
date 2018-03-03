﻿using MICE.Common.Attributes;
using System;

namespace MICE.CPU.MOS6502
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Delegate | AttributeTargets.Method | AttributeTargets.Struct, AllowMultiple = true, Inherited = true)]
    public class MOS6502OpcodeAttribute : OpcodeAttribute
    {
        /// <summary>
        /// The Addressing mode of this instruction.
        /// </summary>
        public AddressingMode AddressingMode { get; private set; } = AddressingMode.None;

        /// <summary>
        /// The amount of cycles this instruction takes.
        /// </summary>
        public int Cycles { get; private set; }

        /// <summary>
        /// The change expected in the PC register, usually how many bytes were read while.
        /// </summary>
        public int PCDelta { get; private set; }

        public bool ShouldVerify { get; private set; }

        public MOS6502OpcodeAttribute(int code, string name, AddressingMode addressingMode, int cycles, int pcDelta, string description = "", bool verify = true)
            : base(code, name, description)
        {
            this.AddressingMode = addressingMode;
            this.Cycles = cycles;
            this.PCDelta = pcDelta;
            this.ShouldVerify = verify;
        }
    }
}
