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

    public class CPUBenchmark
    {
        private const int NumIterations = 1000000;

        [Benchmark]
        public void FullCpuLoad()
        {
            var options = new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount };
            Parallel.For(0, NumIterations, options, i =>
            {
                DoWork(i);
            });
        }

        private void DoWork(int index)
        {
            double result = 1;
            for (int i = 1; i <= 1000; i++)
            {
                result = Math.Sin(index * result) * Math.Tan(index * result);
            }
        }
    }

    public class Program
    {

    }
}