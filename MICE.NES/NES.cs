using MICE.Common.Interfaces;
using MICE.Components.Memory;
using MICE.CPU.MOS6502;
using MICE.Nintendo.Components;
using MICE.Nintendo.Handlers;
using MICE.Nintendo.Loaders;
using MICE.PPU.RicohRP2C02;
using MICE.PPU.RicohRP2C02.Components;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace MICE.Nintendo
{
    public class NES : ISystem
    {
        private readonly NESContext context;
        private readonly Dictionary<string, string> addressLabels = new Dictionary<string, string>()
        {
            { "$2000", "PpuControl_2000" },
            { "$2001", "PpuMask_2001" },
            { "$2002", "PpuStatus_2002" },
            { "$2003", "OamAddr_2003" },
            { "$2004", "OamData_2004" },
            { "$2005", "PpuScroll_2005" },
            { "$2006", "PpuAddr_2006" },
            { "$2007", "PpuData_2007" },
            { "$4000", "Sq1Duty_4000" },
            { "$4001", "Sq1Sweep_4001" },
            { "$4002", "Sq1Timer_4002" },
            { "$4003", "Sq1Length_4003" },
            { "$4004", "Sq1Duty_4004" },
            { "$4005", "Sq1Sweep_4005" },
            { "$4006", "Sq1Timer_4006" },
            { "$4007", "Sq1Length_4007" },
            { "$4008", "TrgLinear_4008" },
            { "$400A", "TrgTimer_400A" },
            { "$400B", "TrgLength_400B" },
            { "$400C", "NoiseVolume_400C" },
            { "$400E", "NoisePeriod_400E" },
            { "$400F", "NoiseLength_400F" },
            { "$4010", "DmcFreq_4010" },
            { "$4011", "DmcCounter_4011" },
            { "$4012", "DmcAddress_4012" },
            { "$4013", "DmcLength_4013" },
            { "$4014", "SpriteDma_4014" },
            { "$4015", "ApuStatus_4015" },
            { "$4016", "Ctrl1_4016" },
            { "$4017", "Ctrl2_FrameCtr_4017" },
        };

        public NES(NESContext context)
        {
            NES.IsDebug = false;

            this.context = context;

            this.PPU = this.context.PPU;

            this.CPU = (Ricoh2A03)this.context.CPU;
            this.CPUMemoryMap = this.CPU.MemoryMap;

            this.InputHandler = context.InputHandler;
        }

        public string Name { get; } = "Nintendo Entertainment System";
        public long CurrentFrame { get; private set; } = 1;

        private static bool isDebug = false;
        public static bool IsDebug
        {
            get => NES.isDebug;
            set
            {
                NES.isDebug = value;
                MOS6502.IsDebug = value;
            }
        }

        public RicohRP2C02 PPU { get; private set; }
        public IMemoryMap CPUMemoryMap { get; private set; }

        public NESCartridge Cartridge { get; private set; }
        public Ricoh2A03 CPU { get; private set; }

        public InputHandler InputHandler { get; }
        public byte[] Screen { get; private set; } = new byte[256 * 240];

        public bool IsPoweredOn => (this.CPU?.IsPowered ?? false) && (this.PPU?.IsPowered ?? false);

        public bool IsPaused { get; private set; } = false;

        private Action cycleCompleteAction;

        public void PowerOn()
        {
            if (this.Cartridge == null)
            {
                throw new InvalidOperationException("Cartridge must be loaded first, unable to power on.");
            }

            this.PPU.Registers.OAMDMA.AfterWriteAction = this.DMATransfer;

            this.MapToCartridge();

            this.cycleCompleteAction = this.DoPerCPUCycle;

            this.CPU.PowerOn(this.cycleCompleteAction);
            this.PPU.PowerOn(this.cycleCompleteAction);

            this.IsPaused = false;
        }

        private void DoPerCPUCycle()
        {
            this.StepPPU();

            if (!this.isInDMA)
            {
                this.HandleInterrupts();
            }
        }

        private void HandleInterrupts()
        {
            this.CPU.PreviousRunIrq = this.CPU.CurrentRunIrq;
            this.CPU.CurrentRunIrq = this.PPU.ShouldNMInterrupt;
        }

        private void StepPPU()
        {
            this.PPU.Step();
            this.PPU.Step();
            this.PPU.Step();
        }

        private void MapToCartridge()
        {
            this.CPUMemoryMap.GetMemorySegment<IExternal>("External").AttachHandler(this.Cartridge.Mapper);

            // Various parts of a cartridge are mapped into the NES's CPU memory map.
            //this.CPUMemoryMap.GetMemorySegment<IExternal>("PRG-ROM Lower Bank").AttachHandler(this.Cartridge.Mapper);
            //this.CPUMemoryMap.GetMemorySegment<IExternal>("PRG-ROM Upper Bank").AttachHandler(this.Cartridge.Mapper);

            // Various parts of a cartridge are mapped into the NES's PPU memory map.
            this.PPU.MemoryMap.GetMemorySegment<PatternTable>("Pattern Table 0").AttachHandler(this.Cartridge.Mapper);
            this.PPU.MemoryMap.GetMemorySegment<PatternTable>("Pattern Table 1").AttachHandler(this.Cartridge.Mapper);

            this.PPU.MemoryMap.GetMemorySegment<Nametable>("Name Table 0").AttachHandler(this.Cartridge.Mapper);
            this.PPU.MemoryMap.GetMemorySegment<Nametable>("Name Table 1").AttachHandler(this.Cartridge.Mapper);
            this.PPU.MemoryMap.GetMemorySegment<Nametable>("Name Table 2").AttachHandler(this.Cartridge.Mapper);
            this.PPU.MemoryMap.GetMemorySegment<Nametable>("Name Table 3").AttachHandler(this.Cartridge.Mapper);
        }

        public void StepFrame()
        {
            var currentFrame = this.PPU.FrameNumber;
            while (currentFrame == this.PPU.FrameNumber)
            {
                this.Step();
            }
        }

        private Stopwatch frameSW = new Stopwatch();
        public void Step()
        {
            string registerState = "";

            if (NES.IsDebug)
            {
                registerState = this.GetRegisterState();
            }

            // 1 Step = 1 Frame to the NES, since we're doing frame-based timing here:
            // 1 System step = 1 CPU step + (3 PPU steps * CPU Cycles in Step) + (2 Audio steps * 1 CPU cycle).
            // Cycles are based on which instructions the CPU ran.
            this.CPU.FetchInstruction();
            this.CPU.DecodeInstruction();
            this.CPU.ExecuteInstruction();

            if (NES.IsDebug)
            {
                Log.Verbose(this.GetState(registerState));
            }

            if (this.CPU.HandleIfIRQ())
            {
                this.PPU.ShouldNMInterrupt = false;
            }

            if (this.PPU.FrameNumber > this.CurrentFrame)
            {
                // TODO: Have two loggers, as trace log shouldn't have these if we need it to match with Mesen's.
                //Log.Information("Frame {currentFrame} took (ms): {elapsed}", this.PPU.FrameNumber, this.frameSW.ElapsedMilliseconds);
                this.frameSW.Restart();

                this.frameStartClock = this.CPU.CurrentCycle;
                this.CurrentFrame = this.PPU.FrameNumber;
                Array.Copy(this.PPU.ScreenData, this.Screen, this.PPU.ScreenData.Length);
            }

            // TODO: APU Cycles
        }

        private long frameStartClock = 0;

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

        private bool isInDMA = false;
        public void DMATransfer(int? address, byte value)
        {
            this.isInDMA = true;
            ushort readAddress = (ushort)(value << 8);
            var lastCycle = CPU.CurrentCycle;

            if (CPU.CurrentCycle % 2 != 0)
            {
                CPU.CycleFinished();
            }

            CPU.CycleFinished();

            for (int i = 0; i < 256; i++)
            {
                // Read cycle
                var readByte = CPU.ReadByteAt((ushort)(readAddress + i));

                // Write cycle...
                CPU.CycleFinished();
                this.PPU.PrimaryOAM.Data[this.PPU.Registers.OAMADDR + i] = readByte;
            }

            //this.CPUMemoryMap.BulkTransfer(readAddress, this.PPU.PrimaryOAM.Data, this.PPU.Registers.OAMADDR, 256);

            //for (int i = 0; i < extraCycles; i++)
            //{
            //    CPU.CycleFinished();
            //}

            Log.Verbose(" - [Sprite DMA Start - Cycle: {startCycle}] - [Sprite DMA End - Cycle: {currentCycle}]", lastCycle, this.CPU.CurrentCycle + 1);
            this.isInDMA = false;
        }

        public Task Run()
        {
            return Task.Factory.StartNew(() =>
            {
                this.frameSW.Start();

                // Spin the PPU a magic number amount of times while the CPU spins up.
                for (int i = 0; i < 31; i++)
                {
                    this.PPU.Step();
                }

                while (!this.IsPaused && this.IsPoweredOn)
                {
                    this.Step();
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

            var opCodeName = CPU.CurrentOpcode?.Name;

            if (opCodeName == "TXS" || opCodeName == "RTS") { stackIndentation += 2; }
            else if (opCodeName == "JSR") { stackIndentation -= 2; }
            else if (opCodeName == "PHA") { stackIndentation -= 1; }
            else if (opCodeName == "PHA") { stackIndentation -= 1; }
            else if (opCodeName == "PLA") { stackIndentation += 1; }
            else if (opCodeName == "RTI") { stackIndentation += 3; }
            else if (opCodeName == "PLP") { stackIndentation += 1; }
            else if (CPU.CurrentOpcode?.IsUnofficial ?? false) { opCodeName += "*"; expectedSpace--; }

            var spaces = new String(' ', Math.Max(1, expectedSpace - label?.Length ?? 3));

            return $"{CPU.LastPC:X4}  {opCodeName ?? "SEI"} {label} {spaces} {registerState}";
        }

        private string GetRegisterState()
            => $"A:{CPU.Registers.A.Read():X2} X:{CPU.Registers.X.Read():X2} Y:{CPU.Registers.Y.Read():X2} P:{CPU.Registers.P.Read():X2} SP:{CPU.Registers.SP.Read():X2} CYC:{this.PPU.Cycle,3} SL:{this.PPU.ScanLine,3} FC:{this.CurrentFrame} CPU Cycle:{this.CPU.CurrentCycle + 1}";

        private string GetLabelForAddress()
        {
            if (CPU.LastAccessedAddress == null)
            {
                return CPU.LastAccessedAddress;
            }

            foreach (var kvp in this.addressLabels)
            {
                CPU.LastAccessedAddress = CPU.LastAccessedAddress.Replace(kvp.Key, kvp.Value);
            }

            return CPU.LastAccessedAddress;
        }

        public EventHandler<NintendoStepArgs> StepCompleted { get; set; }
        public EventHandler<FrameCompleteArgs> FrameFinished { get; set; }
        public EventHandler<CartridgeLoadedArgs> CartridgeLoaded { get; set; }
    }
}
