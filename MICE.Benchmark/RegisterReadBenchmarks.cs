using BenchmarkDotNet.Attributes;
using MICE.Components.CPU;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MICE.Benchmark
{
    public class RegisterReadBenchmarks
    {
        private Register8BitOld register8BitOld;
        private Register8Bit register8Bit;
        private ShiftRegister16Bit shiftRegister16Bit;

        [GlobalSetup]
        public void Setup()
        {
            this.register8BitOld = new Register8BitOld("benchmark old");
            this.register8Bit = new Register8Bit("benchmark unrolled");
            this.shiftRegister16Bit = new ShiftRegister16Bit("benchmark shift");
        }

        [Benchmark]
        public void ShiftRegister16BitRead() => this.shiftRegister16Bit.Read();

        [Benchmark]
        public void Register8BitOldRead() => this.register8BitOld.Read();

        [Benchmark(Baseline = true)]
        public void Register8BitRead() => this.register8Bit.Read();
    }
}
