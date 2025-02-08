#if DEBUG
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
#endif
using System.Diagnostics;
using System.Runtime.InteropServices;

class Program
{
    public static string ?TotalRunTime;

    static void Main(string[] args)
    {
        ConsoleInfo.GetAppInfo();

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            MacOSHelper.DisplayMacInfo();
        }

        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            DisplayWindowsInfo();
        }

        Console.ForegroundColor = ConsoleColor.White;
        Console.Write("Continue to benchmark? (y/n): ");
        var input = Console.ReadLine();

        if (string.Equals(input, "y", StringComparison.OrdinalIgnoreCase))
        {
            RunBenchmark();
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            // Prevent app exit on macOS
            Console.WriteLine("Press Enter to exit to exit...");
            Console.ReadLine();
        }
    }

    static void DisplayWindowsInfo()
    {
        ConsoleSpinner.Start();
        WindowsHelper.DisplayCpuInfo();
        WindowsHelper.DisplayRamInfo();
        DxGpuHelper.DisplayGpuInfo();
        ConsoleSpinner.Stop();
    }

    static void RunBenchmark()
    {
        Console.WriteLine("Choose a benchmark to run:");
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("1. Hashing Benchmark");
        Console.WriteLine("2. Encryption Benchmark");
        Console.WriteLine("3. CPU Prime Computation");
        Console.WriteLine("4. CPU Matrix Multiplication");
        Console.WriteLine("5. Memory Bandwidth");
        Console.WriteLine("6. Run all benchmarks");
#if DEBUG
        Console.WriteLine("7. Debug Mode");
        Console.WriteLine("8. Test Reults Export");
#endif
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write("Enter the number of your choice: ");

        List<string> results = new();
        string? choice = Console.ReadLine();
        var EncrypBenchmark = new EncryptionBenchmark();
        var HashBenchmark = new HashingBenchmark();
        var MMUL = new MatrixMultiplicationBenchmark();
        var MemoryBenchmark = new MemoryBenchmark();
        var benchmarkActions = new Dictionary<string, Action>
        {
            ["1"] = () => results.Add(HashBenchmark.CombinedHashingExport()),
            ["2"] = () => results.Add(EncrypBenchmark.RunEncryptBenchmark()),
            ["3"] = () => results.Add(CPUBenchmark.CpuPrimeCompute()),
            ["4"] = () => results.Add(MMUL.MultiplyMatrix()),
            ["5"] = () => results.Add(MemoryBenchmark.MTMemBandwidth()),
            ["6"] = () =>
            {
                results.AddRange(HashBenchmark.CombinedHashingExport(), EncrypBenchmark.RunEncryptBenchmark(),
                    CPUBenchmark.CpuPrimeCompute(), MMUL.MultiplyMatrix(), MemoryBenchmark.MTMemBandwidth());
            },
#if DEBUG
            ["7"] = () => BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(new[] { "Benchmarks" }, new DebugInProcessConfig()),
            ["8"] = () => BenchmarkExporter.TestExportResults()
#endif
        };

        if (choice != null && benchmarkActions.TryGetValue(choice, out Action? benchmarkAction))
        {
            Console.WriteLine("-----------------------------------------------------------");

            Stopwatch stopwatch = Stopwatch.StartNew();
            benchmarkAction?.Invoke();
            stopwatch.Stop();

            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine($"Total Execution Time: {stopwatch.ElapsedMilliseconds} ms.");

            GcHelper.MemoryCleanUp();
            Program.TotalRunTime = stopwatch.ElapsedMilliseconds.ToString();

            Console.ForegroundColor = ConsoleColor.Cyan;
            BenchmarkExporter.ExportResults("BenchmarkResults.txt", results.ToArray());

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Press Enter to exit...");
            Console.ReadLine();
        }
        else
        {
            Console.WriteLine("Invalid choice.");
        }
    }
}