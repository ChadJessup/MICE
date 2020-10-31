using BenchmarkDotNet.Attributes;

namespace MICE.Benchmark
{
    public partial class OpCodeBenchmarks
    {
        [Benchmark] [BenchmarkCategory("load")] public void LDX() => this.opcodes.LDX(null, 0x1234);
        [Benchmark] [BenchmarkCategory("load")] public void LDY() => this.opcodes.LDY(null, 0x1234);
        [Benchmark] [BenchmarkCategory("load")] public void LDA() => this.opcodes.LDA(null, 0x1234);
    }
}
