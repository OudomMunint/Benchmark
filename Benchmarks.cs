using BenchmarkDotNet.Attributes;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Security.Cryptography;
using System.Buffers;

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
    private const int Iterations = (int)(TotalSize / ChunkSize);
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
        new Random().NextBytes(dataChunk);
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
        int threadCount = Environment.ProcessorCount;
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
    public static void CpuPrimeCompute()
    {
        int taskCount = Environment.ProcessorCount;
        int iterations = 400_000_000;
        int iterationsPerThread = iterations / taskCount;

        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine($"Running Prime Compute with {taskCount} threads...");

        var options = new ParallelOptions
        {
            MaxDegreeOfParallelism = taskCount
        };

        ConsoleSpinner.Start();
        Stopwatch stopwatch = Stopwatch.StartNew();

        Parallel.For(0, taskCount, options, _ =>
        {
            ComputePrimes(iterationsPerThread);
        });

        stopwatch.Stop();
        ConsoleSpinner.Stop();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"Prime compute completed in {stopwatch.ElapsedMilliseconds} ms.");
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

class MatrixMultiplicationBenchmark
{
    private const int N = 2048; // Matrix size
    private readonly double[,] matrixA;
    private readonly double[,] matrixB;
    private readonly double[,] result;

    public MatrixMultiplicationBenchmark()
    {
        matrixA = new double[N, N];
        matrixB = new double[N, N];
        result = new double[N, N];

        Random random = new Random(42);
        for (int i = 0; i < N; i++)
        {
            for (int j = 0; j < N; j++)
            {
                matrixA[i, j] = random.NextDouble() * 100;
                matrixB[i, j] = random.NextDouble() * 100;
            }
        }
    }

    public void MultiplyMatrix()
    {
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine($"Running Matrix Multiplication with {Environment.ProcessorCount} threads...");
        ConsoleSpinner.Start();
        Stopwatch stopwatch = Stopwatch.StartNew();

        var options = new ParallelOptions
        {
            MaxDegreeOfParallelism = Environment.ProcessorCount
        };

        Parallel.For(0, N, options, i =>
        {
            for (int j = 0; j < N; j++)
            {
                double sum = 0;
                for (int k = 0; k < N; k++)
                {
                    sum += matrixA[i, k] * matrixB[k, j];
                }
                result[i, j] = sum;
            }
        });

        stopwatch.Stop();
        ConsoleSpinner.Stop();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"Matrix multiplication completed in {stopwatch.ElapsedMilliseconds} ms.");
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("-----------------------------------------------------------");
    }
}

public class MemoryBenchmark
{
    private const int DataSize = 1024 * 1024 * 1024; // 512MB (avoid hitting array limits)
    private const int ChunkSize = 16 * 1024 * 1024; // 16MB per operation

    public void RunMemoryBandwidthTest()
    {
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("Running Memory Bandwidth test...");

        byte[] memoryBuffer = ArrayPool<byte>.Shared.Rent(DataSize);

        Stopwatch stopwatch = Stopwatch.StartNew();
        ConsoleSpinner.Start();

        Parallel.For(0, DataSize / ChunkSize, i =>
        {
            int offset = i * ChunkSize;
            Array.Copy(memoryBuffer, offset, memoryBuffer, offset, ChunkSize);
        });

        stopwatch.Stop();
        ConsoleSpinner.Stop();

        double seconds = stopwatch.ElapsedMilliseconds / 1000.0;
        double bandwidthGBs = (DataSize / (1024.0 * 1024.0 * 1024.0)) / seconds;

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("Memory Bandwidth test completed....");
        Console.WriteLine($"Bandwidth: {bandwidthGBs:F2} GB/s");

        ArrayPool<byte>.Shared.Return(memoryBuffer); // Release memory

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("-----------------------------------------------------------");
    }
}