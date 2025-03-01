using System.Management;
using System.Runtime.InteropServices;
using Hardware.Info;

class WindowsHelper
{
    public static async void DisplayCpuInfo()
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
                        var L1DataCacheSize = cpu.L1DataCacheSize / 1024;
                        var L1InstructionCacheSize = cpu.L1InstructionCacheSize / 1024;

                        Console.WriteLine("{0}MHz", cpu.CurrentClockSpeed);

                        Console.ForegroundColor = ConsoleColor.White;
                        Console.Write("L1 Data Cache: ");
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("{0}KB", L1DataCacheSize);

                        Console.ForegroundColor = ConsoleColor.White;
                        Console.Write("L1 Instruction Cache: ");
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("{0}KB", L1InstructionCacheSize);
                    }

                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write("L2 Cache: ");
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("{0}MB", Convert.ToInt64(item["L2CacheSize"]) / 1024);

                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write("L3 Cache: ");
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("{0}MB", Convert.ToInt64(item["L3CacheSize"]) / 1024);

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

    public static async void DisplayRamInfo()
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
}