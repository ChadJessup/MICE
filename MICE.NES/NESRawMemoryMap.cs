using MICE.Common.Interfaces;
using MICE.Common.Misc;
using MICE.Components.Memory;
using MICE.Nintendo.Handlers;
using MICE.CPU.MOS6502;
using MICE.PPU.RicohRP2C02;
using System;
using System.Collections;
using System.Collections.Generic;
using MICE.Components.CPU;

namespace MICE.Nintendo
{
    /// <summary>
    /// As basic of a memory map as possible, testing out speed over readability/maintainability.
    /// </summary>
    public class NESRawMemoryMap : IMemoryMap, IMemorySegment
    {
        private static class Constants
        {
            public const string ProgramRomLowerBank = "PRG-ROM Lower Bank";
            public const string ProgramRomUpperBank = "PRG-ROM Upper Bank";
        }

        private static class MemoryRanges
        {
            // http://nesdev.com/NESDoc.pdf - Figure 2-3 CPU memory map
            public static Range<int> ZeroPage = new Range<int>(0x0000, 0x00FF);
            public static Range<int> Stack = new Range<int>(0x0100, 0x01FF);
            public static Range<int> RAM = new Range<int>(0x0200, 0x07FF);

            /// Above mirrored... until 0x1FFF...

            public static Range<int> RegisterRanges = new Range<int>(0x2000, 0x401F);

            // Expansion Memory @ 0x4020 - 0x5FFF

            public static Range<int> SRAM = new Range<int>(0x6000, 0x7FFF);


            public static Range<int> ProgramRomLowerBank = new Range<int>(0x8000, 0xBFFF);
            public static Range<int> ProgramRomUpperBank = new Range<int>(0xC000, 0xFFFF);
        }

        private static class PPURegisterAddresses
        {
            public const int PPUCTRL = 0x2000;
            public const int PPUMASK = 0x2001;
            public const int PPUSTATUS = 0x2002;
            public const int OAMADDR = 0x2003;
            public const int OAMDATA = 0x2004;
            public const int PPUSCROLL = 0x2005;
            public const int PPUADDR = 0x2006;
            public const int PPUDATA = 0x2007;
        }

        private static class APURegisterAddresses
        {
            public const ushort APUChannelStatus = 0x4015;
        }

        private Dictionary<int, Register8Bit> ppuRegisterLookup;

        private byte[] memory = new byte[0x10000];
        private readonly PPURegisters ppuRegisters;
        private readonly InputHandler inputHandler;

        // These memory segments are asked for specifically by the NES system 
        private SRAM sram;
        private ExternalFacade prgROMLowerBank = null;
        private ExternalFacade prgROMUpperBank = null;
        private CPU.MOS6502.Stack stack;

        public NESRawMemoryMap(PPURegisters ppuRegisters, InputHandler inputHandler) : base()
        {
            this.ppuRegisters = ppuRegisters;
            this.inputHandler = inputHandler;

            this.sram = new SRAM(MemoryRanges.SRAM, "SRAM");
            this.stack = new CPU.MOS6502.Stack(MemoryRanges.Stack, "Stack");

            this.prgROMLowerBank = new ExternalFacade(this, MemoryRanges.ProgramRomLowerBank, Constants.ProgramRomLowerBank);
            this.prgROMUpperBank = new ExternalFacade(this, MemoryRanges.ProgramRomUpperBank, Constants.ProgramRomUpperBank);

            this.ppuRegisterLookup = new Dictionary<int, Register8Bit>()
            {
                { PPURegisterAddresses.PPUCTRL, ppuRegisters.PPUCTRL },
                { PPURegisterAddresses.PPUMASK, ppuRegisters.PPUMASK },
                { PPURegisterAddresses.PPUSTATUS, ppuRegisters.PPUSTATUS },
                { PPURegisterAddresses.OAMADDR, ppuRegisters.OAMADDR },
                { PPURegisterAddresses.OAMDATA, ppuRegisters.OAMDATA },
                { PPURegisterAddresses.PPUADDR, ppuRegisters.PPUADDR },
                { PPURegisterAddresses.PPUDATA, ppuRegisters.PPUDATA},
            };

            this.ScopeMemoryRanges();
        }

        public Memory<byte> ZeroPage { get; private set; }
        public Memory<byte> RAM { get; private set; }

        public byte ReadByte(int index)
        {
            if (MemoryRanges.ProgramRomUpperBank.IsInRange(index))
            {
                return this.prgROMUpperBank.ReadByte(index);
            }
            else if (MemoryRanges.ProgramRomLowerBank.IsInRange(index))
            {
                return this.prgROMLowerBank.ReadByte(index);
            }
            else if (this.ppuRegisterLookup.ContainsKey(index))
            {
                return this.ppuRegisterLookup[index].Read();
            }
            else if (MemoryRanges.ZeroPage.IsInRange(index))
            {
                return this.ZeroPage.Span[index];
            }
            else if (MemoryRanges.RAM.IsInRange(index))
            {
                return this.RAM.Span[this.GetOffset(MemoryRanges.RAM, index)];
            }

            switch (index)
            {
                case PPURegisterAddresses.PPUSCROLL:
                    // Shouldn't happen for this particular register.
                    //return this.ppuRegisters.PPUSCROLL.Read();
                    break;
                case APURegisterAddresses.APUChannelStatus:
                    break;
                default:
                    throw new NotImplementedException();
            }

            throw new NotImplementedException();
        }
        
        public ushort ReadShort(int index)
        {
            if (MemoryRanges.ProgramRomUpperBank.IsInRange(index))
            {
                return this.prgROMUpperBank.ReadShort(index);
            }
            else if (MemoryRanges.ProgramRomLowerBank.IsInRange(index))
            {
                return this.prgROMLowerBank.ReadShort(index);
            }
            else if (MemoryRanges.ZeroPage.IsInRange(index))
            {
                return this.ZeroPage.Span.NonPortableCast<byte, ushort>()[this.GetOffset(MemoryRanges.ZeroPage, index)];
            }
            else if (MemoryRanges.RAM.IsInRange(index))
            {
                return this.RAM.Span.NonPortableCast<byte, ushort>()[this.GetOffset(MemoryRanges.RAM, index)];
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public void Write(int index, byte value)
        {
            if (MemoryRanges.RegisterRanges.IsInRange(index))
            {
                this.WriteRegister(index, value);
            }
            else if (MemoryRanges.ZeroPage.IsInRange(index))
            {
                this.ZeroPage.Span[this.GetOffset(MemoryRanges.ZeroPage, index)] = value;
            }
            else if (MemoryRanges.RAM.IsInRange(index))
            {
                this.RAM.Span[this.GetOffset(MemoryRanges.RAM, index)] = value;
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        private void WriteRegister(int index, byte value)
        {
            if (this.ppuRegisterLookup.ContainsKey(index))
            {
                this.ppuRegisterLookup[index].Write(value);
                return;
            }

            switch (index)
            {
                case PPURegisterAddresses.PPUSCROLL:
                    this.ppuRegisters.PPUSCROLL.Write(value);
                    break;
                case APURegisterAddresses.APUChannelStatus:
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        private void ScopeMemoryRanges()
        {
            this.ZeroPage = this.GetMemory(MemoryRanges.ZeroPage);
            this.RAM = this.GetMemory(MemoryRanges.RAM);
        }

        private int GetOffset(Range<int> range, int index) => index - range.Min - 1;

        private Memory<byte> GetMemory(Range<int> range) => new Memory<byte>(this.memory, range.Min, range.Max - range.Min);

        public T GetMemorySegment<T>(string segmentName) where T : IMemorySegment
        {
            if (typeof(T) == typeof(SRAM))
            {
                return (T)(IMemorySegment)this.sram;
            }
            else if (segmentName == Constants.ProgramRomLowerBank)
            {
                return (T)(IMemorySegment)this.prgROMLowerBank;
            }
            else if (segmentName == Constants.ProgramRomUpperBank)
            {
                return (T)(IMemorySegment)this.prgROMUpperBank;
            }
            else if(segmentName == "Stack")
            {
                return (T)(IMemorySegment)this.stack;
            }

            return (T)(IMemorySegment)this;
        }

        public int Count => throw new NotImplementedException();
        public string Name => throw new NotImplementedException();
        public void Clear() => throw new NotImplementedException();
        public bool IsReadOnly => throw new NotImplementedException();
        public Range<int> Range => throw new NotImplementedException();
        public byte[] GetBytes() => throw new NotImplementedException();
        public Range<int> GetRange() => throw new NotImplementedException();
        public void Add(IMemorySegment item) => throw new NotImplementedException();
        public bool ContainsIndex(int index) => throw new NotImplementedException();
        public bool IsIndexInRange(int index) => throw new NotImplementedException();
        IEnumerator IEnumerable.GetEnumerator() => throw new NotImplementedException();
        public bool Remove(IMemorySegment item) => throw new NotImplementedException();
        public int GetOffsetInSegment(int index) => throw new NotImplementedException();
        public Action<int, byte> AfterReadAction => throw new NotImplementedException();
        public Action<int, byte> AfterWriteAction => throw new NotImplementedException();
        public bool Contains(IMemorySegment item) => throw new NotImplementedException();
        public void Write(int index, ushort value) => throw new NotImplementedException();
        public IEnumerator<IMemorySegment> GetEnumerator() => throw new NotImplementedException();
        public IEnumerable<IMemorySegment> GetMemorySegments() => throw new NotImplementedException();
        public void CopyTo(IMemorySegment[] array, int arrayIndex) => throw new NotImplementedException();
        IEnumerable<IMemorySegment> IMemoryMap.GetMemorySegments() => throw new NotImplementedException();
        public void CopyBytes(ushort startAddress, Array destination, int destinationIndex, int length) => throw new NotImplementedException();
        public void BulkTransfer(ushort startAddress, Array destinationArray, int destinationIndex, int size) => throw new NotImplementedException();
    }
}
