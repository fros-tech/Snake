
namespace Snake;


internal class MyConsole
{
    public const char Space = ' ';
    public const char Asterisk = '*'; 
    public const int DefaultDelay = 200;       // milliseconds; delay between an asterisks is moved to a new location
    public static int ConsoleHeight;
    public static int ConsoleWidth;
    public bool FreezeConsole;                 // Determines if asterisks should move around
    public bool HideAsterisks;                 // Determines whether asterisks are hidden or visible
    private object _lockMoving = new();        // locking object for when asterisk is moved
    private object _lockWriting = new();       // locking object for when number of asterisks is tallied
    public static Random Random = new();       // Randomizer for asterisks ext location
    private char[,]? _screenCopy;              // We save a copy of the consoles chars, so that we can see if an asterisk is already there
    // private List<Thread> _threads = new();

    
    public void InitializeConsole()
    {
        if (OperatingSystem.IsWindows())
        {
            Console.SetBufferSize(120, 30);
            Console.SetWindowSize(120, 30);
        }
        ConsoleWidth = Console.WindowWidth;
        ConsoleHeight = Console.WindowHeight;
        _screenCopy = new char[ConsoleWidth, ConsoleHeight];
        Console.Clear();
        Console.CursorVisible = false;
        FreezeConsole = false;
        for (var x = 0; x < ConsoleWidth; x++)
          for (var y = 0; y < ConsoleHeight; y++)
            _screenCopy[x, y] = Space;
    }

    public int getWidth() { return ConsoleWidth; }

    public int getHeight() { return ConsoleHeight; }

    public bool isBlank(int x, int y)
    {
        return (_screenCopy[x, y] == Space);
    }

    public bool isBlank(Position p)
    {
        return isBlank(p.XPos, p.YPos);
    }

    private void WriteAt(string s, Position aPos)
    {
        lock (_lockWriting)                                // Only one thread at a time here
        {
            int saveXPos = Console.CursorLeft;             // Not strictly necessary if ALL writes are done through this method
            int saveYPos = Console.CursorTop;
            Console.CursorLeft = aPos.XPos;
            Console.CursorTop = aPos.YPos;
            Console.Write(s);                              // This is where things are put on the console
            _screenCopy![aPos.XPos, aPos.YPos] = s[0];
            Console.CursorLeft = saveXPos;
            Console.CursorTop = saveYPos;
        }
    }

    public void WriteAt(string s, int x, int y)
    {
        Position p = new Position(x, y);
        WriteAt(s, p);
    }

    public void WriteAt(char c, Position aPos)
    {
        WriteAt(c.ToString(), aPos);
    }


    public void CloseConsole()
    {
        // foreach (Thread t in _threads) // Let's tidy up, and make sure all the threads close
        //     t.Join();
        Console.Clear();
    }

    public void ToggleFreeze()
    {
        FreezeConsole = !FreezeConsole;
    }
}