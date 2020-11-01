using BenchmarkDotNet.Running;
using System;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using System.IO;

namespace MICE.Benchmark
{

    class EntryPoint
    {
        static void Main(string[] args)
        {
            new BenchmarkSwitcher(AllBenchmarks)
                .Run(args, new BenchmarkConfig());
            //var summary = BenchmarkRunner.Run<MICEBenchmarks>(new BenchmarkConfig());
        }

        private static readonly Type[] AllBenchmarks = new[]
        {
            typeof(RegisterWriteBenchmarks),
            typeof(RegisterReadBenchmarks),
            typeof(OpCodeBenchmarks),
            typeof(MICEBenchmarks),
        };
    }

    public class BenchmarkConfig : ManualConfig
    {
        public BenchmarkConfig()
        {
            Add(DefaultConfig.Instance);
            AddDiagnoser(MemoryDiagnoser.Default);

            ArtifactsPath = Path.Combine(AppContext.BaseDirectory, "artifacts", DateTime.Now.ToString("yyyy-mm-dd_hh-MM-ss"));
        }
    }
}