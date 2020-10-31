using BenchmarkDotNet.Attributes;

namespace MICE.Benchmark
{
    public partial class OpCodeBenchmarks
    {
        [Benchmark] [BenchmarkCategory("branch")] public void BPL() => this.opcodes.BPL(null, 0x1234);
        [Benchmark] [BenchmarkCategory("branch")] public void BMI() => this.opcodes.BMI(null, 0x1234);
        [Benchmark] [BenchmarkCategory("branch")] public void BVC() => this.opcodes.BVC(null, 0x1234);
        [Benchmark] [BenchmarkCategory("branch")] public void BVS() => this.opcodes.BVS(null, 0x1234);
        [Benchmark] [BenchmarkCategory("branch")] public void BCC() => this.opcodes.BCC(null, 0x1234);
        [Benchmark] [BenchmarkCategory("branch")] public void BCS() => this.opcodes.BCS(null, 0x1234);
        [Benchmark] [BenchmarkCategory("branch")] public void BNE() => this.opcodes.BNE(null, 0x1234);
    }
}
