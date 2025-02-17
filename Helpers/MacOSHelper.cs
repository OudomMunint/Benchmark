using System.Diagnostics;
using System.Runtime.InteropServices;
using Hardware.Info;

class MacOSHelper
{
    public static void DisplayMacInfo()
    {
        IHardwareInfo hardwareInfo = new HardwareInfo();
        hardwareInfo.RefreshAll();

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("Fetching System Information...\n");

        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "/usr/sbin/system_profiler",
                Arguments = "SPSoftwareDataType SPHardwareDataType SPMemoryDataType SPDisplaysDataType",
                RedirectStandardOutput = true,
                UseShellExecute = false
            };

            using var process = new Process { StartInfo = startInfo };
            process.Start();

            // Store parsed values
            Dictionary<string, string> systemInfo = new();
            List<string> displayInfo = new();
            bool isGPUSection = false;

            while (!process.StandardOutput.EndOfStream)
            {
                string? line = process.StandardOutput.ReadLine()?.Trim();
                if (string.IsNullOrEmpty(line)) continue;

                if (line.StartsWith("Graphics/Displays:"))
                {
                    isGPUSection = true;
                    continue;
                }

                // Software Info
                if (line.StartsWith("System Version:"))
                    systemInfo["Operating system"] = line.Split(": ")[1].Split('(')[0].Trim();

                // CPU Info
                if (line.StartsWith("Chip:"))
                    systemInfo["CPU"] = line.Split(": ")[1];

                if (line.StartsWith("Total Number of Cores:") && !isGPUSection)
                {
                    systemInfo["Cores"] = line.Split(": ")[1].Split(" (")[0].Trim();
                    if (line.Contains("("))
                    {
                        string details = line.Substring(line.IndexOf("(")).Trim('(', ')');
                        systemInfo["CPU Core Breakdown"] = details;
                    }
                }

                // Memory Info
                if (line.StartsWith("Memory:") && !systemInfo.ContainsKey("Memory"))
                    systemInfo["Memory"] = line.Split(": ")[1];

                if (line.StartsWith("Type:") && !systemInfo.ContainsKey("Memory Type"))
                    systemInfo["Memory Type"] = line.Split(": ")[1];

                if (line.StartsWith("Manufacturer:") && systemInfo.ContainsKey("Memory"))
                    systemInfo["Memory Manufacturer"] = line.Split(": ")[1];

                // GPU Information
                if (isGPUSection && line.StartsWith("Chipset Model:"))
                    systemInfo["GPU"] = line.Split(": ")[1];

                if (isGPUSection && line.StartsWith("Total Number of Cores:"))
                    systemInfo["GPU Cores"] = line.Split(": ")[1] + " cores";

                if (line.StartsWith("Metal Support:"))
                    systemInfo["Metal support"] = line.Split(": ")[1];
            }

            process.WaitForExit();

            // Get CPU Model
            string cpuModel = systemInfo.GetValueOrDefault("CPU", "Unknown");

            // Determine Process Node based on family
            string processNode = cpuModel switch
            {
                var name when name.StartsWith("Apple M1") => "5nm (N5)",
                var name when name.StartsWith("Apple M2") => "5nm (N5P)",
                var name when name.StartsWith("Apple M3") => "3nm (N3B)",
                var name when name.StartsWith("Apple M4") => "3nm (N3E)",
                var name when name.StartsWith("Intel") => "14nm",
                _ => "Unknown"
            };

            Console.WriteLine("Software");
            Console.WriteLine($"    Operating system: {systemInfo.GetValueOrDefault("Operating system", "Unknown")}");

            string architecture = RuntimeInformation.OSArchitecture.ToString();
            Console.WriteLine($"    Architecture: {architecture}");

            Console.WriteLine("CPU");
            Console.WriteLine($"    CPU: {cpuModel}");
            Console.WriteLine($"    Process Node: {processNode}");

            if (RuntimeInformation.OSArchitecture == Architecture.Arm64)
            {
            Console.WriteLine($"    Core Layout: {systemInfo.GetValueOrDefault("CPU Core Breakdown", "Unknown")}");
            }

            if (RuntimeInformation.OSArchitecture == Architecture.Arm64)
            {
                Console.WriteLine("          P-Cores");
                foreach (var cpu in new[] { hardwareInfo.CpuList.First() })
            {
                    var L1Dcache = cpu.L1DataCacheSize / 1024;
                    var L1Icache = cpu.L1InstructionCacheSize / 1024;
                    var L2cache = cpu.L2CacheSize / 1024 / 1024;

                    Console.WriteLine($"               Cores/Threads: {cpu.NumberOfCores} / {cpu.NumberOfLogicalProcessors}");
                    Console.WriteLine($"               L1 Data Cache: {L1Dcache} KB");
                    Console.WriteLine($"               L1 Instruction Cache: {L1Icache} KB");
                    Console.WriteLine($"               L2 Cache: {L2cache} MB");
                }
                Console.WriteLine("          E-Cores");
                foreach (var cpu in new[] { hardwareInfo.CpuList.Skip(1).First() })
                {
                    var L1Dcache = cpu.L1DataCacheSize / 1024;
                    var L1Icache = cpu.L1InstructionCacheSize / 1024;
                    var L2cache = cpu.L2CacheSize / 1024 / 1024;

                    Console.WriteLine($"               Cores/Threads: {cpu.NumberOfCores} / {cpu.NumberOfLogicalProcessors}");
                    Console.WriteLine($"               L1 Data Cache: {L1Dcache} KB");
                    Console.WriteLine($"               L1 Instruction Cache: {L1Icache} KB");
                    Console.WriteLine($"               L2 Cache: {L2cache} MB");
                }
            }
            else
            {
                foreach (var cpu in hardwareInfo.CpuList)
                {
                    var L1Dcache = cpu.L1DataCacheSize / 1024;
                    var L1Icache = cpu.L1InstructionCacheSize / 1024;
                    var L2cache = cpu.L2CacheSize / 1024 / 1024;
                    var L3cache = cpu.L3CacheSize / 1024 / 1024;

                    Console.WriteLine($"     Cores/Threads: {cpu.NumberOfCores} / {cpu.NumberOfLogicalProcessors}");
                    Console.WriteLine($"     L1 Data Cache: {L1Dcache} KB");
                    Console.WriteLine($"     L1 Instruction Cache: {L1Icache} KB");
                    Console.WriteLine($"     L2 Cache: {L2cache} MB");
                    Console.WriteLine($"     L3 Cache: {L3cache} MB");
                }
            }

            Console.WriteLine("Memory");
            Console.WriteLine($"    Memory: {systemInfo.GetValueOrDefault("Memory", "Unknown")}");
            Console.WriteLine($"    Type: {systemInfo.GetValueOrDefault("Memory Type", "Unknown")}");
            Console.WriteLine($"    Manufacturer: {systemInfo.GetValueOrDefault("Memory Manufacturer", "Unknown")}");

            Console.WriteLine("GPU");
            Console.WriteLine($"    GPU: {systemInfo.GetValueOrDefault("GPU", "Unknown")}");
            Console.WriteLine($"    GPU Cores: {systemInfo.GetValueOrDefault("GPU Cores", "Unknown")}");
            Console.WriteLine($"    Metal support: {systemInfo.GetValueOrDefault("Metal support", "Unknown")}");
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Error fetching macOS system info: {ex.Message}");
        }
        finally
        {
            Console.ResetColor();
        }
    }

    static void Powermetrics()
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
}