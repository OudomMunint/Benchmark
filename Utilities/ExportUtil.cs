class BenchmarkExporter
{
    public string? BenchmarkVersion { get; set; }
    public static void ExportResults(string filename, params string[] results)
    {
        string TotalTime = Program.TotalRunTime?.ToString() ?? "0";
        string version = ConsoleInfo.Version();
        string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        string output = $"Benchmark Run - {timestamp}\n" + $"Version: {version}\n" +
                        "--------------------------------\n" +
                        string.Join("\n", results) + "\n" + $"Total Run Time: {TotalTime} ms\n" +
                        "\n--------------------------------\n";

        File.AppendAllText(filename, output);
        Console.WriteLine($"Results exported to {filename}");
    }

    public static void TestExportResults()
    {
        List<string> testResults = new();
        testResults.Add("Hasing Benchmark: 1000ms.");
        testResults.Add("Encryption Benchmark: 2000ms.");
        testResults.Add("CPU Prime Computation: 3000ms.");
        testResults.Add("CPU Matrix Multiplication: 4000ms.");
        testResults.Add("Memory Bandwidth: 5000ms.");
        ExportResults("TestResults.txt", testResults.ToArray());
    }
}