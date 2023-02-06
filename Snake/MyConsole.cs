namespace Snake;

internal class MyConsole
{
    public const char Space = ' ';
    private static int _consoleHeight;
    private static int _consoleWidth;
    private readonly object _lockWriting = new();  // locking object for when char or string is written to the console
    private char[,]? _screenCopy;                  // We save a copy of the consoles chars, so that we can see if an asterisk is already there
    private static MyConsole instance;

    private struct character  //  TODO Refactor to hold foreground and background colors in _screencopy
    {
        private char c;
        private ConsoleColor fg;
        private ConsoleColor bg;
    }

    private MyConsole() {}  // Only here to make the constructor private (Singleton pattern)

    public static MyConsole GetInstance()
    {
        if (instance == null)
            instance = new MyConsole();
        return instance;
    }
    public void InitializeConsole()
    {
        if (OperatingSystem.IsWindows())
        {
            Console.SetBufferSize(120, 50);
            Console.SetWindowSize(120, 50);
        }
        _consoleWidth = Console.WindowWidth;
        _consoleHeight = Console.WindowHeight;
        _screenCopy = new char[_consoleWidth, _consoleHeight];
        Console.Clear();
        Console.CursorVisible = false;
        ClearScreenCopy();
    }

    private void ClearScreenCopy()
    {
        for (int x = 0; x < _consoleWidth; x++)
          for (int y = 0; y < _consoleHeight; y++) 
              _screenCopy[x, y] = Space;
    }

    public static int GetWidth() { return _consoleWidth; }

    public static int GetHeight() { return _consoleHeight; }

    public void ClearConsole()
    {
        Console.Clear();
        ClearScreenCopy();
    }

    public bool IsBlank(int x, int y)
    {
        return (_screenCopy[x, y] == Space);
    }

    public char CharAt(Position p)
    {
        return _screenCopy[p.XPos, p.YPos];
    }

    public bool IsBlank(Position p)
    {
        return IsBlank(p.XPos, p.YPos);
    }

    private void WriteAt(string s, Position aPos, ConsoleColor fgc, ConsoleColor bgc)
    {
        lock (_lockWriting)                                // Only one thread at a time here
        {
            ConsoleColor oldFgc = Console.ForegroundColor;
            ConsoleColor oldBgc = Console.BackgroundColor;
            Console.ForegroundColor = fgc;
            Console.BackgroundColor = bgc;
            Console.CursorLeft = aPos.XPos;
            Console.CursorTop = aPos.YPos;
            Console.Write(s);                              // This is where things are put on the console
            _screenCopy![aPos.XPos, aPos.YPos] = s[0];
            Console.ForegroundColor = oldFgc;
            Console.BackgroundColor = oldBgc;
        }
    }

    public void WriteAt(string s, int x, int y)
    {
        Position p = new Position(x, y);
        WriteAt(s, p, Console.ForegroundColor, Console.BackgroundColor);
    }
    public void WriteAt(string s, int x, int y, ConsoleColor fgc, ConsoleColor bgc)
    {
        Position p = new Position(x, y);
        WriteAt(s, p, fgc, bgc);
    }

    public void WriteAt(char c, Position aPos)
    {
        WriteAt(c.ToString(), aPos, Console.ForegroundColor, Console.BackgroundColor);
    }

    public void WriteAt(char c, Position aPos, ConsoleColor fgc, ConsoleColor bgc)
    {
        WriteAt(c.ToString(), aPos, fgc, bgc);
    }

    
    public void InvertAt(Position pos)
    {
        throw new NotImplementedException();
    }

    public void DrawFrame(int x, int y, int Width, int Height)
    {
        WriteAt("+", x, y);
        WriteAt("+", 0, Height);
        WriteAt("+", Width, 0);
        WriteAt("+", Width, Height);
        for (byte b = 1; b < Width - 1; b++)
        {
            WriteAt("-", b, 0);
            WriteAt("-", b, Height);
        }
        for (byte b = 1; b < Height; b++)
        {
            WriteAt("|", 0, b);
            WriteAt("|", Width, b);
        }
    }
    
    public char PopUpQuestion(int Width, int Height, String message, String ValidResponses)
    {
        // Determine size of the popup
        // Save copy of relevant area of _screencopy
        throw new NotImplementedException();
    }
}