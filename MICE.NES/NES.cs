using MICE.Common.Interfaces;
using MICE.Components.Bus;
using MICE.Components.Memory;
using MICE.CPU.MOS6502;
using MICE.Nintendo.Loaders;
using MICE.PPU.RicohRP2C02;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MICE.Nintendo
{
    public class NES : ISystem
    {
        private CancellationToken cancellationToken;
        private long currentFrame = 0;

        public NES(CancellationToken cancellationToken)
        {
            if (File.Exists(this.debugPath))
            {
                File.Delete(this.debugPath);
            }

            this.sw = File.AppendText(this.debugPath);

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
        public CPUMemoryMap MemoryMap { get; private set; }

        public NESCartridge Cartridge { get; private set; }
        public Ricoh2A03 CPU { get; private set; }

        // Hook them up...

        public void PowerOn()
        {
            if (this.Cartridge == null)
            {
                throw new InvalidOperationException("Cartridge must be loaded first, unable to power on.");
            }

            var ppuRegisters = new PPURegisters();
            this.MemoryMap = new CPUMemoryMap(ppuRegisters, this.sw);
            this.PPU = new RicohRP2C02(new PPUMemoryMap(this.sw), ppuRegisters, this.MemoryMap);

            this.PPU.Registers.OAMDMA.AfterWriteAction = this.DMATransfer;

            this.MapToCartridge();

            this.CPU = new Ricoh2A03(this.MemoryMap, this.sw);

            this.CPU.PowerOn(this.cancellationToken);
            this.PPU.PowerOn(this.cancellationToken);
        }

        private void MapToCartridge()
        {
            // Various parts of a cartridge are mapped into the NES's memory map.
            this.MemoryMap.GetMemorySegment<SRAM>("SRAM").Data = this.Cartridge.SRAM;

            this.MemoryMap.GetMemorySegment<External>("PRG-ROM Lower Bank").AttachHandler(this.Cartridge.Mapper);
            this.MemoryMap.GetMemorySegment<External>("PRG-ROM Upper Bank").AttachHandler(this.Cartridge.Mapper);
        }

        public void Step()
        {
            // 1 Step = 1 Frame to the NES, since we're doing frame-based timing here:
            // 1 System step = 1 CPU step + (3 PPU steps * CPU Cycles in Step) + (2 Audio steps * 1 CPU cycle).
            // Cycles are based on which instructions the CPU ran.

            Stopwatch cpuSW = new Stopwatch();
            cpuSW.Start();
            var cpuCycles = this.CPU.Step();
            CPU.CurrentCycle += cpuCycles;
            cpuSW.Stop();

            for (int i = 0; i < cpuCycles * 3; i++)
            {
                var ppuCycles = this.PPU.Step();

                if (this.PPU.WasNMIRequested)
                {
                    this.CPU.WasNMIRequested = true;
                    this.PPU.WasNMIRequested = false;
                }
            }

            this.currentFrame = this.PPU.Frame;
            // TODO: APU Cycles
        }

        private void SetupOpcodes()
        {
            throw new NotImplementedException();
        }

        public async Task PowerOff()
        {
            await Task.CompletedTask;
        }

        public async Task Reset()
        {
            await Task.CompletedTask;
        }

        public void DMATransfer(byte value)
        {
            ushort readAddress = (ushort)(value << 8);

            // TODO: This is terrible, and a double copy...convert to stream later through a bus or something.
            // Especially since this is normally DRAM and refreshed all the time from what I can tell?
            var copiedBytes = this.MemoryMap.BulkTransfer(readAddress, 255);
            Array.Copy(copiedBytes, 0, this.PPU.PrimaryOAM, this.PPU.Registers.OAMADDR, 255);
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
