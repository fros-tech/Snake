
namespace Snake;


internal class MyConsole
{
    public const char Space = ' ';
    public const char Asterisk = '*'; 
    // private readonly List<Asterisk> _asterisks = new(); // Used when accessing the asterisk on screen
    public const int DefaultDelay = 200;       // milliseconds; delay between an asterisks is moved to a new location
    public static int ConsoleHeight;
    public static int ConsoleWidth;
    public bool FreezeConsole;                 // Determines if asterisks should move around
    public bool HideAsterisks;                 // Determines whether asterisks are hidden or visible
    private object _lockMoving = new();        // locking object for when asterisk is moved
    private object _lockWriting = new();       // locking object for when number of asterisks is tallied
    public static Random Random = new();       // Randomizer for asterisks ext location

    private char[,]? _screenCopy;              // We save a copy of the consoles chars, so that we can see if an asterisk is already there

    private List<Thread> _threads = new();

    public void InitializeConsole()
    {
        Console.SetBufferSize(230,60);
        Console.SetWindowSize(230,60);
        Console.WriteLine("LargestWindowWidth  :{0}", Console.LargestWindowWidth);
        Console.WriteLine("LargestWindowHeight :{0}", Console.LargestWindowHeight);
        Console.WriteLine("BufferWidth         :{0}", Console.BufferWidth);
        Console.WriteLine("BufferHeight        :{0}", Console.BufferHeight);
        _screenCopy = new char[Console.WindowWidth, Console.WindowHeight];
        Console.Clear();
        Console.CursorVisible = false;
        FreezeConsole = false;
        HideAsterisks = false;
        ConsoleWidth = Console.WindowWidth;
        ConsoleHeight = Console.WindowHeight;
        for (var x = 0; x < ConsoleWidth; x++)
          for (var y = 0; y < ConsoleHeight; y++)
            _screenCopy[x, y] = Space;
    }

    private void WriteAt(string s, Position aPos)
    {
        lock (_lockWriting) // Only one thread at a time here
        {
            int saveXPos = Console.CursorLeft;
            int saveYPos = Console.CursorTop;
            Console.CursorLeft = aPos.XPos;
            Console.CursorTop = aPos.YPos;
            Console.Write(s);
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
        _screenCopy![aPos.XPos, aPos.YPos] = c;
        WriteAt(c.ToString(), aPos);
    }

    public void AddAsterisk()
    {
        // Instantiates a new asterisks, adds it to the asterisks list,
        // creates a thread, adds it to the threads list and starts it
        Asterisk asterisk = new Asterisk(this);
        _asterisks.Add(asterisk);
        Thread thread = new Thread(asterisk.Run);
        _threads.Add(thread);
        thread.Start();
    }

    public void RemoveAsterisk()
    {
        lock (_lockMoving)
        {
            // Remove exactly one asterisk from screen and stop its thread
            if (!_asterisks.Any()) return; // Fail early
            Asterisk asteriskToRemove = _asterisks.First();
            KillAsterisk(asteriskToRemove);
        }
    }

    public void RemoveAllAsterisks()
    {
        foreach (Asterisk a in _asterisks.ToList())  // ToList() avoids collection modified exception
            KillAsterisk(a);
    }

    private void KillAsterisk(Asterisk a)
    {
        a.TurnOffAsterisk(); // remove the asterisk from the console
        a.IsAlive = false; // Should cause the thread to stop
        _asterisks.Remove(a);
    }

    public void CloseConsole()
    {
        RemoveAllAsterisks();
        foreach (Thread t in _threads) // Let's tidy up, and make sure all the threads close
            t.Join();
        Console.Clear();
    }

    public int NumAsterisks()
    {
        return _asterisks.Count;
    }

    public void ShowAsterisk(Asterisk a)
    {
        lock (_lockMoving)
        {
            do
            {
                a.Pos.Randomize();
            } while (_screenCopy[a.Pos.XPos, a.Pos.YPos] == Asterisk);
            if(a.Visible)
              WriteAt(Asterisk, a.Pos);
        }
    }

    public void MoveAsterisk(Asterisk a)
    {
        lock (_lockMoving)
        {
            WriteAt(Space, a.Pos);
            ShowAsterisk(a);
        }
    }

    public void ToggleFreeze()
    {
        FreezeConsole = !FreezeConsole;
    }

    public void ToggleVisible()
    {
        lock (_lockMoving)
        {
            foreach (Asterisk a in _asterisks)
            {
                a.Visible = !a.Visible;
                WriteAt(!a.Visible ? Space : Asterisk, a.Pos);
            }
            HideAsterisks = !HideAsterisks;
        }
    }
}