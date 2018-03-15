﻿using MICE.Common.Interfaces;
using MICE.PPU.RicohRP2C02.Components;
using System;
using System.Collections.Generic;

namespace MICE.PPU.RicohRP2C02
{
    public class BackgroundHandler
    {
        private readonly PPURegisters registers;
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

        public BackgroundHandler(IMemoryMap ppuMemoryMap, PPURegisters registers, IMemoryMap cpuMemoryMap, IList<byte[]>chrBanks)
        {
            this.chrBanks = chrBanks;
            this.ppuMemoryMap = ppuMemoryMap;
            this.registers = registers;
            this.cpuMemoryMap = cpuMemoryMap;

            this.scrollHandler = new ScrollHandler(registers);

            this.CacheMemorySegments(ppuMemoryMap);
        }

        // Cache memory segments that are used heavily....
        // TODO: Find a better way, this sucks.
        private void CacheMemorySegments(IMemoryMap ppuMemoryMap)
        {
            this.nameTable0 = ppuMemoryMap.GetMemorySegment<Nametable>("Name Table 0");
            this.nameTable1 = ppuMemoryMap.GetMemorySegment<Nametable>("Name Table 1");
            this.nameTable2 = ppuMemoryMap.GetMemorySegment<Nametable>("Name Table 2");
            this.nameTable3 = ppuMemoryMap.GetMemorySegment<Nametable>("Name Table 3");

            this.imagePalette = ppuMemoryMap.GetMemorySegment<Palette>("Image Palette");
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

        public (byte, Tile) DrawBackgroundPixel(int x, int y)
        {
            if (x <= 8 && !this.DrawLeft8BackgroundPixels)
            {
                return (0, null);
            }

            var (indexedX, indexedY, nameTable) = this.SelectNametable(x, y);

            var tile = nameTable.GetTileFromPixel(indexedX, indexedY, this.IsBackgroundPatternTableAddress1000 ? 0x1000 : 0x0000, this.chrBanks[0]);
            var palette = this.ppuMemoryMap.ReadByte(tile.PaletteAddress);

            return (palette, tile);
        }

        private (int indexedX, int indexedY, Nametable nameTable) SelectNametable(int x, int y)
        {
            var (scrollX, scrollY) = this.scrollHandler.GetScrollValues();

            int nameTableId = 0;
            int indexedX = (scrollY + x) % 512;

            if (indexedX >= 256)
            {
                nameTableId += 1;
                indexedX -= 256;
            }

            int indexedY = (scrollY + y) % 480;
            if (indexedY >= 240)
            {
                nameTableId += 2;
                indexedY -= 240;
            }

            return (indexedX, indexedY, this.GetTable(nameTableId));
        }

        // TODO: mirroring...
        private Nametable GetTable(int nametableId)
        {
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
    }
}