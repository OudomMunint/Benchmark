using System.Diagnostics;
using System.Security.Cryptography;
using System.Buffers;

public class HashingBenchmark
{
    private int N;
    private readonly byte[] data;
    private readonly SHA256 sha256 = SHA256.Create();
    private readonly SHA512 sha512 = SHA512.Create();
    private readonly MD5 md5 = MD5.Create();

    public HashingBenchmark()
    {
        if (Debugger.IsAttached)
        {
            N = 1000000000;
        }
        else
        {
            N = 2000000000;
        }
        data = new byte[N];
        new Random(42).NextBytes(data);
    }

    public byte[] Sha256() => sha256.ComputeHash(data);

    public byte[] Sha512() => sha512.ComputeHash(data);

    public byte[] Md5() => md5.ComputeHash(data);

    public string CombinedHashingExport()
    {
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine($"Running Hash on SHA256, SHA512, MD5... Hashing {N / 1_000_000_000} GB...");
        Stopwatch stopwatch = Stopwatch.StartNew();
        ConsoleSpinner.Start();

        Sha256();
        Sha512();
        Md5();

        stopwatch.Stop();
        ConsoleSpinner.Stop();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"Hashing completed in {stopwatch.ElapsedMilliseconds} ms.");
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("-----------------------------------------------------------");

        string result = $"Hashing Benchmark: {stopwatch.ElapsedMilliseconds} ms";
        return result;
    }
}

public class EncryptionBenchmark
{
    private long TotalSize;
    private const int ChunkSize = 100_000_000; // 100MB per operation
    private int Iterations;
    private readonly byte[] dataChunk;
    private readonly byte[] key;
    private readonly byte[] iv;
    private readonly Aes aes;

    public EncryptionBenchmark()
    {
        if (Debugger.IsAttached)
        {
            TotalSize = 1L * 1_000_000_000; // 1GB
        }
        else
        {
            TotalSize = 16L * 1_000_000_000; // 16GB
        }
        Iterations = (int)(TotalSize / ChunkSize);
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

    public string RunEncryptBenchmark()
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

        string result = $"Encryption Benchmark: {stopwatch.ElapsedMilliseconds} ms";
        return result;
    }
}

class CPUBenchmark
{
    public static string CpuPrimeCompute()
    {
        int iterations;

        if (Debugger.IsAttached)
        {
            iterations = 100_000_000;
        }
        else
        {
            iterations = 400_000_000; // Default value if not debugging
        }

        int taskCount = Environment.ProcessorCount;
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

        string result = $"Prime Compute Benchmark: {stopwatch.ElapsedMilliseconds} ms";
        return result;
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
    private int N; // Matrix size
    private readonly double[,] matrixA;
    private readonly double[,] matrixB;
    private readonly double[,] result;

    public MatrixMultiplicationBenchmark()
    {
        if (Debugger.IsAttached)
        {
            N = 1024;
        }
        else
        {
            N = 2048;
        }
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

    public string MultiplyMatrix()
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

        string benchResult = $"Matrix Multiplication Benchmark: {stopwatch.ElapsedMilliseconds} ms";
        return benchResult;
    }
}

// WIP
public class MemoryBenchmark
{
    public void MTMemBandwidth()
    {
        uint[] data = new uint[10000000 * 32];
        List<(long Sum, double Bandwidth)> AllResults = new();
        (long Sum, double Bandwidth) BestResult;

        for (int j = 0; j < 15; j++)
        {
            long totalSum = 0;
            var sw = Stopwatch.StartNew();
            int chunkSize = data.Length / Environment.ProcessorCount;
            object lockObj = new object();

            Parallel.For(0, Environment.ProcessorCount, threadId =>
            {
                int start = threadId * chunkSize;
                int end = (threadId == Environment.ProcessorCount - 1) ? data.Length : start + chunkSize;
                uint localSum = 0;

                for (int i = start; i < end; i += 64)
                {
                    localSum += data[i] + data[i + 16] + data[i + 32] + data[i + 48];
                }

                lock (lockObj)
                {
                    totalSum += localSum;
                }
            });

            sw.Stop();
            long dataSize = data.Length * 4;
            double bandwidth = dataSize / sw.Elapsed.TotalSeconds / (1024 * 1024 * 1024);

            //Console.WriteLine("{1:0.000} GB/s", totalSum, bandwidth);
            AllResults.Add((totalSum, bandwidth));
        }

        BestResult = AllResults.OrderByDescending(x => x.Bandwidth).First(); // Sort for highest
        Console.WriteLine($"Memory Bandwidth: {BestResult.Sum} {BestResult.Bandwidth:0.000} GB/s");
    }

    public void STMemBandwidth()
    {
        List<(long Sum, double Bandwidth)> AllResults = new();
        (long Sum, double Bandwidth) BestResult;
        uint[] data = new uint[10000000 * 32];
        for (int j = 0; j < 15; j++)
        {
            long totalSum = 0;
            uint sum = 0;
            var sw = Stopwatch.StartNew();
            for (uint i = 0; i < data.Length; i += 64)
            {
                sum += data[i] + data[i + 16] + data[i + 32] + data[i + 48];
            }
            sw.Stop();
            long dataSize = data.Length * 4;
            double bandwidth = dataSize / sw.Elapsed.TotalSeconds / (1024 * 1024 * 1024);
            Console.WriteLine("{0} {1:0.000} GB/s", sum, dataSize / sw.Elapsed.TotalSeconds / (1024 * 1024 * 1024));
            AllResults.Add((totalSum, bandwidth));
        }

        BestResult = AllResults.OrderByDescending(x => x.Bandwidth).First(); // Sort for highest
        Console.WriteLine($"Memory Bandwidth: {BestResult.Sum} {BestResult.Bandwidth:0.000} GB/s");
    }
}