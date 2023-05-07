using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Benchmark
{
    [Config(typeof(SingleThreadPerCoreConfig))]
    public class MyBenchmark
    {
        private const int N = 10000;
        private readonly byte[] data;

        private readonly SHA256 sha256 = SHA256.Create();
        private readonly SHA512 sha512 = SHA512.Create();
        private readonly MD5 md5 = MD5.Create();

        public MyBenchmark()
        {
            data = new byte[N];
            new Random(42).NextBytes(data);
        }

        [Benchmark]
        public byte[] Sha256() => sha256.ComputeHash(data);

        [Benchmark]
        public byte[] Sha512() => sha512.ComputeHash(data);

        [Benchmark]
        public byte[] Md5() => md5.ComputeHash(data);
    }

    public class SingleThreadPerCoreConfig : ManualConfig
    {
        public SingleThreadPerCoreConfig()
        {
            var job = Job.Default.WithRuntime(CoreRuntime.Core70).WithId("SingleThreadPerCore")
                .WithJit(Jit.RyuJit).WithGcServer(false).WithGcConcurrent(false)
                .WithEnvironmentVariable("COMPlus_EnablePreferredAffinity", "1")
                .WithEnvironmentVariable("COMPlus_ThreadPool_ForceMinWorkerThreads", "20");
            Add(job);
        }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<MyBenchmark>();
        }
    }
}