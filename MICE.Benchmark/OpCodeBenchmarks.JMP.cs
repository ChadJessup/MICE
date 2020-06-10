using BenchmarkDotNet.Attributes;

namespace MICE.Benchmark
{
    public partial class OpCodeBenchmarks
    {
        [Benchmark(Baseline = true)] [BenchmarkCategory("jmp")] public void JMP() => this.opcodes.JMP(null, 0x1234);
        [Benchmark] [BenchmarkCategory("jmp")] public void JSR() => this.opcodes.JSR(null, 0x1234);
        [Benchmark] [BenchmarkCategory("jmp")] public void RTS() => this.opcodes.RTS(null, 0x1234);
        [Benchmark] [BenchmarkCategory("jmp")] public void RTI() => this.opcodes.RTI(null, 0x1234);
        [Benchmark] [BenchmarkCategory("jmp")] public void BRK() => this.opcodes.BRK(null, 0x1234);
        [Benchmark] [BenchmarkCategory("jmp")] public void NOP() => this.opcodes.NOP(null, 0x1234);
    }
}
