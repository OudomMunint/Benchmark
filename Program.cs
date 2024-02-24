using Benchmark;
using BenchmarkDotNet.Running;
using System.Management;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Direct3D;
using System;
using Device = SharpDX.Direct3D11.Device;

Console.WriteLine("Welcome to the best benchmark in the entire universe");
Console.WriteLine("-----------------------------------------------------------");

if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
{

    var startInfo = new ProcessStartInfo
    {
        FileName = "/usr/sbin/system_profiler",
        Arguments = " sudo SPHardwareDataType SPDisplaysDataType",
        RedirectStandardOutput = true,
        UseShellExecute = false
    };

    var process = new Process { StartInfo = startInfo };
    process.Start();

    while (!process.StandardOutput.EndOfStream)
    {
        string line = process.StandardOutput.ReadLine();
        Console.WriteLine(line);
    }

    process.WaitForExit();

}

else
{
    // GET CPU info
    // Windows specific
    using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Processor"))
    {
        foreach (var item in searcher.Get())
        {
            Console.WriteLine("[CPU information]");
            Console.WriteLine("Processor Name: {0}", item["Name"]);
            Console.WriteLine("Manufacturer: {0}", item["Manufacturer"]);
            Console.WriteLine("Core: {0}", item["NumberOfCores"]);
            Console.WriteLine("Threads: {0}", item["NumberOfLogicalProcessors"]);
            Console.WriteLine("Clock Speed: {0}MHz", item["MaxClockSpeed"]);
                Console.WriteLine("L2 Cache: {0}MB", (Convert.ToInt64(item["L2CacheSize"]) / 1024));
            Console.WriteLine("L3 Cache: {0}MB", (Convert.ToInt64(item["L3CacheSize"]) / 1024));
            Console.WriteLine("Voltage: {0}V", item["CurrentVoltage"]);
            Console.WriteLine("-----------------------------------------------------------");
        }
    }

    // GET RAM info
    // Windows only
    using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PhysicalMemory"))
    {
        // GET total MEM
        long totalCapacity = 0;
        string manufacturer = null;
        foreach (ManagementObject item in searcher.Get())
        {
            totalCapacity += Convert.ToInt64(item["Capacity"]);
            manufacturer = item["Manufacturer"].ToString().Trim();
        }

        Console.WriteLine("[Memory Information]");
        Console.WriteLine("Total Capacity: {0} GB", totalCapacity / (1024 * 1024 * 1024));
        Console.WriteLine("Manufacturer: {0}", manufacturer);

        // GET Capacity per stick
        searcher.Query = new ObjectQuery("SELECT * FROM Win32_PhysicalMemory");
        int slotNumber = 1;
        foreach (ManagementObject item in searcher.Get())
        {
            Console.WriteLine();
            Console.WriteLine("(Slot {0})", slotNumber++);
            Console.WriteLine("Speed: {0} MHz", item["Speed"]);
            Console.WriteLine("Capacity: {0} GB", Convert.ToInt64(item["Capacity"]) / (1024 * 1024 * 1024));
        }
        Console.WriteLine("-----------------------------------------------------------");
    }

    // Windows specific
    // Retrieve GPU information
    // iGPU + dGPU
    using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController"))
    {
        foreach (var item in searcher.Get())
        {
            var manufacturer = item["AdapterCompatibility"].ToString();
            var VideoMemoryType = item["VideoMemoryType"].ToString();
            if (manufacturer.ToLower().Contains("intel") || manufacturer.ToLower().Contains("amd"))
            {
                Console.WriteLine("[Integrated GPU]");
                Console.WriteLine("Name: {0}", item["Name"]);
                Console.WriteLine("Manufacturer: {0}", manufacturer);
                Console.WriteLine("Driver Version: {0}", item["DriverVersion"]);
                Console.WriteLine("VRAM: {0}MB", Convert.ToUInt64(item["AdapterRAM"]) / (1024 * 1024));
                Console.WriteLine("-----------------------------------------------------------");
            }
            else
            {
                Console.WriteLine("[Dedicated GPU]");
                    Console.WriteLine("Number of GPUs: {0}", searcher.Get().Count);
                Console.WriteLine("Name: {0}", item["Name"]);
                Console.WriteLine("Manufacturer: {0}", manufacturer);
                Console.WriteLine("Driver Version: {0}", item["DriverVersion"]);
                }
        }
    }

    using (var factory = new Factory1())
    {
        using (var adapter = factory.GetAdapter(0))
        {
            var desc = adapter.Description;
                Console.WriteLine("Dedicated GPU Memory", desc.DedicatedVideoMemory / (1024 * 1024));
                Console.WriteLine("Shared GPU Memory: {0}MB", desc.SharedSystemMemory / (1024 * 1024));
                Console.WriteLine("-----------------------------------------------------------");
            }
        }
    }

Console.Write("Continue to benchmark? (y/n): ");
var input = Console.ReadLine();
if (string.Equals("y", "Y"))
{
    
            Console.WriteLine("Choose a benchmark to run:");
            Console.WriteLine("1. Hashing Benchmark");
            Console.WriteLine("2. Encryption Benchmark");
            Console.WriteLine("3. Multithreading Benchmark");
            Console.WriteLine("4. Run all benchmarks");
            Console.Write("Enter the number of your choice: ");

            string choice = Console.ReadLine();

        var benchmarkActions = new Dictionary<string, Action>
            {
            ["1"] = () => BenchmarkRunner.Run<HashingBenchmark>(),
            ["2"] = () => BenchmarkRunner.Run<EncryptionBenchmark>(),
            ["3"] = () => BenchmarkRunner.Run<MultithreadingBenchmark>(),
            ["4"] = () => BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).RunAllJoined()
        };

        if (benchmarkActions.TryGetValue(choice, out Action benchmarkAction))
        {
            benchmarkAction.Invoke();
        }
        else
        {
                    Console.WriteLine("Invalid choice.");
        }
            }
    }