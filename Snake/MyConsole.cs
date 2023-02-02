using System.Xml;

namespace Snake;

internal class MyConsole
{
    public const char Space = ' ';
    private static int _consoleHeight;
    private static int _consoleWidth;
    private readonly object _lockWriting = new();  // locking object for when char or string is written to the console
    private char[,]? _screenCopy;                  // We save a copy of the consoles chars, so that we can see if an asterisk is already there
    private static MyConsole instance;

    private MyConsole() {}

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
        Console.ForegroundColor = ConsoleColor.White;
        Console.BackgroundColor = ConsoleColor.Black;
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

    public int GetWidth() { return _consoleWidth; }

    public int GetHeight() { return _consoleHeight; }

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
            for (byte b = 0; b < s.Length; b++)
                _screenCopy[b + aPos.XPos, aPos.YPos] = s[b];
            //_screenCopy![aPos.XPos, aPos.YPos] = s[0];
            Console.ForegroundColor = oldFgc;
            Console.BackgroundColor = oldBgc;
        }
    }

    public void WriteAt(char c, Position aPos, ConsoleColor fgc, ConsoleColor bgc)
    {
        lock (_lockWriting)                                // Only one thread at a time here
        {
            ConsoleColor oldFgc = Console.ForegroundColor;
            ConsoleColor oldBgc = Console.BackgroundColor;
            Console.ForegroundColor = fgc;
            Console.BackgroundColor = bgc;
            Console.CursorLeft = aPos.XPos;
            Console.CursorTop = aPos.YPos;
            Console.Write(c);                              // This is where things are put on the console
            _screenCopy![aPos.XPos, aPos.YPos] = c;
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
        WriteAt(c, aPos, Console.ForegroundColor, Console.BackgroundColor);
    }

    public void InvertAt(Position pos)
    {
        throw new NotImplementedException();
    }

    public void DrawFrame(int x, int y, int Width, int Height)
    {
        WriteAt("+", x, y);           
        WriteAt("+", x, y + Height);
        WriteAt("+", x + Width, y); 
        WriteAt("+", x + Width, y + Height);
        for (byte b = 1; b < Width; b++)  { WriteAt("-", x + b, y); WriteAt("-", x + b, y + Height); }
        for (byte b = 1; b < Height; b++) { WriteAt("|", x, y + b); WriteAt("|", x + Width, y + b);  }
        for(byte i=1; i<Width; i++)
            for(byte j=1; j<Height; j++)
                WriteAt(" ",i+x,j+y);
    }

    public void RestoreConsole(char[,] sc)
    {
        for (int x = 0; x < _consoleWidth; x++)
          for (int y = 0; y < _consoleHeight; y++)
          { Console.CursorLeft = x;
            Console.CursorTop = y;
            Console.Write(sc[x, y]); }
    }

    public char PopUpQuestion(int Width, int Height, String message, String ValidResponses)
    {
        char[,] _tempScreenCopy = new char[_consoleWidth, _consoleHeight];
        _tempScreenCopy = (char[,]) _screenCopy.Clone();  // Easiest way to just restore entire console
        int x = (_consoleWidth / 2) - (Width / 2);
        int y = (_consoleHeight / 2) - (Height / 2);
        DrawFrame(x, y, Width, Height);
        WriteAt(message, x+2, y+2);
        char c = '?';
        do { if (Console.KeyAvailable) c = Console.ReadKey(true).KeyChar; } while (!ValidResponses.Contains(c));
        RestoreConsole(_tempScreenCopy);
        return c;
        // Determine size of the popup
        // Save copy of relevant area of _screencopy
    }
}