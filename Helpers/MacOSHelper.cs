using System.Diagnostics;
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
            bool isGPUSection = false; // Detect when we're in the GPU section

            while (!process.StandardOutput.EndOfStream)
            {
                string? line = process.StandardOutput.ReadLine()?.Trim();
                if (string.IsNullOrEmpty(line)) continue;

                // Detect when we are parsing GPU section
                if (line.StartsWith("Graphics/Displays:"))
                {
                    isGPUSection = true;
                    continue;
                }

                // Software Information
                if (line.StartsWith("System Version:"))
                    systemInfo["Operating system"] = line.Split(": ")[1].Split('(')[0].Trim();

                // CPU Information
                if (line.StartsWith("Chip:"))
                    systemInfo["CPU"] = line.Split(": ")[1];

                if (line.StartsWith("Total Number of Cores:") && !isGPUSection) // Ensure it's CPU cores
                {
                    systemInfo["Cores"] = line.Split(": ")[1].Split(" (")[0].Trim(); // Extracts "10"
                    if (line.Contains("(")) // Extract performance/efficiency details
                    {
                        string details = line.Substring(line.IndexOf("(")).Trim('(', ')');
                        systemInfo["CPU Core Breakdown"] = details;
                    }
                }

                // Memory Information
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

                // Display Information
                if (line.StartsWith("Resolution:"))
                    displayInfo.Add("Resolution: " + line.Split(": ")[1].Split(" (")[0]);

                if (line.Contains("@") && line.Contains("Hz"))
                    displayInfo.Add("Refresh rate: " + line.Split("@ ")[1]);
            }

            process.WaitForExit();

            // 🔥 Formatted Output
            Console.WriteLine("Software");
            Console.WriteLine($"    Operating system: {systemInfo.GetValueOrDefault("Operating system", "Unknown")}");

            Console.WriteLine("CPU");
            Console.WriteLine($"    CPU: {systemInfo.GetValueOrDefault("CPU", "Unknown")}");
            Console.WriteLine($"    Cores: {systemInfo.GetValueOrDefault("Cores", "Unknown")}");
            Console.WriteLine($"    Threads: {systemInfo.GetValueOrDefault("Cores", "Unknown")}");
            Console.WriteLine($"    Core Breakdown: {systemInfo.GetValueOrDefault("CPU Core Breakdown", "Unknown")}");

            Console.WriteLine("Memory");
            Console.WriteLine($"    Memory: {systemInfo.GetValueOrDefault("Memory", "Unknown")}");
            Console.WriteLine($"    Type: {systemInfo.GetValueOrDefault("Memory Type", "Unknown")}");
            Console.WriteLine($"    Manufacturer: {systemInfo.GetValueOrDefault("Memory Manufacturer", "Unknown")}");

            Console.WriteLine("GPU");
            Console.WriteLine($"    GPU: {systemInfo.GetValueOrDefault("GPU", "Unknown")}");
            Console.WriteLine($"    GPU Cores: {systemInfo.GetValueOrDefault("GPU Cores", "Unknown")}");
            Console.WriteLine($"    Metal support: {systemInfo.GetValueOrDefault("Metal support", "Unknown")}");

            // Console.WriteLine("Display");
            // if (displayInfo.Count > 0)
            // {
            //     foreach (var item in displayInfo)
            //     {
            //         Console.WriteLine($"    {item}");
            //     }
            // }
            // else
            // {
            //     Console.WriteLine("    No display information found.");
            // }
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

    public static void DisplayMacInfo1()
    {
        IHardwareInfo hardwareInfo = new HardwareInfo();
        hardwareInfo.RefreshAll();

        foreach (var cpu in hardwareInfo.CpuList)
        {
            Console.WriteLine("{0}MHz", cpu.CurrentClockSpeed);

            Console.WriteLine(cpu.NumberOfCores.ToString());

            foreach (var cpuCore in cpu.CpuCoreList)
                Console.WriteLine(cpuCore);

            foreach (var hardware in hardwareInfo.MemoryList)
                Console.WriteLine(hardware);

            foreach (var hardware in hardwareInfo.VideoControllerList)
                Console.WriteLine(hardware);

            Console.WriteLine(hardwareInfo.OperatingSystem);

            Console.WriteLine(hardwareInfo.MemoryStatus);
        }

        foreach (var gpu in hardwareInfo.VideoControllerList)
        {
            Console.WriteLine(gpu);
        }

        foreach (var hardware in hardwareInfo.ComputerSystemList)
            Console.WriteLine(hardware);
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