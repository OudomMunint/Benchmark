using BenchmarkDotNet.Attributes;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Security.Cryptography;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Device = SharpDX.Direct3D11.Device;

public class HashingBenchmark
{
    private const int N = 2000000000;
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

    public void CombinedHashing()
    {
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("Running Hashing operation...");
        Stopwatch stopwatch = Stopwatch.StartNew();
        ConsoleSpinner.Start();

        Sha256();
        Sha512();
        Md5();

        stopwatch.Stop();
        ConsoleSpinner.Stop();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"Hasing completed in {stopwatch.ElapsedMilliseconds} ms.");
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("-----------------------------------------------------------");
    }
}

public class EncryptionBenchmark
{
    private const long TotalSize = 16L * 1_000_000_000; // 16GB
    private const int ChunkSize = 100_000_000; // 100MB per operation
    private const int Iterations = (int)(TotalSize / ChunkSize); // Number of chunks needed
    private readonly byte[] dataChunk;
    private readonly byte[] key;
    private readonly byte[] iv;
    private readonly Aes aes;

    public EncryptionBenchmark()
    {
        aes = Aes.Create();
        aes.KeySize = 256;
        aes.GenerateKey();
        aes.GenerateIV();

        key = aes.Key;
        iv = aes.IV;

        dataChunk = new byte[ChunkSize];
        new Random().NextBytes(dataChunk); // Generate random data once
    }

    public byte[] AesEncrypt(byte[] data)
    {
        using var encryptor = aes.CreateEncryptor(key, iv);
        return encryptor.TransformFinalBlock(data, 0, data.Length);
    }

    public byte[] AesDecrypt(byte[] encryptedData)
    {
        using var decryptor = aes.CreateDecryptor(key, iv);
        return decryptor.TransformFinalBlock(encryptedData, 0, encryptedData.Length);
    }

    public void RunEncryptBenchmark()
    {
        int threadCount = Environment.ProcessorCount; // Match CPU core count
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine($"Running AES-256 Encryption... processing {TotalSize / 1_000_000_000} GB with {threadCount} threads...");

        Stopwatch stopwatch = Stopwatch.StartNew();
        ConsoleSpinner.Start();

        Parallel.For(0, threadCount, _ =>
        {
            for (int i = 0; i < Iterations / threadCount; i++)
            {
                byte[] encrypted = AesEncrypt(dataChunk);
                byte[] decrypted = AesDecrypt(encrypted);
            }
        });

        stopwatch.Stop();
        ConsoleSpinner.Stop();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"Encryption completed in {stopwatch.ElapsedMilliseconds} ms.");
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("-----------------------------------------------------------");
    }
}

class CPUBenchmark
{
    public static void CpuTest()
    {
        int taskCount = Environment.ProcessorCount; // Number of parallel tasks
        int iterations = 35_000_000; // Workload per task

        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine($"Computing Primes with {taskCount} threads...");

        var options = new ParallelOptions
        {
            MaxDegreeOfParallelism = taskCount
        };

        ConsoleSpinner.Start();
        Stopwatch stopwatch = Stopwatch.StartNew();

        Parallel.For(0, taskCount, options, _ =>
        {
            ComputePrimes(iterations);
        });

        stopwatch.Stop();
        ConsoleSpinner.Stop();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"Primes Computed in {stopwatch.ElapsedMilliseconds} ms.");
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("-----------------------------------------------------------");
    }

    private static int ComputePrimes(int limit)
    {
        int count = 0;
        for (int i = 2; i < limit; i++)
        {
            if (IsPrime(i))
                count++;
        }
        return count;
    }

    private static bool IsPrime(int number)
    {
        if (number < 2) return false;
        if (number % 2 == 0 && number != 2) return false;
        for (int i = 3; i * i <= number; i += 2)
        {
            if (number % i == 0)
                return false;
        }
        return true;
    }
}