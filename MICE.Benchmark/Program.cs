using MICE.Nintendo;
using Ninject;
using System;
using System.Diagnostics;

namespace MICE.Benchmark
{
    public class Program
    {
        private NESContext nesContext;
        private IKernel kernel;
        private NES nes;

        public static void Main(string[] args)
        {
            var program = new Program();
            program.Go();

            program.TimeFrames();
            Console.ReadKey();
        }

        public void TimeFrames(int totalFrames = 60)
        {
            var then = DateTime.Now;
            var thenFrameCount = this.nes.PPU.FrameNumber;
            var stopAtFrame = this.nes.PPU.FrameNumber + totalFrames;

            while (this.nes.PPU.FrameNumber < stopAtFrame)
            {
                this.StepNESFrame();
            }

            var now = DateTime.Now;
            Console.WriteLine($"{totalFrames} frames took a total of {(now - then).TotalSeconds} seconds ({(now - then).TotalMilliseconds / totalFrames} avg ms/frame)");
            Console.WriteLine($"{1000.0 / 60.0} ms/frame needed");
        }

        public void Go()
        {
            this.kernel = new StandardKernel(new NintendoModule());

            this.nesContext = this.kernel.Get<NESContext>();
            var cartridge = NESLoader.CreateCartridge(@"C:\Emulators\NES\Games\World\Donkey Kong (JU).nes");

            this.nes = this.kernel.Get<NES>();
            this.nes.LoadCartridge(cartridge);

            this.nes.PowerOn();
        }

        long maxCycles = 1000000;
        private Stopwatch sw = new Stopwatch();
        private void StepNESFrame()
        {
            sw.Restart();
            this.nes.StepFrame();
            Console.WriteLine($"Frame {this.nes.PPU.FrameNumber} took: {sw.Elapsed.TotalMilliseconds} milliseconds");
        }

        private void StepNES()
        {
            this.nes.StepFrame();

            var maxCyclesAllotted = this.nes.CPU.CurrentCycle + maxCycles;

            while (this.nes.CPU.CurrentCycle < maxCyclesAllotted)
            {
                this.nes.Step();
            }
        }
    }
}
