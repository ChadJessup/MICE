using MICE.Common.Interfaces;
using MICE.PPU.RicohRP2C02.Components;
using System;
using System.Collections.Generic;

namespace MICE.PPU.RicohRP2C02.Handlers
{
    public class BackgroundHandler
    {
        private readonly PPUInternalRegisters internalRegisters;
        private readonly ScrollHandler scrollHandler;
        private readonly PaletteHandler paletteHandler;
        private readonly PPURegisters registers;
        private readonly IMemoryMap ppuMemoryMap;
        private readonly IMemoryMap cpuMemoryMap;

        private readonly IList<byte[]> chrBanks;

        // Having issues, going to reproduce EmuNES's methods for now then break it down if possible.
        private ulong tileData;
        private byte nameTableByte;
        private byte attributeTableByte;
        private byte lowTileByte;
        private byte highTileByte;
        //

        public Tile PreviousTile { get; set; }
        public Tile CurrentTile { get; set; } = new Tile();

        public BackgroundHandler(IMemoryMap ppuMemoryMap, PPURegisters registers, PPUInternalRegisters internalRegisters, ScrollHandler scrollHandler, PaletteHandler paletteHandler, IMemoryMap cpuMemoryMap, IList<byte[]> chrBanks)
        {
            this.chrBanks = chrBanks;
            this.registers = registers;
            this.ppuMemoryMap = ppuMemoryMap;
            this.cpuMemoryMap = cpuMemoryMap;
            this.scrollHandler = scrollHandler;
            this.paletteHandler = paletteHandler;
            this.internalRegisters = internalRegisters;
        }

        public int BaseNametableAddress
        {
            get => (this.registers.PPUCTRL.GetBit(0) ? 1 : 0) | (this.registers.PPUCTRL.GetBit(1) ? 1 : 0) << 2;
        }

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

        public (byte drawnPixel, Tile backgroundTile) GetBackgroundPixel(int x, int y)
        {
            if (x <= 8 && !this.DrawLeft8BackgroundPixels)
            {
                return (0, null);
            }

            var tileX = x / 8;
            var tileY = y / 8;

            if (tileX == 25 && tileY == 12)
            {

            }

            if (tileX == 26 && tileY == 12)
            {

            }

            var rawTileData = (uint)(tileData >> 32);
            uint data = rawTileData >> ((7 - this.scrollHandler.FineXScroll) * 4);

            var tileByte = (byte)(data & 0x0F);

            var colorIndex = tileByte;

            var tile = new Tile();

            byte paletteId = 0;

            // Transparent background.
            if (tileByte % 4 == 0)
            {
                colorIndex = 0;
            }

            if (colorIndex != 0)
            {
                paletteId = (byte)(this.attributeTableByte & 0b00000011);
            }

            if (paletteId != this.attribute.TopLeft)
            {

            }

            var testPaletteId = this.attribute.TopLeft;//.GetPaletteId(x, y);

            if (paletteId != testPaletteId)
            {

            }

            tile.PaletteAddress = (ushort)(0x3f00 + 4 * paletteId + colorIndex);

            try
            {
                var paletteTest = this.paletteHandler.GetBackgroundColor(testPaletteId, colorIndex);
                var palette = this.ppuMemoryMap.ReadByte(tile.PaletteAddress);

                if (paletteTest != palette)
                {

                }

                return (palette, tile);
            }
            catch (Exception e)
            {
                return (0, tile);
            }
        }

        public void NextCycle() => tileData <<= 4;

        private NametableAttribute attribute;

        public void FetchNametableByte()
        {
            var address = (ushort)(0x2000 | this.internalRegisters.v & 0x0FFF);

            this.nameTableByte = this.ppuMemoryMap.ReadByte(address);
        }

        public void FetchAttributeByte()
        {
            var address = (ushort)(0x23C0 | (this.internalRegisters.v & 0x0C00) | ((this.internalRegisters.v >> 4) & 0x38) | ((this.internalRegisters.v >> 2) & 0x07));
            int shift = ((this.internalRegisters.v >> 4) & 0b00000100) | (this.internalRegisters.v & 0b00000010);

            var value = this.ppuMemoryMap.ReadByte(address);
            this.attributeTableByte = (byte)(((value >> shift) & 0b00000011) << 2);
            this.attribute = new NametableAttribute(value, address);
        }

        public void FetchLowBGTile()
        {
            var baseAddress = this.IsBackgroundPatternTableAddress1000
                ? 0x1000
                : 0x0000;

            ushort address = (ushort)(baseAddress + (this.nameTableByte * 16) + this.scrollHandler.vFineYScroll);
            this.lowTileByte = this.ppuMemoryMap.ReadByte(address);
        }

        public void FetchHighBGTile()
        {
            var baseAddress = this.IsBackgroundPatternTableAddress1000
                ? 0x1000
                : 0x1000;

            ushort address = (ushort)(baseAddress + (this.nameTableByte * 16) + this.scrollHandler.vFineYScroll + 8);
            this.highTileByte = this.ppuMemoryMap.ReadByte(address);
        }

        // Temp from EmuNES...
        public void StoreTileData()
        {
            uint data = 0;

            for (int i = 0; i < 8; i++)
            {
                int p1 = (lowTileByte & 0x80) >> 7;
                int p2 = (highTileByte & 0x80) >> 6;

                lowTileByte <<= 1;
                highTileByte <<= 1;
                data <<= 4;

                data |= (uint)(attributeTableByte | p1 | p2);
            }

            tileData |= (ulong)(data);
        }
        //
    }
}