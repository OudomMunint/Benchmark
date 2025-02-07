class GcHelper
{
    public static void MemoryCleanUp()
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("-----------------------------------------------------------");

        long memoryBefore = GC.GetTotalMemory(false);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("Cleaning up memory...");

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        long memoryAfter = GC.GetTotalMemory(true);
        long memoryFreed = memoryBefore - memoryAfter;

        Console.WriteLine($"Freed up {memoryFreed / (1024 * 1024 * 1024.0):F2} GB of memory.");
        Console.WriteLine("-----------------------------------------------------------");
    }
}