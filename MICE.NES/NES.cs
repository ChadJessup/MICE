using MICE.Common.Interfaces;
using MICE.Components.Bus;
using MICE.Components.Memory;
using MICE.CPU.MOS6502;
using MICE.Nintendo.Loaders;
using MICE.PPU.RicohRP2C02;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MICE.Nintendo
{
    public class NES : ISystem
    {
        private CancellationToken cancellationToken;

        public NES(CancellationToken cancellationToken)
        {
            this.cancellationToken = cancellationToken;
        }

        public string Name { get; } = "Nintendo Entertainment System";

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

            this.PPU = new RicohRP2C02();
            this.MemoryMap = new CPUMemoryMap(this.PPU);

            this.MapToCartridge();

            this.CPU = new Ricoh2A03(this.MemoryMap);

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
            // 1 Step = 1 Frame to the NES. Since we're doing frame-based timing here, which seems to be the preferred method to emulate a console.
            // 1 System step = 1 CPU step + (3 PPU steps * CPU Cycles in Step) + (1 Audio step * CPU cycles).
            // Cycles are based on which instructions the CPU ran.

            var cpuCycles = this.CPU.Step();

            for (int i = 0; i < cpuCycles * 3; i++)
            {
                var ppuCycles = this.PPU.Step();
            }

            // TODO: APU Cycles

            CPU.CurrentCycle += cpuCycles;
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
