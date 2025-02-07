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
}