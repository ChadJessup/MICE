using MICE.Common.Interfaces;
using MICE.Components.Bus;
using MICE.Components.Memory;
using MICE.CPU.MOS6502;
using MICE.Nintendo.Loaders;
using MICE.PPU.RicohRP2C02;
using MICE.PPU.RicohRP2C02.Components;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MICE.Nintendo
{
    public class NES : ISystem
    {
        private CancellationToken cancellationToken;
        public long CurrentFrame { get; private set; } = 0;

        public NES(CancellationToken cancellationToken)
        {
            if (File.Exists(this.debugPath))
            {
                File.Delete(this.debugPath);
            }

         //   this.sw = File.AppendText(this.debugPath);

            this.cancellationToken = cancellationToken;
        }

        public string Name { get; } = "Nintendo Entertainment System";
        private string debugPath = @"c:\emulators\nes\debug-mice.txt";

        public StreamWriter sw;

        // Create components...
        public DataBus DataBus { get; } = new DataBus();
        public AddressBus AddressBus { get; } = new AddressBus();
        public ControlBus ControlBus { get; } = new ControlBus();

        public RicohRP2C02 PPU { get; private set; }
        public NESMemoryMap CPUMemoryMap { get; private set; }

        public NESCartridge Cartridge { get; private set; }
        public Ricoh2A03 CPU { get; private set; }
        public byte[] Screen { get; private set; } = new byte[256 * 240];

        // Hook them up...

        public void PowerOn()
        {
            if (this.Cartridge == null)
            {
                throw new InvalidOperationException("Cartridge must be loaded first, unable to power on.");
            }

            var ppuRegisters = new PPURegisters();
            this.CPUMemoryMap = new NESMemoryMap(ppuRegisters, this.sw);
            this.PPU = new RicohRP2C02(new PPUMemoryMap(this.sw), ppuRegisters, this.CPUMemoryMap, this.Cartridge.CharacterRomBanks);

            this.PPU.Registers.OAMDMA.AfterWriteAction = this.DMATransfer;

            this.MapToCartridge();

            this.CPU = new Ricoh2A03(this.CPUMemoryMap, this.sw);

            this.CPU.PowerOn(this.cancellationToken);
            this.PPU.PowerOn(this.cancellationToken);
        }

        private void MapToCartridge()
        {
            // Little confused here...https://wiki.nesdev.com/w/index.php/CPU_memory_map
            // states "battery backed save OR Work RAM.  The 01-Implied test cartridge, has no SRAM, but writes to it...
            // so if cartridge doesn't have ram let's just put some memory there anyways!
            this.CPUMemoryMap.GetMemorySegment<SRAM>("SRAM").Data = this.Cartridge.SRAM ?? new byte[0x7fff - 0x6000];

            // Various parts of a cartridge are mapped into the NES's CPU memory map.
            this.CPUMemoryMap.GetMemorySegment<External>("PRG-ROM Lower Bank").AttachHandler(this.Cartridge.Mapper);
            this.CPUMemoryMap.GetMemorySegment<External>("PRG-ROM Upper Bank").AttachHandler(this.Cartridge.Mapper);

            // Various parts of a cartridge are mapped into the NES's PPU memory map.
            this.PPU.MemoryMap.GetMemorySegment<PatternTable>("Pattern Table 0").AttachHandler(this.Cartridge.Mapper);
            this.PPU.MemoryMap.GetMemorySegment<PatternTable>("Pattern Table 1").AttachHandler(this.Cartridge.Mapper);

            this.PPU.MemoryMap.GetMemorySegment<Nametable>("Name Table 0").AttachHandler(this.Cartridge.Mapper);
            this.PPU.MemoryMap.GetMemorySegment<Nametable>("Name Table 1").AttachHandler(this.Cartridge.Mapper);
            this.PPU.MemoryMap.GetMemorySegment<Nametable>("Name Table 2").AttachHandler(this.Cartridge.Mapper);
            this.PPU.MemoryMap.GetMemorySegment<Nametable>("Name Table 3").AttachHandler(this.Cartridge.Mapper);
        }

        public void Step()
        {
            // 1 Step = 1 Frame to the NES, since we're doing frame-based timing here:
            // 1 System step = 1 CPU step + (3 PPU steps * CPU Cycles in Step) + (2 Audio steps * 1 CPU cycle).
            // Cycles are based on which instructions the CPU ran.

            var cpuCycles = this.CPU.Step();
            CPU.CurrentCycle += cpuCycles;

            for (int i = 0; i < cpuCycles * 3; i++)
            {
                var ppuCycles = this.PPU.Step();

                if (this.PPU.ShouldNMInterrupt)
                {
                    this.CPU.WasNMIRequested = true;
                    this.PPU.WasNMIRequested = false;
                    this.PPU.ShouldNMInterrupt = false;
                }
            }

            if (this.PPU.FrameNumber > this.CurrentFrame)
            {
                this.CurrentFrame = this.PPU.FrameNumber;
                Array.Copy(this.PPU.ScreenData, this.Screen, this.PPU.ScreenData.Length);
            }

            // TODO: APU Cycles
        }

        public async Task PowerOff()
        {
            await Task.CompletedTask;
        }

        public async Task Reset()
        {
            await Task.CompletedTask;
        }

        public void DMATransfer(int? address, byte value)
        {
            ushort readAddress = (ushort)(value << 8);
            //this.sw.WriteLine("DMA Just Happened!");

            this.CPUMemoryMap.BulkTransfer(readAddress, this.PPU.PrimaryOAM.Data, this.PPU.Registers.OAMADDR, 256);
        }

        public Task Run() => Task.Factory.StartNew(() =>
        {
            while (!this.cancellationToken.IsCancellationRequested)
            {
                this.Step();

                // TODO: Get RAW Screen data from PPU.
                // TODO: Audio.
            }
        });

        /// <summary>
        /// Loads an <seealso cref="NESCartridge"/> into the NES system.
        /// </summary>
        /// <param name="cartridge">The cartridge to load.</param>
        public void LoadCartridge(NESCartridge cartridge)
        {
            this.Cartridge = cartridge;
        }
    }
}
