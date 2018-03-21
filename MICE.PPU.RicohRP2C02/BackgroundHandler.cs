using MICE.Common.Interfaces;
using MICE.PPU.RicohRP2C02.Components;
using System;
using System.Collections.Generic;

namespace MICE.PPU.RicohRP2C02
{
    public class BackgroundHandler
    {
        private readonly PPURegisters registers;
        private readonly PPUInternalRegisters internalRegisters;
        private readonly IMemoryMap ppuMemoryMap;
        private readonly IMemoryMap cpuMemoryMap;
        private readonly ScrollHandler scrollHandler;

        private Nametable nameTable0;
        private Nametable nameTable1;
        private Nametable nameTable2;
        private Nametable nameTable3;

        private Nametable currentNameTable;

        private Palette imagePalette;
        private readonly IList<byte[]> chrBanks;

        public BackgroundHandler(IMemoryMap ppuMemoryMap, PPURegisters registers, PPUInternalRegisters internalRegisters, ScrollHandler scrollHandler, IMemoryMap cpuMemoryMap, IList<byte[]> chrBanks)
        {
            this.chrBanks = chrBanks;
            this.ppuMemoryMap = ppuMemoryMap;
            this.registers = registers;
            this.internalRegisters = internalRegisters;
            this.cpuMemoryMap = cpuMemoryMap;
            this.scrollHandler = scrollHandler;

            this.CacheMemorySegments(ppuMemoryMap);
        }

        // Cache memory segments that are used heavily....
        private void CacheMemorySegments(IMemoryMap ppuMemoryMap)
        {
            this.nameTable0 = ppuMemoryMap.GetMemorySegment<Nametable>("Name Table 0");
            this.nameTable1 = ppuMemoryMap.GetMemorySegment<Nametable>("Name Table 1");
            this.nameTable2 = ppuMemoryMap.GetMemorySegment<Nametable>("Name Table 2");
            this.nameTable3 = ppuMemoryMap.GetMemorySegment<Nametable>("Name Table 3");

            this.imagePalette = ppuMemoryMap.GetMemorySegment<Palette>("Image Palette");
        }

        public int BaseNametableAddress => (this.registers.PPUCTRL.GetBit(0) ? 1 : 0) | (this.registers.PPUCTRL.GetBit(1) ? 1 : 0) << 2;

        public bool DrawLeft8BackgroundPixels
        {
            get => this.registers.PPUMASK.GetBit(1);
            set => this.registers.PPUMASK.SetBit(1, value);
        }

        public bool ShowBackground
        {
            get => this.registers.PPUMASK.GetBit(3);
            set => this.registers.PPUMASK.SetBit(3, value);
        }

        public bool IsBackgroundPatternTableAddress1000
        {
            get => this.registers.PPUCTRL.GetBit(4);
            set => this.registers.PPUCTRL.SetBit(4, value);
        }

        public (byte, Tile) GetBackgroundPixel(int x, int y)
        {
            if (x <= 8 && !this.DrawLeft8BackgroundPixels)
            {
                return (0, null);
            }

            var nameTable = this.GetTable(this.scrollHandler.vNametable);

            // var (scrolledX, scrolledY, nameTable) = this.GetScrolledXYAndNametable(x, y);

            var tile = nameTable.GetTileFromPixel(this.scrollHandler,
                this.IsBackgroundPatternTableAddress1000 ? 0x1000 : 0x0000,
                this.chrBanks[0], this.internalRegisters);

            var palette = this.ppuMemoryMap.ReadByte(tile.PaletteAddress);

            return (palette, tile);
        }

        private (int scrolledX, int scrolledY, Nametable nameTable) GetScrolledXYAndNametable(int x, int y)
        {
            var (scrollX, scrollY) = this.scrollHandler.GetScrollValues();

            return (this.scrollHandler.tCoarseXScroll, this.scrollHandler.tCoarseYScroll, this.GetTable(this.scrollHandler.tNametable));
        }

        private Nametable GetTable(int nametableId)
        {
            // TODO: mirroring...
            switch (nametableId)
            {
                case 0:
                    return this.nameTable0;
                case 1:
                    return this.nameTable1;
                case 2:
                    return this.nameTable2;
                case 3:
                    return this.nameTable3;
                default:
                    throw new InvalidOperationException($"Unexpected Nametable Id requested: {nametableId}");
            }
        }

        public void FetchAttributeByte()
        {
            ushort address = (ushort)(0x23C0 | (this.internalRegisters.v & 0x0C00) | ((this.internalRegisters.v >> 4) & 0x38) | ((this.internalRegisters.v >> 2) & 0x07));
            int shift = ((this.internalRegisters.v >> 4) & 4) | (this.internalRegisters.v & 2);
            var atbyte = (byte)(((this.ppuMemoryMap.ReadByte(address) >> shift) & 3) << 2);
        }

        private byte nameTableByte = 0;
        public void FetchNametableByte()
        {
            ushort address = (ushort)(0x2000 | this.internalRegisters.v & 0x0FFF);
            this.nameTableByte = this.ppuMemoryMap.ReadByte(address);
        }

        public void FetchHighBGTile()
        {
            int fineY = (this.internalRegisters.v >> 12) & 7;
            ushort address = (ushort)(this.BaseNametableAddress + nameTableByte * 16 + fineY + 8);
            var highTileByte = this.ppuMemoryMap.ReadByte(address);
        }

        public void FetchLowBGTile()
        {
            int fineY = (this.internalRegisters.v >> 12) & 7;
            ushort address = (ushort)(this.BaseNametableAddress + nameTableByte * 16 + fineY);
            var lowTileByte = this.ppuMemoryMap.ReadByte(address);
        }
    }
}