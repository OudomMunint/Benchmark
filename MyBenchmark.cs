using System.Security.Cryptography;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace Benchmark
{
    //[Config(typeof(SingleThreadPerCoreConfig))]
    //public class MyBenchmark
    //{
    //    private const int N = 1000000000;
    //    private readonly byte[] data;

    //    private readonly SHA256 sha256 = SHA256.Create();
    //    private readonly SHA512 sha512 = SHA512.Create();
    //    private readonly MD5 md5 = MD5.Create();

    //    public MyBenchmark()
    //    {
    //        data = new byte[N];
    //        new Random(42).NextBytes(data);
    //    }

    //    [Benchmark]
    //    public byte[] Sha256() => sha256.ComputeHash(data);

    //    [Benchmark]
    //    public byte[] Sha512() => sha512.ComputeHash(data);

    //    [Benchmark]
    //    public byte[] Md5() => md5.ComputeHash(data);
    //}

    //public class EncryptionBenchmark
    //{
    //    private const int N = 1000000;
    //    private readonly byte[] plainData;
    //    private readonly byte[] key;

    //    private readonly Aes aes = Aes.Create();

    //    public EncryptionBenchmark()
    //    {
    //        plainData = new byte[N];
    //        key = new byte[aes.KeySize / 8];

    //        new Random(42).NextBytes(plainData);
    //        new Random(42).NextBytes(key);
    //    }

    //    [Benchmark]
    //    public byte[] AesEncrypt()
    //    {
    //        using (var encryptor = aes.CreateEncryptor(key, aes.IV))
    //        {
    //            return encryptor.TransformFinalBlock(plainData, 0, plainData.Length);
    //        }
    //    }

    //    [Benchmark]
    //    public byte[] AesDecrypt()
    //    {
    //        using (var decryptor = aes.CreateDecryptor(key, aes.IV))
    //        {
    //            return decryptor.TransformFinalBlock(AesEncrypt(), 0, N);
    //        }
    //    }
    //}

    //public class SingleThreadPerCoreConfig : ManualConfig
    //{
    //    [Obsolete]
    //    public SingleThreadPerCoreConfig()
    //    {
    //        //var job = Job.Default.WithRuntime(CoreRuntime.Core70).WithId("SingleThreadPerCore")
    //        //    .WithJit(Jit.RyuJit).WithGcServer(false).WithGcConcurrent(false)
    //        //    .WithEnvironmentVariable("COMPlus_EnablePreferredAffinity", "1")
    //        //    .WithEnvironmentVariable("COMPlus_ThreadPool_ForceMinWorkerThreads", "20");
    //        //Add(job);
    //    }
    //}

    public class MultithreadingBenchmark
    {
        private const int NumThreads = 8;
        private const int NumIterations = 1000000;
        private readonly int[] array;

        public MultithreadingBenchmark()
        {
            array = new int[NumIterations];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = i;
            }
        }

        [Benchmark]
        public void SingleThread()
        {
            for (int i = 0; i < array.Length; i++)
            {
                DoWork(i);
            }
        }

        [Benchmark]
        public void MultiThread()
        {
            var options = new ParallelOptions { MaxDegreeOfParallelism = NumThreads };
            Parallel.For(0, array.Length, options, i =>
            {
                DoWork(i);
            });
        }

        private void DoWork(int index)
        {
            // simulate some work
            var result = Math.Pow(index, 2);
            result = Math.Pow(result, 2);
            result = Math.Pow(result, 2);
        }
    }


    public class Program
    {
        public static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<MultithreadingBenchmark>();
        }
    }
}