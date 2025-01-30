using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using Benchmark;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using Hardware.Info;
using NvAPIWrapper;
using NvAPIWrapper.Display;
using NvAPIWrapper.GPU;
using SharpDX.DXGI;
using Device = SharpDX.Direct3D11.Device;

class Program
{
    static void Main(string[] args)
    {
        ConsoleInfo.GetAppInfo();

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            DisplayMacInfo();
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

    static void DisplayMacInfo()
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        var startInfo = new ProcessStartInfo
        {
            FileName = "/usr/sbin/system_profiler",
            Arguments = "SPSoftwareDataType SPHardwareDataType SPMemoryDataType SPDisplaysDataType",
            RedirectStandardOutput = true,
            UseShellExecute = false
        };

        var process = new Process { StartInfo = startInfo };
        process.Start();

        var exclusions = new List<string>
        {
            "Serial Number (system):", "Hardware UUID:", "Model Number:", "Provisioning UDID:", "Boot Volume:",
            "Boot Mode:", "Computer Name:", "User Name:", "Kernel Version: Darwin", "Secure Virtual Memory: Enabled",
            "System Integrity Protection: Enabled", "Time since boot:", "System Firmware Version:", "OS Loader Version:",
            "Activation Lock Status:", "Bus: Built-In", "Vendor:"
        };

        while (!process.StandardOutput.EndOfStream)
        {
            string? line = process.StandardOutput.ReadLine();
            if (line != null && !exclusions.Any(line.Contains))
            {
                Console.WriteLine(line);
            }
        }
    }

    static void DisplayMacInfoByPowermetrics()
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        var startInfo = new ProcessStartInfo
        {
            FileName = "/usr/bin/powermetrics",
            Arguments = "sudo powermetrics --samplers cpu_power,gpu_power -n 1s",
            RedirectStandardOutput = true,
            UseShellExecute = false
        };

        var process = new Process { StartInfo = startInfo };
        process.Start();

        while (!process.StandardOutput.EndOfStream)
        {
            string? line = process.StandardOutput.ReadLine();
            if (line != null)
            {
                Console.WriteLine(line);
            }
        }
    }

    static void DisplayWindowsInfo()
    {
        ConsoleSpinner.Start();
        DisplayCpuInfo();
        DisplayRamInfo();
        DisplayGpuInfo();
        ConsoleSpinner.Stop();
    }

    static async void DisplayCpuInfo()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Processor");
            foreach (var item in searcher.Get())
            {
                try
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine("[CPU Information]");

                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write("Processor Name: ");
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine(item["Name"]);

                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write("Cores: ");
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write($"{item["NumberOfCores"]}");

                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write(", Threads: ");
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine(item["NumberOfLogicalProcessors"]);

                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write("Base Frequency: ");
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("{0}MHz", item["MaxClockSpeed"]);

                    IHardwareInfo hardwareInfo = new HardwareInfo();
                    hardwareInfo.RefreshAll();
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write("Current Frequency: ");
                    Console.ForegroundColor = ConsoleColor.Yellow;

                    foreach (var cpu in hardwareInfo.CpuList)
                    {
                        Console.WriteLine("{0}MHz", cpu.CurrentClockSpeed);
                    }

                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write("L2 Cache: ");
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("{0}MB", Convert.ToInt64(item["L2CacheSize"]) / 1024);

                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write("L3 Cache: ");
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("{0}MB", Convert.ToInt64(item["L3CacheSize"]) / 1024);

                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write("Voltage: ");
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("{0}V", item["CurrentVoltage"]);

                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.WriteLine("-----------------------------------------------------------");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("An error occurred while retrieving CPU information: " + ex.Message);
                }
                await Task.Delay(500);
            }
        }
    }

    static async void DisplayRamInfo()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PhysicalMemory");
            try
            {
                long totalCapacity = 0;
                string? manufacturer = null;
                foreach (ManagementObject item in searcher.Get().Cast<ManagementObject>())
                {
                    totalCapacity += Convert.ToInt64(item["Capacity"]);
                    manufacturer = item["Manufacturer"]?.ToString()?.Trim();
                }

                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("[Memory Information]");

                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("Total Capacity: ");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("{0} GB", totalCapacity / (1024 * 1024 * 1024));

                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("Manufacturer: ");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(manufacturer);

                searcher.Query = new ObjectQuery("SELECT * FROM Win32_PhysicalMemory");
                int slotNumber = 1;
                foreach (ManagementObject item in searcher.Get().Cast<ManagementObject>())
                {
                    Console.WriteLine();
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("(Slot {0})", slotNumber++);

                    Console.Write("Speed: ");
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("{0} MHz", item["Speed"]);

                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write("Capacity: ");
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("{0} GB", Convert.ToInt64(item["Capacity"]) / (1024 * 1024 * 1024));
                }

                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine("-----------------------------------------------------------");
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred while retrieving memory information: " + ex.Message);
            }
            await Task.Delay(500);
        }
    }

    static async void DisplayGpuInfo()
    {
        try
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController");
                bool hasNvidiaGPU = PhysicalGPU.GetPhysicalGPUs().Any(gpu => gpu.FullName.Contains("NVIDIA"));

                if (hasNvidiaGPU)
                {
                    NVIDIA.Initialize();
                    var nvidiaGPUs = PhysicalGPU.GetPhysicalGPUs();
                    var driver = NVIDIA.DriverVersion;
                    var driverbranch = NVIDIA.DriverBranchVersion;

                    foreach (var gpu in nvidiaGPUs)
                    {
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine("[GPU Information]");

                        Console.ForegroundColor = ConsoleColor.White;
                        Console.Write("GPU Type: ");
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine(gpu.GPUType);

                        Console.ForegroundColor = ConsoleColor.White;
                        Console.Write("Name: ");
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine(gpu.FullName);

                        Console.ForegroundColor = ConsoleColor.White;
                        Console.Write("GPU Core: ");
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine(gpu.ArchitectInformation.ShortName);

                        Console.ForegroundColor = ConsoleColor.White;
                        Console.Write("Shaders: ");
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine(gpu.ArchitectInformation.NumberOfCores);

                        var graphicsClockMHz = gpu.BoostClockFrequencies.GraphicsClock.Frequency / 1000;
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.Write("GPU Core Speed: ");
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("{0} MHz", graphicsClockMHz);

                        Console.ForegroundColor = ConsoleColor.White;
                        Console.Write("VRAM: ");
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("{0} MB", gpu.MemoryInformation.DedicatedVideoMemoryInkB / 1024);

                        Console.ForegroundColor = ConsoleColor.White;
                        Console.Write("VRAM Type: ");
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine(gpu.MemoryInformation.RAMType);

                        var memoryClockMHz = gpu.BoostClockFrequencies.MemoryClock.Frequency / 1000;
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.Write("VRAM Frequency: ");
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("{0} MHz", memoryClockMHz);
                    }
                }

                foreach (var item in searcher.Get())
                {
                    var manufacturer = item["AdapterCompatibility"]?.ToString();
                    if (manufacturer == null) continue;

                    if (manufacturer.ToLower().Contains("nvidia")) continue;

                    if (manufacturer.ToLower().Contains("intel") || manufacturer.ToLower().Contains("amd"))
                    {
                        Console.ForegroundColor = ConsoleColor.Magenta;
                        Console.WriteLine("-----------------------------------------------------------");

                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine("[Integrated GPU]");

                        Console.ForegroundColor = ConsoleColor.White;
                        Console.Write("Name: ");
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine(item["Name"]);

                        Console.ForegroundColor = ConsoleColor.White;
                        Console.Write("Manufacturer: ");
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine(manufacturer);

                        Console.ForegroundColor = ConsoleColor.White;
                        Console.Write("Driver Version: ");
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine(item["DriverVersion"]);

                        Console.ForegroundColor = ConsoleColor.White;
                        Console.Write("VRAM: ");
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("{0} MB", Convert.ToUInt64(item["AdapterRAM"]) / (1024 * 1024));

                        Console.ForegroundColor = ConsoleColor.Magenta;
                        Console.WriteLine("-----------------------------------------------------------");
                    }
                    else
                    {
                        if (manufacturer.ToLower().Contains("advanced micro devices"))
                        {
                            manufacturer = "AMD";
                        }

                        if (!hasNvidiaGPU)
                        {
                            using var factory = new Factory1();
                            using var adapter = factory.GetAdapter(0);
                            var desc = adapter.Description;

                            Console.ForegroundColor = ConsoleColor.White;
                            Console.Write("Name: ");
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine(desc.Description);

                            Console.ForegroundColor = ConsoleColor.White;
                            Console.Write("Manufacturer: ");
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine(manufacturer);

                            Console.ForegroundColor = ConsoleColor.White;
                            Console.Write("Driver Version: ");
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine(item["DriverVersion"]);

                            if (desc.DedicatedVideoMemory == 0)
                            {
                                Console.WriteLine("No dedicated GPU memory found");
                            }
                            else
                            {
                                Console.ForegroundColor = ConsoleColor.White;
                                Console.Write("VRAM: ");
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.WriteLine("{0} MB", desc.DedicatedVideoMemory / (1024 * 1024));

                                Console.ForegroundColor = ConsoleColor.White;
                                Console.Write("Shared Memory: ");
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.WriteLine("{0} MB", desc.SharedSystemMemory / (1024 * 1024));
                            }

                            Console.ForegroundColor = ConsoleColor.Magenta;
                            Console.WriteLine("-----------------------------------------------------------");
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("An error occurred while retrieving GPU information: " + ex.Message);
        }
        await Task.Delay(500);
    }

    static void RunBenchmark()
    {
        Console.WriteLine("Choose a benchmark to run:");

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("1. Hashing Benchmark");
        Console.WriteLine("2. Encryption Benchmark");
        Console.WriteLine("3. Multithread Benchmark");
        Console.WriteLine("4. Run all benchmarks");
#if DEBUG
        Console.WriteLine("5. Debug Mode");
#endif

        Console.ForegroundColor = ConsoleColor.White;
        Console.Write("Enter the number of your choice: ");

        string? choice = Console.ReadLine();

        var benchmarkActions = new Dictionary<string, Action>
        {
            ["1"] = () => BenchmarkRunner.Run<HashingBenchmark>(),
            ["2"] = () => BenchmarkRunner.Run<EncryptionBenchmark>(),
            ["3"] = () => BenchmarkRunner.Run<CPUBenchmark>(),
            ["4"] = () => BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).RunAll(),
#if DEBUG
            ["5"] = () => BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(new[] { "Benchmarks" }, new DebugInProcessConfig())
#endif
        };

        if (choice != null && benchmarkActions.TryGetValue(choice, out Action? benchmarkAction))
        {
            Console.WriteLine("-----------------------------------------------------------");
            Console.WriteLine(" ");

            Stopwatch stopwatch = Stopwatch.StartNew(); // Start Global Timer
            benchmarkAction?.Invoke();
            stopwatch.Stop(); // Stop Global Timer

            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine($"Total Execution Time: {stopwatch.ElapsedMilliseconds} ms.");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("-----------------------------------------------------------");
        }
        else
        {
            Console.WriteLine("Invalid choice.");
        }
    }
}