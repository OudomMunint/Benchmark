// See https://aka.ms/new-console-template for more information
using Benchmark;
using BenchmarkDotNet.Running;
using System.Management;
using System.Diagnostics;
using Microsoft.CodeAnalysis;
using System.Runtime.InteropServices;

Console.WriteLine("Welcome to the best benchmark in the entire universe");
Console.WriteLine("-----------------------------------------------------------");

if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
{
    static void Main(string[] args)
    {
        Console.WriteLine("Welcome to the best benchmark in the entire universe");
        Console.WriteLine("-----------------------------------------------------------");

        // Get CPU information
        Console.WriteLine("[CPU information]");
        Console.WriteLine(GetShellOutput("sysctl -n machdep.cpu.brand_string"));
        Console.WriteLine("Physical Cores: " + GetShellOutput("sysctl -n hw.physicalcpu"));
        Console.WriteLine("Logical Cores: " + GetShellOutput("sysctl -n hw.logicalcpu"));
        Console.WriteLine("Max Frequency: " + GetShellOutput("sysctl -n machdep.cpu.max_frequency") + " Hz");
        Console.WriteLine("L3 Cache Size: " + (long.Parse(GetShellOutput("sysctl -n hw.l3cachesize")) / 1024 / 1024) + " MB");
        Console.WriteLine("-----------------------------------------------------------");

        // Get RAM information
        Console.WriteLine("[Memory Information]");
        Console.WriteLine("Total Capacity: " + (long.Parse(GetShellOutput("sysctl -n hw.memsize")) / 1024 / 1024 / 1024) + " GB");
        Console.WriteLine(GetShellOutput("system_profiler SPMemoryDataType | grep \"Type:\\|Size:\""));
        Console.WriteLine("-----------------------------------------------------------");

        // Get GPU information
        Console.WriteLine("[GPU Information]");
        Console.WriteLine(GetShellOutput("system_profiler SPDisplaysDataType | grep \"Chipset Model:\" | awk '{print $3 \" \" $4}'"));
        Console.WriteLine("VRAM: " + GetShellOutput("system_profiler SPDisplaysDataType | grep \"VRAM (Dynamic, Max):\" | awk '{print $4}'"));
        Console.WriteLine("-----------------------------------------------------------");
    }

    static string GetShellOutput(string command)
    {
        var process = new Process()
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "/bin/bash",
                Arguments = $"-c \"{command}\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            }
        };
        process.Start();
        string output = process.StandardOutput.ReadToEnd();
        process.WaitForExit();
        return output.Trim();
    }
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
            Console.WriteLine("VRAM: {0}MB", Convert.ToUInt32(item["AdapterRAM"]) / (1024 * 1024));
            Console.WriteLine("-----------------------------------------------------------");
        }
        else
        {
            Console.WriteLine("[Dedicated GPU]");
            Console.WriteLine("Name: {0}", item["Name"]);
            Console.WriteLine("Manufacturer: {0}", manufacturer);
            Console.WriteLine("Driver Version: {0}", item["DriverVersion"]);
            Console.WriteLine("VRAM: {0}MB", Convert.ToUInt32(item["AdapterRAM"]) / (1024 * 1024));
            Console.WriteLine("-----------------------------------------------------------");
        }
    }
}
}

Console.Write("Continue to benchmark? (y/n): ");
var input = Console.ReadLine();
if (input.ToLower() == "y")
{
    BenchmarkRunner.Run<MyBenchmark>();
}