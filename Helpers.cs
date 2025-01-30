using BenchmarkDotNet.Attributes;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Security.Cryptography;

class ConsoleInfo
{
    public static void GetAppInfo()
    {
        Assembly? assembly = Assembly.GetEntryAssembly();
        AssemblyName? assemblyName = assembly?.GetName();
        Version? version = assemblyName?.Version;
        string? fileVersion = assembly?.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version;

        //Header
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Welcome to the best benchmark in the entire universe");
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine("-----------------------------------------------------------");

        //Metadata
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"Version: v{version} ({fileVersion})");
        Console.WriteLine("Author(s): Oudom Munint");
        Console.WriteLine("Version History: https://github.com/OudomMunint/Benchmark/releases");
        Console.WriteLine($"Report Issues: https://github.com/OudomMunint/Benchmark/issues");

        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine("-----------------------------------------------------------");
    }
}

class ConsolePasswordHelper
{
    public static void PasswordPrompt()
    {
        Console.ForegroundColor = ConsoleColor.Cyan;

        Console.Write("Enter your password for sudo access: ");
        string? password = ReadPassword();

        var startInfo = new ProcessStartInfo
        {
            FileName = "/usr/bin/sudo"
        };

        var process = new Process { StartInfo = startInfo };
        process.Start();

        using (var writer = process.StandardInput)
        {
            writer.WriteLine(password);
        }

        while (!process.StandardOutput.EndOfStream)
        {
            string? line = process.StandardOutput.ReadLine();
            if (line != null)
            {
                Console.WriteLine(line);
            }
        }

        string? error = process.StandardError.ReadToEnd();
        if (!string.IsNullOrEmpty(error))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Error: {error}");
        }
    }

    // Read pw w/o echoing to console
    static string ReadPassword()
    {
        string password = string.Empty;
        ConsoleKeyInfo keyInfo;

        do
        {
            keyInfo = Console.ReadKey(intercept: true);
            if (keyInfo.Key != ConsoleKey.Enter)
            {
                password += keyInfo.KeyChar;
            }
        } while (keyInfo.Key != ConsoleKey.Enter);

        Console.WriteLine();
        return password;
    }
}