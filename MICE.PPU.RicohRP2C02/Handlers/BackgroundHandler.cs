using MICE.Common.Interfaces;
using MICE.PPU.RicohRP2C02.Components;
using Ninject;

namespace MICE.PPU.RicohRP2C02.Handlers
{
    public class BackgroundHandler
    {
        private readonly PPUInternalRegisters internalRegisters;
        private readonly PaletteHandler paletteHandler;
        private readonly ScrollHandler scrollHandler;
        private readonly IMemoryMap ppuMemoryMap;
        private readonly IMemoryMap cpuMemoryMap;
        private readonly PPURegisters registers;

        // Having issues, going to reproduce EmuNES's methods for now then break it down if possible.
        private ulong tileData;
        private byte nameTableByte;
        // private byte attributeTableByte;
        private byte lowTileByte;
        private byte highTileByte;
        //

        public Tile PreviousTile { get; set; }
        public Tile CurrentTile { get; set; } = new Tile();

        public BackgroundHandler([Named("PPU")] IMemoryMap ppuMemoryMap, PPURegisters registers, PPUInternalRegisters internalRegisters, ScrollHandler scrollHandler, PaletteHandler paletteHandler, [Named("CPU")] IMemoryMap cpuMemoryMap)
        {
            this.registers = registers;
            this.ppuMemoryMap = ppuMemoryMap;
            this.cpuMemoryMap = cpuMemoryMap;
            this.scrollHandler = scrollHandler;
            this.paletteHandler = paletteHandler;
            this.internalRegisters = internalRegisters;
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

        public byte GetBackgroundPixel(int x, int y)
        {
            if (x <= 7 && !this.DrawLeft8BackgroundPixels)
            {
                return 0;
            }

            var rawTileData = (uint)(tileData >> 32);
            uint data = rawTileData >> ((7 - this.scrollHandler.FineXScroll) * 4);

            var tileByte = (byte)(data & 0x0F);

            this.CurrentTile.ColorIndex = tileByte;

            // Transparent background.
            if (tileByte % 4 == 0)
            {
                this.CurrentTile.ColorIndex = 0;
            }

            this.CurrentTile.TileByte = tileByte;

            var paletteId = this.currentAttribute.GetPaletteOffset();

            this.CurrentTile.PaletteAddress = (ushort)(0x3f00 + 4 * paletteId + this.CurrentTile.ColorIndex);

            return this.paletteHandler.GetBackgroundColor(paletteId, this.CurrentTile.ColorIndex);
        }

        public void NextCycle() => tileData <<= 4;

        private NametableAttribute currentAttribute = new NametableAttribute();

        public void FetchNametableByte()
        {
            var address = (ushort)(0x2000 | this.internalRegisters.v & 0x0FFF);

            this.nameTableByte = this.ppuMemoryMap.ReadByte(address);
        }

        public void FetchAttributeByte()
        {
            var v = this.internalRegisters.v;
            var address = (ushort)(0x23C0 | (v & 0x0C00) | ((v >> 4) & 0x38) | ((v >> 2) & 0x07));

            this.currentAttribute.RawByte = this.ppuMemoryMap.ReadByte(address);
            this.currentAttribute.Address = v;
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
                : 0x0000;

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

                data |= (uint)(this.currentAttribute.AttributeTableByte | p1 | p2);
            }

            tileData |= (ulong)(data);
        }
        //
    }
}