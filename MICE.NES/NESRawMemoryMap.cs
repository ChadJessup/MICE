using MICE.Common.Interfaces;
using MICE.Common.Misc;
using MICE.Components.CPU;
using MICE.Components.Memory;
using MICE.Nintendo.Handlers;
using MICE.Nintendo.Interfaces;
using MICE.PPU.RicohRP2C02;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace MICE.Nintendo
{
    /// <summary>
    /// As basic of a memory map as possible, with the existing API.
    /// Testing out speed over readability/maintainability.
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
            public static Range ZeroPage = new Range(0x0000, 0x0FF);
            public static Range Stack = new Range(0x0100, 0x01FF);
            public static Range RAM = new Range(0x0200, 0x07FF);

            public static Range MirroredRAM = new Range(0x0800, 0x1FFF);

            public static Range RegisterRanges = new Range(0x2000, 0x401F);
            public static Range APURegisterRange = new Range(0x4000, 0x4017);

            // Expansion Memory @ 0x4020 - 0x5FFF

            public static Range SRAM = new Range(0x6000, 0x7FFF);

            public static Range ProgramRomLowerBank = new Range(0x8000, 0xBFFF);
            public static Range ProgramRomUpperBank = new Range(0xC000, 0xFFFF);

            public static Range Controller1 = new Range(0x4016, 0x4016);
            public static Range Controller2 = new Range(0x4017, 0x4017);
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

            public const int OAMDMA = 0x4014;
        }

        private static class APURegisterAddresses
        {
            public const ushort Pulse1TimerLow = 0x4002;
            public const ushort Pulse1TimerHigh = 0x4003;

            public const ushort Pulse2TimerLow = 0x4006;
            public const ushort Pulse2TimerHigh = 0x4007;

            public const ushort TriangleTimerLow = 0x400A;
            public const ushort TriangleTimerHigh = 0x400B;

            public const ushort DirectLoad = 0x4011;
            public const ushort ChannelStatus = 0x4015;
        }

        private Dictionary<int, Register8Bit> ppuRegisterLookup;
        private Dictionary<int, Register8Bit> apuRegisterLookup;

        private byte[] memory = new byte[0x10000];
        private readonly PPURegisters ppuRegisters;
        private readonly InputHandler inputHandler;

        // These memory segments are asked for specifically by the NES system
        private ExternalFacade prgROMLowerBank = null;
        private ExternalFacade prgROMUpperBank = null;
        private CPU.MOS6502.Stack stack;
        private INESInput Controller1;
        private INESInput Controller2;
        private SRAM sram;

        public NESRawMemoryMap(PPURegisters ppuRegisters, InputHandler inputHandler) : base()
        {
            this.ppuRegisters = ppuRegisters;
            this.inputHandler = inputHandler;

            this.sram = new SRAM(MemoryRanges.SRAM, "SRAM");
            this.stack = new CPU.MOS6502.Stack(MemoryRanges.Stack, "Stack", new Memory<byte>(this.memory, MemoryRanges.Stack.Min, (MemoryRanges.Stack.Max - MemoryRanges.Stack.Min) + 1));

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
                { PPURegisterAddresses.OAMDMA, ppuRegisters.OAMDMA }
            };

            this.apuRegisterLookup = new Dictionary<int, Register8Bit>()
            {
                { APURegisterAddresses.Pulse1TimerLow, null },
                { APURegisterAddresses.Pulse1TimerHigh, null },
                { APURegisterAddresses.Pulse2TimerLow, null },
                { APURegisterAddresses.Pulse2TimerHigh, null },
                { APURegisterAddresses.DirectLoad, null },
                { APURegisterAddresses.ChannelStatus, null},
                { APURegisterAddresses.TriangleTimerLow, null},
                { APURegisterAddresses.TriangleTimerHigh, null},
            };

            this.inputHandler.ControllerChanged += this.OnControllerChanged;

            this.ScopeMemoryRanges();
        }

        public Memory<byte> ZeroPage { get; private set; }
        public Memory<byte> RAM { get; private set; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte ReadByte(int index)
        {
            if (MemoryRanges.ZeroPage.IsInRange(index))
            {
                return this.ZeroPage.Span[index];
            }
            else if (MemoryRanges.Stack.IsInRange(index))
            {
                return this.stack.ReadByte(index);
            }
            else if (MemoryRanges.RAM.TryGetOffset(index, out int ramOffset))
            {
                return this.RAM.Span[ramOffset];
            }
            else if (MemoryRanges.MirroredRAM.IsInRange(index))
            {
                return this.ReadByte(index % 0x07FF);
            }
            else if (MemoryRanges.SRAM.TryGetOffset(index, out int sramOffset))
            {
                return this.sram.ReadByte(sramOffset);
            }
            else if (MemoryRanges.ProgramRomUpperBank.IsInRange(index))
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

            switch (index)
            {
                case var _ when MemoryRanges.Controller1.IsInRange(index):
                    return this.Controller1.ReadByte(index);
                case var _ when MemoryRanges.Controller2.IsInRange(index):
                    return this.Controller2?.ReadByte(index) ?? 0x0;
                case APURegisterAddresses.DirectLoad:
                case APURegisterAddresses.ChannelStatus:
                    return 0x0;
                default:
                    throw new NotImplementedException();
            }

            throw new NotImplementedException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ushort ReadShort(int index)
        {
            switch (index)
            {
                case var _ when MemoryRanges.ZeroPage.TryGetOffset(index, out int offset):
                    return (ushort)(this.ZeroPage.Span[offset + 1] << 8 | this.ZeroPage.Span[offset]); ;
                case var _ when MemoryRanges.RAM.TryGetOffset(index, out int offset):
                    return (ushort)(this.RAM.Span[offset + 1] << 8 | this.RAM.Span[offset]);
                case var _ when MemoryRanges.ProgramRomUpperBank.IsInRange(index):
                    return this.prgROMUpperBank.ReadShort(index);
                case var _ when MemoryRanges.ProgramRomLowerBank.IsInRange(index):
                    return this.prgROMLowerBank.ReadShort(index);
                default:
                    throw new NotImplementedException();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(int index, byte value)
        {
            switch (index)
            {
                case var _ when MemoryRanges.RegisterRanges.IsInRange(index):
                    this.WriteRegister(index, value);
                    break;
                case var _ when MemoryRanges.ZeroPage.TryGetOffset(index, out int offset):
                    this.ZeroPage.Span[offset] = value;
                    break;
                case var _ when MemoryRanges.RAM.TryGetOffset(index, out int offset):
                    this.RAM.Span[offset] = value;
                    break;
                case var _ when MemoryRanges.SRAM.IsInRange(index):
                    this.sram.Write(index, value);
                    break;
                case var _ when MemoryRanges.ProgramRomLowerBank.IsInRange(index):
                    this.prgROMLowerBank.Write(index, value);
                    break;
                case var _ when MemoryRanges.ProgramRomUpperBank.IsInRange(index):
                    this.prgROMUpperBank.Write(index, value);
                    break;
                case var _ when MemoryRanges.Stack.IsInRange(index):
                    this.stack.Write(index, value);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void WriteRegister(int index, byte value)
        {
            if (this.ppuRegisterLookup.TryGetValue(index, out Register8Bit ppuRegister))
            {
                ppuRegister.Write(value);
                return;
            }

            switch (index)
            {
                case PPURegisterAddresses.PPUSCROLL:
                    this.ppuRegisters.PPUSCROLL.Write(value);
                    break;
                case var _ when MemoryRanges.Controller1.IsInRange(index):
                    this.Controller1?.Write(index, value);
                    break;
                case var _ when MemoryRanges.Controller2.IsInRange(index):
                    this.Controller2?.Write(index, value);
                    break;
                default:
                    if (MemoryRanges.APURegisterRange.IsInRange(index))
                    {
                        return;
                    }

                    throw new NotImplementedException();
            }

        }

        private void ScopeMemoryRanges()
        {
            this.ZeroPage = this.GetMemory(MemoryRanges.ZeroPage);
            this.RAM = this.GetMemory(MemoryRanges.RAM);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetOffset(Range range, int index)
        {
            if (range.Min == 0)
            {
                return index;
            }

            return index - range.Min;
        }

        private Memory<byte> GetMemory(Range range) => new Memory<byte>(this.memory, range.Min, 1 + (range.Max - range.Min));

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

        public void BulkTransfer(ushort startAddress, Span<byte> destinationArray, int destinationIndex, int size)
        {
            if (MemoryRanges.RAM.TryGetOffset(startAddress, out int offset))
            {
                var ramSlice = this.RAM.Slice(offset, size);
                var oamSlice = destinationArray.Slice(destinationIndex, size);

                ramSlice.Span.CopyTo(oamSlice);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        private void OnControllerChanged(object sender, ControllerChangedArgs e)
        {
            var args = e;

            switch (args.ControllerId)
            {
                case ControllerId.Controller1:
                    this.Controller1 = args.Controller;
                    break;
                case ControllerId.Controller2:
                    this.Controller2 = args.Controller;
                    break;
                case ControllerId.Unknown:
                default:
                    throw new InvalidOperationException();
            }
        }

        // TODO: How about some base classes Chad?  No need to interface it all to hell.
        public int Count => throw new NotImplementedException();
        public string Name => throw new NotImplementedException();
        public Range Range => throw new NotImplementedException();
        public void Clear() => throw new NotImplementedException();
        public bool IsReadOnly => throw new NotImplementedException();
        public Range GetRange() => throw new NotImplementedException();
        public byte[] GetBytes() => throw new NotImplementedException();
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
        public void CopyBytes(ushort startAddress, Span<byte> destination, int destinationIndex, int length) => throw new NotImplementedException();
    }
}
