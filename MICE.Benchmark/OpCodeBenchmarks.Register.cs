using BenchmarkDotNet.Attributes;

namespace MICE.Benchmark
{
    public partial class OpCodeBenchmarks
    {
        [Benchmark] [BenchmarkCategory("register")] public void DEX() => this.opcodes.DEX(null, 0x1234);
        [Benchmark] [BenchmarkCategory("register")] public void DEY() => this.opcodes.DEY(null, 0x1234);
        [Benchmark] [BenchmarkCategory("register")] public void INY() => this.opcodes.INY(null, 0x1234);
        [Benchmark] [BenchmarkCategory("register")] public void INX() => this.opcodes.INX(null, 0x1234);
        [Benchmark] [BenchmarkCategory("register")] public void TXA() => this.opcodes.TXA(null, 0x1234);
        [Benchmark] [BenchmarkCategory("register")] public void TAX() => this.opcodes.TAX(null, 0x1234);
        [Benchmark] [BenchmarkCategory("register")] public void TAY() => this.opcodes.TAY(null, 0x1234);
        [Benchmark] [BenchmarkCategory("register")] public void TYA() => this.opcodes.TYA(null, 0x1234);
    }
}
