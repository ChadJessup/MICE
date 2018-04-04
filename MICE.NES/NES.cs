using MICE.Common.Interfaces;
using MICE.Components.Bus;
using MICE.Components.Memory;
using MICE.CPU.MOS6502;
using MICE.Nintendo.Components;
using MICE.Nintendo.Handlers;
using MICE.Nintendo.Loaders;
using MICE.PPU.RicohRP2C02;
using MICE.PPU.RicohRP2C02.Components;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace MICE.Nintendo
{
    public class NES : ISystem
    {
        private static class Constants
        {
            public const string DebugFile = @"C:\Users\shabu\Desktop\MICE - Trace.txt";
        }

        private long ppuTotalCycles = 0;

        public string Name { get; } = "Nintendo Entertainment System";
        public long CurrentFrame { get; private set; } = 1;

        // Create components...
        public DataBus DataBus { get; } = new DataBus();
        public AddressBus AddressBus { get; } = new AddressBus();
        public ControlBus ControlBus { get; } = new ControlBus();

        public RicohRP2C02 PPU { get; private set; }
        public NESMemoryMap CPUMemoryMap { get; private set; }

        public NESCartridge Cartridge { get; private set; }
        public Ricoh2A03 CPU { get; private set; }

        public InputHandler InputHandler { get; } = new InputHandler();
        public byte[] Screen { get; private set; } = new byte[256 * 240];

        public bool IsPoweredOn => (this.CPU?.IsPowered ?? false) && (this.PPU?.IsPowered ?? false);

        public bool IsPaused { get; private set; } = false;

        // Hook them up...
        public void PowerOn()
        {
            if (File.Exists(Constants.DebugFile))
            {
                File.Delete(Constants.DebugFile);
            }

            if (this.Cartridge == null)
            {
                throw new InvalidOperationException("Cartridge must be loaded first, unable to power on.");
            }

            var ppuRegisters = new PPURegisters();
            this.CPUMemoryMap = new NESMemoryMap(ppuRegisters, this.InputHandler);
            this.PPU = new RicohRP2C02(new PPUMemoryMap(), ppuRegisters, this.CPUMemoryMap);

            this.PPU.Registers.OAMDMA.AfterWriteAction = this.DMATransfer;

            this.MapToCartridge();

            this.CPU = new Ricoh2A03(this.CPUMemoryMap, NES.traceFileOutput);

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

        public static StringBuilder traceFileOutput = new StringBuilder();
        public void Step()
        {
            if (CPU.CurrentCycle == 957167)
            {

            }
            var registerState = this.GetRegisterState();

            // 1 Step = 1 Frame to the NES, since we're doing frame-based timing here:
            // 1 System step = 1 CPU step + (3 PPU steps * CPU Cycles in Step) + (2 Audio steps * 1 CPU cycle).
            // Cycles are based on which instructions the CPU ran.
            var cycleEvent = new NintendoStepArgs
            {
                CPUStepsOccurred = this.CPU.Step()
            };

            NES.traceFileOutput.AppendLine(this.GetState(registerState));

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
                File.AppendAllText(Constants.DebugFile, NES.traceFileOutput.ToString());
                NES.traceFileOutput.Clear();
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
            var lastCycle = this.CPU.CurrentCycle;

            this.CPU.CurrentOpcode.AddedCycles +=
                ((this.CPU.CurrentCycle + this.CPU.CurrentOpcode.Cycles) % 2) == 1
                ? 515
                : 514;

            this.CPUMemoryMap.BulkTransfer(readAddress, this.PPU.PrimaryOAM.Data, this.PPU.Registers.OAMADDR, 256);

            NES.traceFileOutput.Append($" - [Sprite DMA Start - Cycle: {lastCycle}] - [Sprite DMA End - Cycle: {this.CPU.CurrentCycle + this.CPU.CurrentOpcode.AddedCycles}]");
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

        public void InputChanged(object inputs) => this.InputHandler.InputChanged((NESInputs)inputs);

        // TODO: This is all temporary for now, I'm just bug hunting. Format below matches Masen's trace logs.
        private string GetState(string registerState)
        {
            var label = this.GetLabelForAddress();
            var expectedSpace = 29;
            var stackIndentation = 0xFF - CPU.Registers.SP.Read();
            if (CPU.CurrentOpcode?.Name == "TXS" || CPU.CurrentOpcode?.Name == "RTS") { stackIndentation += 2; }
            else if (CPU.CurrentOpcode?.Name == "JSR") { stackIndentation -= 2; }
            else if (CPU.CurrentOpcode?.Name == "PHA") { stackIndentation -= 1; }
            else if (CPU.CurrentOpcode?.Name == "PHA") { stackIndentation -= 1; }
            else if (CPU.CurrentOpcode?.Name == "PLA") { stackIndentation += 1; }
            else if (CPU.CurrentOpcode?.Name == "RTI") { stackIndentation += 3; }

            var output = new StringBuilder($"{CPU.LastPC:X4}");
            try
            {
                output.Append(' ', stackIndentation + 2);
                output.Append($"{CPU.CurrentOpcode?.Name ?? "SEI"} {label}");
                output.Append(' ', Math.Max(1, expectedSpace - label.Length));
                output.Append(registerState);
            }
            catch(Exception e)
            {
            }

            return output.ToString();
        }

        private string GetRegisterState()
        {
            var output = new StringBuilder($"A:{CPU.Registers.A.Read():X2} ");
            try
            {
                output.Append($"X:{CPU.Registers.X.Read():X2} ");
                output.Append($"Y:{CPU.Registers.Y.Read():X2} ");
                output.Append($"P:{CPU.Registers.P.Read():X2} ");
                output.Append($"SP:{CPU.Registers.SP.Read():X2} ");
                output.Append($"CYC:{this.PPU.Cycle,3} ");
                output.Append($"SL:{this.PPU.ScanLine,3} ");
                output.Append($"FC:{this.CurrentFrame} ");
                output.Append($"CPU Cycle:{this.CPU.CurrentCycle}");
            }
            catch(Exception e)
            {
            }

            return output.ToString();
        }

        private string GetLabelForAddress()
        {
            if (CPU.LastAccessedAddress.Contains("$4017"))
            {
                CPU.LastAccessedAddress = CPU.LastAccessedAddress.Replace("$4017", "Ctrl2_FrameCtr_4017");
            }

            switch (CPU.LastAccessedAddress)
            {
                case "$2000":
                    return "PpuControl_2000";
                case "$2001":
                    return "PpuMask_2001";
                case "$2002":
                    return "PpuStatus_2002";
                case "$2003":
                    return "OamAddr_2003";
                case "$2005":
                    return "PpuScroll_2005";
                case "$2006":
                    return "PpuAddr_2006";
                case "$2007":
                    return "PpuData_2007";
                case "$4011":
                    return "DmcCounter_4011";
                case "$4014":
                    return "SpriteDma_4014";
                case "$4015":
                    return "ApuStatus_4015";
                case var addr when addr.Contains("$4016"):
                    return addr.Replace("$4016", "Ctrl1_4016");
                case var addr when addr.Contains("$4017"):
                    return addr.Replace("$4017", "Ctrl2_FrameCtr_4017");
                default:
                    return string.IsNullOrWhiteSpace(CPU.LastAccessedAddress)
                        ? " "
                        : CPU.LastAccessedAddress;
            }
        }

        public EventHandler<NintendoStepArgs> StepCompleted { get; set; }
        public EventHandler<FrameCompleteArgs> FrameFinished { get; set; }
        public EventHandler<CartridgeLoadedArgs> CartridgeLoaded { get; set; }
    }
}
