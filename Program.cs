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
using NvAPIWrapper;
using NvAPIWrapper.Display;
using NvAPIWrapper.GPU;
using Hardware.Info;
using NvAPIWrapper.DRS.SettingValues;
using System.Linq;
using System.Net.NetworkInformation;

Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine("Welcome to the best benchmark in the entire universe");

Console.ForegroundColor = ConsoleColor.Magenta;
Console.WriteLine("-----------------------------------------------------------");

if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
{
    Console.ForegroundColor = ConsoleColor.Cyan;
    var startInfo = new ProcessStartInfo
    {
        FileName = "/usr/sbin/system_profiler",
        Arguments = " SPSoftwareDataType SPHardwareDataType SPMemoryDataType SPDisplaysDataType",
        RedirectStandardOutput = true,
        UseShellExecute = false
    };

    var process = new Process { StartInfo = startInfo };
    process.Start();

    while (!process.StandardOutput.EndOfStream)
    {
        string? line = process.StandardOutput.ReadLine();
        //Exclusions
        if (line != null && !line.Contains("Serial Number (system):") && !line.Contains("Hardware UUID:") && !line.Contains("Model Number:") && !line.Contains("Provisioning UDID:") && !line.Contains("Boot Volume:")
            && !line.Contains("Boot Mode:") && !line.Contains("Computer Name:") && !line.Contains("User Name:") && !line.Contains("Kernel Version: Darwin") && !line.Contains("Secure Virtual Memory: Enabled")
            && !line.Contains("System Integrity Protection: Enabled") && !line.Contains("Time since boot:") && !line.Contains("System Firmware Version:") && !line.Contains("OS Loader Version:")
            && !line.Contains("Activation Lock Status:") && !line.Contains("Bus: Built-In") && !line.Contains("Vendor:"))
        {
            Console.WriteLine(line);
        }
    }
}

else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
{
    // GET CPU info
    // Windows specific
    using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Processor"))
    {
        foreach (var item in searcher.Get())
        {
            try
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("[CPU information]");

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

                IHardwareInfo hardwareInfo;
                hardwareInfo = new HardwareInfo();
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
                Console.WriteLine("{0}MB", (Convert.ToInt64(item["L2CacheSize"]) / 1024));

                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("L3 Cache: ");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("{0}MB", (Convert.ToInt64(item["L3CacheSize"]) / 1024));

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
        }
    }

    // GET RAM info
    // Windows only
    using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PhysicalMemory"))
    {
        try
        {
            // GET total MEM
            long totalCapacity = 0;
            string? manufacturer = null;
            foreach (ManagementObject item in searcher.Get())
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

            // GET Capacity per stick
            searcher.Query = new ObjectQuery("SELECT * FROM Win32_PhysicalMemory");
            int slotNumber = 1;
            foreach (ManagementObject item in searcher.Get())
            {
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("(Slot {0})", slotNumber++);

                Console.Write("Speed: " );
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
    }
    // Windows specific
    // Retrieve GPU information
    // iGPU
    try
    {
        using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController"))
        {
            bool hasNvidiaGPU = PhysicalGPU.GetPhysicalGPUs().Any(gpu => gpu.FullName.Contains("NVIDIA"));

            if (hasNvidiaGPU)
            {
                NVIDIA.Initialize();

                //var nvidiaGPUs = PhysicalGPU.GetPhysicalGPUs().Where(gpu => gpu.FullName.Contains("NVIDIA")).ToList();
                var nvidiaGPUs = PhysicalGPU.GetPhysicalGPUs();
                var driver = NVIDIA.DriverVersion;
                var driverbranch = NVIDIA.DriverBranchVersion;

                foreach (var gpu in nvidiaGPUs)
                {
                    //Header
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

                    var graphicsClockKHz = gpu.BoostClockFrequencies.GraphicsClock.Frequency;
                    var graphicsClockMHz = graphicsClockKHz / 1000;

                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write("GPU Core Speed: ");
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("{0} MHz", graphicsClockMHz);

                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write("VRAM: ");
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("{0} MB", gpu.MemoryInformation.DedicatedVideoMemoryInkB / 1024); // Convert kB to MB

                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write("VRAM Type: ");
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine(gpu.MemoryInformation.RAMType);

                    var memoryClockKHz = gpu.BoostClockFrequencies.MemoryClock.Frequency;
                    var memoryClockMHz = memoryClockKHz / 1000;

                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write("VRAM Frequency: ");
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("{0} MHz", memoryClockMHz);

                }
            }

            foreach (var item in searcher.Get())
            {
                var manufacturer = item["AdapterCompatibility"]?.ToString();
                var VideoMemoryType = item["VideoMemoryType"]?.ToString();

                //if (manufacturer != null && manufacturer.ToLower().Contains("nvidia"))
                //{
                //    hasNvidiaGPU = true;
                //    break;  // Exit the loop if an NVIDIA GPU is found
                //}

                if (manufacturer != null && (manufacturer.ToLower().Contains("intel") || manufacturer.ToLower().Contains("amd")))
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
                    // shorten Advanced Micro Devices, Inc. to AMD
                    if (manufacturer != null && manufacturer.ToLower().Contains("advanced micro devices"))
                    {
                        manufacturer = "AMD";
                    }

                    if (!hasNvidiaGPU)
                    {
                        using (var factory = new Factory1())
                        {
                            using (var adapter = factory.GetAdapter(0))
                            {
                                var desc = adapter.Description;
                                var adapterMain = adapter;

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
        }
    }

    catch (Exception ex)
    {
        Console.WriteLine("An error occurred while retrieving GPU information: " + ex.Message);
    }
}

    Console.ForegroundColor = ConsoleColor.White;
    Console.Write("Continue to benchmark? (y/n): ");
    var input = Console.ReadLine();

    if (string.Equals(input, "y", StringComparison.OrdinalIgnoreCase))
    {
        Console.WriteLine("Choose a benchmark to run:");

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("1. Hashing Benchmark");
        Console.WriteLine("2. Encryption Benchmark");
        Console.WriteLine("3. Multithreading Benchmark");
        Console.WriteLine("4. Run all benchmarks");

        Console.ForegroundColor = ConsoleColor.White;
        Console.Write("Enter the number of your choice: ");

        string? choice = Console.ReadLine();

        var benchmarkActions = new Dictionary<string, Action>
        {
            ["1"] = () => BenchmarkRunner.Run<HashingBenchmark>(),
            ["2"] = () => BenchmarkRunner.Run<EncryptionBenchmark>(),
            ["3"] = () => BenchmarkRunner.Run<MultithreadingBenchmark>(),
            ["4"] = () => BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).RunAllJoined()
        };

        if (choice != null && benchmarkActions.TryGetValue(choice, out Action? benchmarkAction))
        {
            benchmarkAction?.Invoke();
        }
        else
        {
            Console.WriteLine("Invalid choice.");
        }
    }