using BenchmarkDotNet.Attributes;
using MICE.Components.CPU;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MICE.Benchmark
{
    public class RegisterWriteBenchmarks
    {
        private Register8BitOld register8BitOld;
        private Register8Bit register8Bit;
        private ShiftRegister16Bit shiftRegister16Bit;

        [GlobalSetup]
        public void Setup()
        {
            this.register8BitOld = new Register8BitOld("benchmark old");
            this.register8Bit = new Register8Bit("benchmark unrolled");
            this.shiftRegister16Bit = new ShiftRegister16Bit("benchmark shift 16bit");
        }

        [Benchmark(Baseline = true)] public void Register8BitWriteZero() => this.register8Bit.Write(0);
        [Benchmark] public void Register8BitOldWriteZeroes() => this.register8BitOld.Write(0);
        [Benchmark] public void Register8BitWriteMax() => this.register8Bit.Write(byte.MaxValue);
        [Benchmark] public void Register8BitOldWriteMax() => this.register8BitOld.Write(byte.MaxValue);
        [Benchmark] public void ShiftRegister16BitWriteMax() => this.shiftRegister16Bit.Write(byte.MaxValue);
        [Benchmark] public void ShiftRegister16BitWriteZero() => this.shiftRegister16Bit.Write(0);
    }
}
