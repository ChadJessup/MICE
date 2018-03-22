﻿using MICE.Common.Interfaces;
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

        // Having issues, going to reproduce EmuNES's methods for now then break it down if possible.
        private ulong tileData;
        private byte nameTableByte;
        private byte attributeTableByte;
        private byte lowTileByte;
        private byte highTileByte;
        //

        public Tile PreviousTile { get; set; }
        public Tile CurrentTile { get; set; } = new Tile();

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

        public (byte, Tile) GetBackgroundPixel(int x, int y)
        {
            if (x <= 8 && !this.DrawLeft8BackgroundPixels)
            {
                return (0, null);
            }

            var nameTable = this.GetTable(this.scrollHandler.vNametable);

            // var (scrolledX, scrolledY, nameTable) = this.GetScrolledXYAndNametable(x, y);
            uint data = FetchTileData() >> ((7 - this.scrollHandler.FineXScroll) * 4);

            var tileByte = (byte)(data & 0x0F);

            var colorIndex = tileByte;

            var tile = new Tile();

            byte paletteId = 0;
            if (colorIndex != 0)
            {
                paletteId = (byte)((tile.AttributeData >> 0) & 3);
            }

            // (ushort)(0x3f00 + 4 * paletteId + colorIndex)
            tile.PaletteAddress = (ushort)(0x3f00 + 4 * paletteId + colorIndex);

            //var tile = nameTable.GetTileFromPixel(x, y, this.scrollHandler, this.PreviousTile,
            //    this.IsBackgroundPatternTableAddress1000 ? 0x1000 : 0x0000,
            //    this.chrBanks[0], this.internalRegisters);

            var palette = this.ppuMemoryMap.ReadByte(tile.PaletteAddress);

            return (palette, tile);
        }

        public void NextCycle() => tileData <<= 4;

        public void FetchAttributeByte()
        {
            var address = (ushort)(0x23C0 | (this.internalRegisters.v & 0x0C00) | ((this.internalRegisters.v >> 4) & 0x38) | ((this.internalRegisters.v >> 2) & 0x07));
            int shift = ((this.internalRegisters.v >> 4) & 4) | (this.internalRegisters.v & 2);
            this.attributeTableByte = (byte)(((this.ppuMemoryMap.ReadByte(address) >> shift) & 3) << 2);

            //this.CurrentTile.AttributeAddress = (ushort)(0x23C0 | (this.internalRegisters.v & 0x0C00) | ((this.internalRegisters.v >> 4) & 0x38) | ((this.internalRegisters.v >> 2) & 0x07));
            //int shift = ((this.internalRegisters.v >> 4) & 4) | (this.internalRegisters.v & 2);
            //this.CurrentTile.AttributeData = (byte)(((this.ppuMemoryMap.ReadByte(this.CurrentTile.AttributeAddress) >> shift) & 3) << 2);
        }

        public void FetchNametableByte()
        {
            var address = (ushort)(0x2000 | this.internalRegisters.v & 0x0FFF);

            this.nameTableByte = this.ppuMemoryMap.ReadByte(address);

            //this.CurrentTile.Nametable = this.scrollHandler.vNametable;
            //this.CurrentTile.TileAddress = (ushort)(0x2000 | this.internalRegisters.v & 0x0FFF);
            //this.CurrentTile.nameTableByte = this.ppuMemoryMap.ReadByte(this.CurrentTile.TileAddress);

            //if(this.CurrentTile.nameTableByte != 0x0)
            //{

            //}
        }

        public void FetchHighBGTile()
        {
            var baseAddress = this.IsBackgroundPatternTableAddress1000
                ? 0x1000
                : 0x1000;

            ushort address = (ushort)(baseAddress + (this.nameTableByte * 16) + this.scrollHandler.vFineYScroll + 8);
            this.highTileByte = this.ppuMemoryMap.ReadByte(address);
        }

        public void FetchLowBGTile()
        {
            var baseAddress = this.IsBackgroundPatternTableAddress1000
                ? 0x1000
                : 0x0000;

            ushort address = (ushort)(baseAddress + (this.nameTableByte * 16) + this.scrollHandler.vFineYScroll);
            this.lowTileByte = this.ppuMemoryMap.ReadByte(address);
        }

        private (int scrolledX, int scrolledY, Nametable nameTable) GetScrolledXYAndNametable(int x, int y)
        {
            var (scrollX, scrollY) = this.scrollHandler.GetScrollValues();

            return (this.scrollHandler.tCoarseXScroll, this.scrollHandler.tCoarseYScroll, this.GetTable(this.scrollHandler.tNametable));
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

            if(tileData != 0x0)
            {

            }
        }

        private uint FetchTileData()
        {
            return (uint)(tileData >> 32);
        }
        //
    }
}