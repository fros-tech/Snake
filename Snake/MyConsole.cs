﻿using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;
using System.Xml;

namespace Snake;

internal class MyConsole
{
    [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    static extern SafeFileHandle CreateFile(
        string fileName,
        [MarshalAs(UnmanagedType.U4)] uint fileAccess,
        [MarshalAs(UnmanagedType.U4)] uint fileShare,
        IntPtr securityAttributes,
        [MarshalAs(UnmanagedType.U4)] FileMode creationDisposition,
        [MarshalAs(UnmanagedType.U4)] int flags,
        IntPtr template);

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern bool WriteConsoleOutputW(
        SafeFileHandle hConsoleOutput, 
        CharInfo[] lpBuffer, 
        Coord dwBufferSize, 
        Coord dwBufferCoord, 
        ref SmallRect lpWriteRegion);

    [StructLayout(LayoutKind.Sequential)]
    public struct Coord
    {
        public short X;
        public short Y;

        public Coord(short X, short Y)
        {
            this.X = X;
            this.Y = Y;
        }
    };

    [StructLayout(LayoutKind.Explicit)]
    public struct CharUnion
    {
        [FieldOffset(0)] public ushort UnicodeChar;
        [FieldOffset(0)] public byte AsciiChar;
    }

    [StructLayout(LayoutKind.Explicit)]
    private struct CharInfo
    {
        [FieldOffset(0)] public CharUnion Char;
        [FieldOffset(2)] public short Attributes;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct SmallRect
    {
        public short Left;
        public short Top;
        public short Right;
        public short Bottom;
    }

    //private const int WindowWidth = 120;
    //private const int WindowHeight = 45;
    private CharInfo[] buf;
    private SmallRect rect;
    private SafeFileHandle h;

    public const char Space = ' ';
    private static int _consoleHeight;
    private static int _consoleWidth;
    private readonly object _lockWriting = new();  // locking object for when char or string is written to the console
    // private char[,]? _screenCopy;                  // We save a copy of the consoles chars, so that we can see if an asterisk is already there
    private static MyConsole instance;
    private ScreenChar[,] _tempScreen;
    
    private struct ScreenChar
    {
        public char c;
        public ConsoleColor fgc;
        public ConsoleColor bgc;
    }

    private ScreenChar[,] _screenCopy_;

    private MyConsole() {}

    public static MyConsole GetInstance()
    {
        if (instance == null)
            instance = new MyConsole();
        return instance;
    }
    [STAThread]
    public void InitializeConsole()
    {
        h = CreateFile("CONOUT$", 0x40000000, 2, IntPtr.Zero, FileMode.Open, 0, IntPtr.Zero);
        if (!h.IsInvalid)
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
            // _screenCopy = new char[_consoleWidth, _consoleHeight];
            buf = new CharInfo[_consoleWidth * _consoleHeight];
            rect = new SmallRect() { Left = 0, Top = 0, Right = (short) _consoleWidth, Bottom = (short) _consoleHeight };
            _screenCopy_ = new ScreenChar[_consoleWidth, _consoleHeight];
            Console.Clear();
            ClearScreenCopy();
            Console.CursorVisible = false;
        }
    }

    private void ClearScreenCopy()
    {
        for (int x = 0; x < _consoleWidth; x++)
        for (int y = 0; y < _consoleHeight; y++)
        {
            // _screenCopy[x, y] = Space;
            _screenCopy_[x, y] = new ScreenChar();
            _screenCopy_[x, y].c = Space;
            _screenCopy_[x, y].fgc = ConsoleColor.White;
            _screenCopy_[x, y].bgc = ConsoleColor.Black;
        }
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
        return (_screenCopy_[x, y].c == Space);
    }

    public char CharAt(Position p)
    {
        return _screenCopy_[p.XPos, p.YPos].c;
    }

    public bool IsBlank(Position p)
    {
        return IsBlank(p.XPos, p.YPos);
    }

    private void WriteAt(string s, Position aPos, ConsoleColor fgc, ConsoleColor bgc)
    {
        lock (_lockWriting)  // Only one thread at a time here
        {
            for (int i = 0; i < s.Length; i++)
            {
                buf[(aPos.YPos * _consoleWidth) + aPos.XPos].Attributes = (short) ( (int) fgc  | ((int) bgc << 4));
                buf[(aPos.YPos * _consoleWidth) + aPos.XPos].Char.AsciiChar = Encoding.ASCII.GetBytes(s)[i];
                _screenCopy_[i + aPos.XPos, aPos.YPos].c = s[i];
                _screenCopy_[i + aPos.XPos, aPos.YPos].fgc = fgc;
                _screenCopy_[i + aPos.XPos, aPos.YPos].bgc = bgc;
            }
            bool b = WriteConsoleOutputW(h, buf, new Coord() { X = (short) _consoleWidth, Y = (short) _consoleHeight },
                new Coord() { X = 0, Y = 0 }, ref rect);
        }
    }

    public void WriteAt(char c, Position aPos, ConsoleColor fgc, ConsoleColor bgc)
    {
        lock (_lockWriting)  // Only one thread at a time here
        {
            buf[(aPos.YPos * _consoleWidth) + aPos.XPos].Attributes = (short) ( (int) fgc  | ((int) bgc << 4));
            buf[(aPos.YPos * _consoleWidth) + aPos.XPos].Char.UnicodeChar = (ushort)c;
            bool b = WriteConsoleOutputW(h, buf, new Coord() { X = (short) _consoleWidth, Y = (short) _consoleHeight },
                new Coord() { X = 0, Y = 0 }, ref rect);
            _screenCopy_[aPos.XPos, aPos.YPos].c = c;
            _screenCopy_[aPos.XPos, aPos.YPos].fgc = fgc;
            _screenCopy_[aPos.XPos, aPos.YPos].bgc = bgc;
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

    public void WriteAt(char c, int x, int y, ConsoleColor fgc, ConsoleColor bgc)
    {
        Position p = new Position(x, y);
        WriteAt(c, p, fgc, bgc);
    }

    public void WriteAt(char c, int x, int y)
    {
        Position p = new Position(x, y);
        WriteAt(c, p);
    }

    public void WriteAt(char c, Position aPos)
    {
        WriteAt(c, aPos, Console.ForegroundColor, Console.BackgroundColor);
    }

    public void InvertAt(Position pos)
    {
        WriteAt(_screenCopy_[pos.XPos, pos.YPos].c,pos,_screenCopy_[pos.XPos, pos.YPos].bgc,_screenCopy_[pos.XPos, pos.YPos].fgc );
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

    public void SaveScreen()
    {
        _tempScreen = BackupConsole();
    }

    public void RestoreScreen()
    {
        RestoreConsole(_tempScreen);
    }
    
    private void RestoreConsole(ScreenChar[,] sc)
    {
        // TODO Refactor for speed
        for (int x = 0; x < _consoleWidth; x++)
        for (int y = 0; y < _consoleHeight - 1; y++)
        {
            WriteAt(sc[x, y].c.ToString(), x, y, sc[x, y].fgc, sc[x, y].bgc);
        }
    }

    private ScreenChar[,] BackupConsole()
    {  // TODO Refactor for speed
        ScreenChar[,] _tmpScrCopy = new ScreenChar[_consoleWidth, _consoleHeight];
        for(int x=0; x<_consoleWidth; x++)
          for (int y = 0; y < _consoleHeight-1; y++)
          {
              _tmpScrCopy[x, y] = new ScreenChar
              {
                  c = _screenCopy_[x, y].c, 
                  fgc = _screenCopy_[x, y].fgc, 
                  bgc = _screenCopy_[x, y].bgc
              };
          }
        return _tmpScrCopy;
    }

    public ConsoleKey WaitForKey(ConsoleKey[] keys)
    {
        ConsoleKey k;
        do { k = Console.ReadKey(true).Key; } while (!keys.Contains(k));
        return k;
    }
    
    public ConsoleKey PopUpQuestion(int Width, int Height, String message, ConsoleKey[] ValidKeys)
    {
        ScreenChar[,] _tempScreenCopy = BackupConsole();
        int x = (_consoleWidth / 2) - (Width / 2);
        int y = (_consoleHeight / 2) - (Height / 2);
        DrawFrame(x, y, Width, Height);
        WriteAt(message, x+2, y+2);
        ConsoleKey k = WaitForKey(ValidKeys);
        RestoreConsole(_tempScreenCopy);
        return k;
    }
}