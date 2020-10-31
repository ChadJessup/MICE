using MICE.Common.Interfaces;
using MICE.PPU.RicohRP2C02.Components;

namespace MICE.PPU.RicohRP2C02.Handlers
{
    public class BackgroundHandler
    {
        private readonly PPUInternalRegisters internalRegisters;
        private readonly PaletteHandler paletteHandler;
        private readonly ScrollHandler scrollHandler;
        private readonly IPPUMemoryMap ppuMemoryMap;
        private readonly PPURegisters registers;

        // Having issues, going to reproduce EmuNES's methods for now then break it down if possible.
        private ulong tileData;
        private byte nameTableByte;
        // private byte attributeTableByte;
        private byte lowTileByte;
        private byte highTileByte;
        //

        private Nametable nameTable0;
        private Nametable nameTable1;
        private Nametable nameTable2;
        private Nametable nameTable3;

        private PatternTable patternTable0;
        private PatternTable patternTable1;

        public Tile PreviousTile { get; set; }
        public Tile CurrentTile { get; set; } = new Tile();

        public BackgroundHandler(
            IPPUMemoryMap ppuMemoryMap,
            PPURegisters registers,
            PPUInternalRegisters internalRegisters,
            ScrollHandler scrollHandler,
            PaletteHandler paletteHandler)
        {
            this.registers = registers;
            this.ppuMemoryMap = ppuMemoryMap;
            this.scrollHandler = scrollHandler;
            this.paletteHandler = paletteHandler;
            this.internalRegisters = internalRegisters;

            foreach (var patternTable in this.ppuMemoryMap.GetMemorySegments<PatternTable>())
            {
                if (patternTable.Range.Min == 0x0000)
                {
                    this.patternTable0 = patternTable;
                }
                else if (patternTable.Range.Min == 0x1000)
                {
                    this.patternTable1 = patternTable;
                }
            }

            foreach (var nameTable in this.ppuMemoryMap.GetMemorySegments<Nametable>())
            {
                if (nameTable.Range.Min == 0x2000)
                {
                    this.nameTable0 = nameTable;
                }
                else if (nameTable.Range.Min == 0x2400)
                {
                    this.nameTable1 = nameTable;
                }
                else if (nameTable.Range.Min == 0x2800)
                {
                    this.nameTable2 = nameTable;
                }
                else if (nameTable.Range.Min == 0x2C00)
                {
                    this.nameTable3 = nameTable;
                }
            }
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

            ushort address = (ushort)((this.nameTableByte * 16) + this.scrollHandler.vFineYScroll);

            this.lowTileByte = this.IsBackgroundPatternTableAddress1000
                ? this.patternTable1.ReadByte(baseAddress + address)
                : this.patternTable0.ReadByte(baseAddress + address);
        }

        public void FetchHighBGTile()
        {
            var baseAddress = this.IsBackgroundPatternTableAddress1000
              ? 0x1000
              : 0x0000;

            ushort address = (ushort)((this.nameTableByte * 16) + this.scrollHandler.vFineYScroll + 8);

            this.highTileByte = this.IsBackgroundPatternTableAddress1000
                ? this.patternTable1.ReadByte(baseAddress + address)
                : this.patternTable0.ReadByte(baseAddress + address);
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

        private byte GetByteFromNameTables(ushort address)
        {
            if (this.nameTable0.Range.TryGetOffset(address, out int ntoffset0))
            {
                return this.nameTable0.Data[ntoffset0];
            }
            else if (this.nameTable1.Range.TryGetOffset(address, out int ntoffset1))
            {
                return this.nameTable1.Data[ntoffset1];
            }
            else if (this.nameTable2.Range.TryGetOffset(address, out int ntoffset2))
            {
                return this.nameTable2.Data[ntoffset2];
            }
            else if (this.nameTable3.Range.TryGetOffset(address, out int ntoffset3))
            {
                return this.nameTable3.Data[ntoffset3];
            }

            return 0x0;
        }
    }
}