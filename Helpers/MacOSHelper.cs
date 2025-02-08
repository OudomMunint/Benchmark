using System.Diagnostics;

class MacOSHelper
{
    public static void DisplayMacInfo()
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