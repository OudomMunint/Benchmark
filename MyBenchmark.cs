using System.Security.Cryptography;
using System.Timers;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace Benchmark
{
    public class HashingBenchmark
    {
        private const int N = 1000000000;
        private readonly byte[] data;

        private readonly SHA256 sha256 = SHA256.Create();
        private readonly SHA512 sha512 = SHA512.Create();
        private readonly MD5 md5 = MD5.Create();

        public HashingBenchmark()
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

    public class EncryptionBenchmark
    {
        private const int N = 1000000;
        private readonly byte[] plainData;
        private readonly byte[] key;

        private readonly Aes aes = Aes.Create();

        public EncryptionBenchmark()
        {
            plainData = new byte[N];
            key = new byte[aes.KeySize / 8];

            new Random(42).NextBytes(plainData);
            new Random(42).NextBytes(key);
        }

        [Benchmark]
        public byte[] AesEncrypt()
        {
            using var encryptor = aes.CreateEncryptor(key, aes.IV);
            return encryptor.TransformFinalBlock(plainData, 0, plainData.Length);
        }

        [Benchmark]
        public byte[] AesDecrypt()
        {
            using var decryptor = aes.CreateDecryptor(key, aes.IV);
            return decryptor.TransformFinalBlock(AesEncrypt(), 0, N);
        }
    }

    public class MultithreadingBenchmark
    {
        private const int NumThreads = 8;
        private const int NumIterations = 1000000;
        private readonly int[] array;

        private DateTime startTime = DateTime.Now;
        private DateTime endTime = DateTime.Now;
        
        public MultithreadingBenchmark()
        {
            array = new int[NumIterations];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = i;
            }
            SingleThread();
            MultiThread();
        }

        [Benchmark]
        public void SingleThread()
        {
            //Console.WriteLine($"Benchmark started at: {startTime}");
            for (int i = 0; i < array.Length; i++)
            {
                DoWork(i);
            }
            //Console.WriteLine($"Benchmark finished at: {endTime}");
            //var TimeTaken = endTime - startTime;
            //Console.WriteLine($"Total elapsed time: {TimeTaken}");
        }

        [Benchmark]
        public void MultiThread()
        {
            //Console.WriteLine($"Benchmark started at: {startTime}");
            var options = new ParallelOptions { MaxDegreeOfParallelism = NumThreads };
            Parallel.For(0, array.Length, options, i =>
            {
                DoWork(i);
            });
            //Console.WriteLine($"Benchmark finished at: {endTime}");
            //var TimeTaken = endTime - startTime;
            //Console.WriteLine($"Total elapsed time: {TimeTaken}");
        }

        private static void DoWork(int index)
        {
            // simulate some work
            var result = Math.Pow(index, 2);
            result = Math.Pow(result, 2);
            _ = Math.Pow(result, 2);
        }
    }

    public class Program
    {
        // public static void Main(string[] args)
        // {
        //     _ = BenchmarkRunner.Run<MultithreadingBenchmark>();
        // }
    }
}