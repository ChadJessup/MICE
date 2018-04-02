using MICE.Common.Interfaces;
using MICE.Components.Bus;
using MICE.Components.Memory;
using MICE.CPU.MOS6502;
using MICE.Nintendo.Loaders;
using MICE.PPU.RicohRP2C02;
using MICE.PPU.RicohRP2C02.Components;
using System;
using System.IO;
using System.Threading.Tasks;

namespace MICE.Nintendo
{
    public class NES : ISystem
    {
        public NES()
        {
            if (File.Exists(this.debugPath))
            {
                File.Delete(this.debugPath);
            }

         //   this.sw = File.AppendText(this.debugPath);
        }

        public string Name { get; } = "Nintendo Entertainment System";
        public long CurrentFrame { get; private set; } = 0;

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

        public bool IsPoweredOn => (this.CPU?.IsPowered ?? false) && (this.PPU?.IsPowered ?? false);

        public bool IsPaused { get; private set; } = false;

        // Hook them up...
        public void PowerOn()
        {
            if (this.Cartridge == null)
            {
                throw new InvalidOperationException("Cartridge must be loaded first, unable to power on.");
            }

            var ppuRegisters = new PPURegisters();
            this.CPUMemoryMap = new NESMemoryMap(ppuRegisters, this.sw);
            this.PPU = new RicohRP2C02(new PPUMemoryMap(this.sw), ppuRegisters, this.CPUMemoryMap);

            this.PPU.Registers.OAMDMA.AfterWriteAction = this.DMATransfer;

            this.MapToCartridge();

            this.CPU = new Ricoh2A03(this.CPUMemoryMap, this.sw);

            this.CPU.PowerOn();
            this.PPU.PowerOn();

            this.IsPaused = false;
        }

        private void MapToCartridge()
        {
            this.CPUMemoryMap.GetMemorySegment<SRAM>("SRAM").AttachHandler(this.Cartridge.Mapper);

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

        private long ppuTotalCycles = 0;

        public void Step()
        {
            // 1 Step = 1 Frame to the NES, since we're doing frame-based timing here:
            // 1 System step = 1 CPU step + (3 PPU steps * CPU Cycles in Step) + (2 Audio steps * 1 CPU cycle).
            // Cycles are based on which instructions the CPU ran.
            var cycleEvent = new NintendoStepArgs();

            cycleEvent.CPUStepsOccurred = this.CPU.Step();
            CPU.CurrentCycle += cycleEvent.CPUStepsOccurred;

            cycleEvent.TotalCPUSteps = CPU.CurrentCycle;

            for (int i = 0; i < cycleEvent.CPUStepsOccurred * 3; i++)
            {
                cycleEvent.PPUStepsOccurred = this.PPU.Step();
                this.ppuTotalCycles += cycleEvent.PPUStepsOccurred;

                cycleEvent.TotalPPUSteps = this.ppuTotalCycles;

                if (this.PPU.ShouldNMInterrupt)
                {
                    this.CPU.WasNMIRequested = true;
                    this.PPU.WasNMIRequested = false;
                    this.PPU.ShouldNMInterrupt = false;
                }
            }

            if (this.PPU.FrameNumber > this.CurrentFrame)
            {
                //Console.WriteLine($"Finished Frame: {this.CurrentFrame}");

                this.CurrentFrame = this.PPU.FrameNumber;
                Array.Copy(this.PPU.ScreenData, this.Screen, this.PPU.ScreenData.Length);
            }

            // TODO: APU Cycles

            this.StepCompleted?.Invoke(this, cycleEvent);
        }

        public void PowerOff()
        {
            this.CPU.PowerOff();
            this.PPU.PowerOff();
            this.PPU.ClearOutput();
        }

        public void Reset()
        {
            this.PowerOn();
            this.Run();
        }

        public void DMATransfer(int? address, byte value)
        {
            ushort readAddress = (ushort)(value << 8);
            //this.sw.WriteLine("DMA Just Happened!");

            this.CPU.CurrentOpcode.AddedCycles +=
                (this.CPU.CurrentCycle % 2) == 1
                ? 514
                : 513;

            this.CPUMemoryMap.BulkTransfer(readAddress, this.PPU.PrimaryOAM.Data, this.PPU.Registers.OAMADDR, 256);
        }

        public Task Run()
        {
            return Task.Factory.StartNew(() =>
            {
                while (!this.IsPaused && this.IsPoweredOn)
                {
                    try
                    {
                        this.Step();
                    }
                    catch (Exception e)
                    {
                    }

                    // TODO: Get RAW Screen data from PPU.
                    // TODO: Audio.
                }
            });
        }

        /// <summary>
        /// Loads an <seealso cref="NESCartridge"/> into the NES system.
        /// </summary>
        /// <param name="cartridge">The cartridge to load.</param>
        public void LoadCartridge(NESCartridge cartridge)
        {
            this.Cartridge = cartridge;
            this.CartridgeLoaded?.Invoke(this, new CartridgeLoadedArgs(this.Cartridge));
        }

        public void Pause(bool isPaused = true)
        {
            this.IsPaused = isPaused;

            if (!this.IsPaused)
            {
                this.Run();
            }
        }

        public EventHandler<NintendoStepArgs> StepCompleted { get; set; }
        public EventHandler<FrameCompleteArgs> FrameFinished { get; set; }
        public EventHandler<CartridgeLoadedArgs> CartridgeLoaded { get; set; }
    }
}
