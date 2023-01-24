
namespace Snake;


internal class MyConsole
{
    public const char Space = ' ';
    public const char Asterisk = '*'; 
    private static int ConsoleHeight;
    private static int ConsoleWidth;
    private object _lockWriting = new();       // locking object for when char or string is written to the console
    private char[,]? _screenCopy;              // We save a copy of the consoles chars, so that we can see if an asterisk is already there
    
    public void InitializeConsole()
    {
        if (OperatingSystem.IsWindows())
        {
            Console.SetBufferSize(120, 50);
            Console.SetWindowSize(120, 50);
        }
        ConsoleWidth = Console.WindowWidth;
        ConsoleHeight = Console.WindowHeight;
        _screenCopy = new char[ConsoleWidth, ConsoleHeight];
        Console.Clear();
        Console.CursorVisible = false;
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

    public char CharAt(Position p)
    {
        return _screenCopy[p.XPos, p.YPos];
    }

    public bool isBlank(Position p)
    {
        return isBlank(p.XPos, p.YPos);
    }

    private void WriteAt(string s, Position aPos, ConsoleColor fgc, ConsoleColor bgc)
    {
        ConsoleColor oldFgc, oldBgc;
        lock (_lockWriting)                                // Only one thread at a time here
        {
            oldFgc = Console.ForegroundColor;
            oldBgc = Console.BackgroundColor;
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

    public void CloseConsole()
    {
        Console.Clear();
    }
}