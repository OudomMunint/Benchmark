using System;
using System.Threading;

class Spinner : IDisposable
{
    private const string Sequence = @"/-\|";
    private int counter = 0;
    private readonly int left;
    private readonly int top;
    private readonly int delay;
    private bool active;
    private readonly Thread thread;

    public Spinner(int left, int top, int delay = 100)
    {
        this.left = left;
        this.top = top;
        this.delay = delay;
        thread = new Thread(Spin);
    }

    public void Start()
    {
        active = true;
        if (!thread.IsAlive)
            thread.Start();
    }

    public void Stop()
    {
        active = false;
        Draw(' ');
    }

    private void Spin()
    {
        while (active)
        {
            Turn();
            Thread.Sleep(delay);
        }
    }

    private void Draw(char c)
    {
        Console.SetCursorPosition(left, top);
        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write(c);
    }

    private void Turn()
    {
        Draw(Sequence[++counter % Sequence.Length]);
    }

    public void Dispose()
    {
        Stop();
    }
}
class ConsoleSpinner
{
    private static readonly string[] SpinnerFrames = { "/", "-", "\\", "|" };
    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(0.3);
    private static Thread? spinnerThread;
    private static bool stopSpinner;

    public static void Start()
    {
        Console.CursorVisible = false;
        stopSpinner = false;
        spinnerThread = new Thread(Spin);
        spinnerThread.Start();
    }

    public static void Stop()
    {
        stopSpinner = true;
        spinnerThread?.Join();
    }

    private static void Spin()
    {
        Console.CursorVisible = false;
        int frameIndex = 0;
        while (!stopSpinner)
        {
            Console.Write(SpinnerFrames[frameIndex]);
            Console.SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop);
            frameIndex = (frameIndex + 1) % SpinnerFrames.Length;
            Thread.Sleep(Interval);
        }
    }
}

class ConsoleProgressBar
{
    private const int ProgressBarWidth = 50;
    private const char ProgressBarCharacter = '#';
    private const char BackgroundCharacter = '-';

    public static void DisplayProgressBar(int progressPercentage)
    {
        Console.CursorVisible = false;

        int completedWidth = (int)Math.Round(progressPercentage / 100.0 * ProgressBarWidth);
        int remainingWidth = ProgressBarWidth - completedWidth;

        string progressBar = new string(ProgressBarCharacter, completedWidth) + new string(BackgroundCharacter, remainingWidth);

        Console.Write("[{0}] {1}%", progressBar, progressPercentage);

        Console.SetCursorPosition(0, Console.CursorTop);
    }
}

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

class ConsoleIndeterminateBar
{
    private const int ProgressBarWidth = 50;
    private const char ProgressBarCharacter = '#';
    private const char BackgroundCharacter = '-';

    public static void DisplayProgressBar()
    {
        Console.CursorVisible = false;

        while (true)
        {
            for (int i = 0; i <= ProgressBarWidth; i++)
            {
                Console.SetCursorPosition(i, Console.CursorTop);
                Console.Write(ProgressBarCharacter);
                Thread.Sleep(50);
            }

            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write(new string(BackgroundCharacter, ProgressBarWidth + 1));
            Console.SetCursorPosition(0, Console.CursorTop);
        }
    }
}