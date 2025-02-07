using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Threading;
using System.Timers;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using NvAPIWrapper;
using NvAPIWrapper.Display;
using NvAPIWrapper.GPU;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Device = SharpDX.Direct3D11.Device;

namespace Benchmark
{
    [Obsolete]
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

        public byte[] Sha256() => sha256.ComputeHash(data);

        public byte[] Sha512() => sha512.ComputeHash(data);

        public byte[] Md5() => md5.ComputeHash(data);

        [Benchmark]
        public void CombinedHashing()
        {
            Sha256();
            Sha512();
            Md5();
        }
    }

    [Obsolete]
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

        public byte[] AesEncrypt()
        {
            using var encryptor = aes.CreateEncryptor(key, aes.IV);
            return encryptor.TransformFinalBlock(plainData, 0, plainData.Length);
        }

        public byte[] AesDecrypt()
        {
            using var decryptor = aes.CreateDecryptor(key, aes.IV);
            return decryptor.TransformFinalBlock(AesEncrypt(), 0, N);
        }

        [Benchmark]
        public void CombinedEncryption()
        {
            AesEncrypt();
            AesDecrypt();
        }
    }

    [Obsolete]
    public class CPUBenchmark
    {
        private const int NumIterations = 1000;

        [Benchmark]
        public void FullCpuLoad()
        {
            var options = new ParallelOptions
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount,
            };

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Process.GetCurrentProcess().ProcessorAffinity = (IntPtr)0xFF;
            }

            Parallel.For(0, NumIterations, options, i =>
            {
                DoWork(i);
            });
        }

        private void DoWork(int index)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Thread.CurrentThread.Priority = ThreadPriority.AboveNormal;
            }

            double result = 1;
            for (int i = 1; i <= 100; i++)
            {
                result = Math.Sin(index * result) * Math.Tan(index * result);
            }
        }
    }

    [Obsolete]
    public class GpuBenchmark
    {
        private const int NumIterations = 1000;
        private Device? device;

        [GlobalSetup]
        public void Setup()
        {
            device = new Device(SharpDX.Direct3D.DriverType.Hardware, DeviceCreationFlags.None);
        }

        [GlobalCleanup]
        public void Cleanup()
        {
            device?.Dispose();
        }

        public void FullGpuLoad()
        {
            for (int i = 0; i < NumIterations; i++)
            {
                using var texture = new SharpDX.Direct3D11.Texture2D(device!, new SharpDX.Direct3D11.Texture2DDescription
                {
                    Width = 1920,
                    Height = 1080,
                    ArraySize = 1,
                    BindFlags = SharpDX.Direct3D11.BindFlags.RenderTarget,
                    Usage = SharpDX.Direct3D11.ResourceUsage.Default,
                    CpuAccessFlags = SharpDX.Direct3D11.CpuAccessFlags.None,
                    Format = Format.R8G8B8A8_UNorm,
                    MipLevels = 1,
                    OptionFlags = SharpDX.Direct3D11.ResourceOptionFlags.None,
                    SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0),
                });
            }
        }
    }
}