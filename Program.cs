// See https://aka.ms/new-console-template for more information
using Benchmark;
using BenchmarkDotNet.Running;
using System.Management;
using System.Diagnostics;
using Microsoft.CodeAnalysis;

Console.WriteLine("Welcome to the best benchmark in the entire universe");
Console.WriteLine("-----------------------------------------------------------");

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


// Run benchmark
BenchmarkRunner.Run<MyBenchmark>();