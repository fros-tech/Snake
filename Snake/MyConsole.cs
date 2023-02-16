using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;

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
    private struct Coord
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
    public struct CharInfo
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

    private CharInfo[] buf, bufCopy;
    private SmallRect rect;
    private SafeFileHandle h;

    public const char Space = ' ';
    private static int _consoleHeight;
    private static int _consoleWidth;
    private readonly object _lockWriting = new();  // locking object for when char or string is written to the console
    private static MyConsole instance;
    
    private struct ScreenChar
    {
        public char c;
        public ConsoleColor fgc;
        public ConsoleColor bgc;
    }

    // private ScreenChar[,] _screenCopy_;

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
            bufCopy = new CharInfo[_consoleWidth * _consoleHeight];
            rect = new SmallRect() { Left = 0, Top = 0, Right = (short) _consoleWidth, Bottom = (short) _consoleHeight };
            // _screenCopy_ = new ScreenChar[_consoleWidth, _consoleHeight];
            Console.Clear();
            ClearScreenCopy();
            Console.CursorVisible = false;
        }
    }

    private void ClearScreenCopy()
    {
        // Array.Copy(buf, bufCopy);  // How to fill in a blank buf
        for (int i = 0; i < buf.Length; i++)
        {
            buf[i].Attributes = (short) ( (int) (ConsoleColor.White)  | ((int) (ConsoleColor.Black) << 4));
            buf[i].Char.AsciiChar = 32;
        }
    }

    public void WriteCon()
    {
        WriteCon(buf);
    }
    
    private void WriteCon(CharInfo[] ci)
    {
        bool b = WriteConsoleOutputW(h, ci, new Coord() { X = (short)_consoleWidth, Y = (short)_consoleHeight },
            new Coord() { X = 0, Y = 0 }, ref rect);
    }

    public int GetWidth() { return _consoleWidth; }

    public int GetHeight() { return _consoleHeight; }

    public void ClearConsole()
    {
        for(int i=0; i<buf.Length; i++)
        {
            buf[i].Attributes = 15;
            buf[i].Char.AsciiChar = 32;
        }

        WriteCon(buf);
    }

    public bool IsBlank(int x, int y)
    {
        return (buf[x + y* _consoleWidth].Char.AsciiChar == 32);
    }

    public char CharAt(Position p)
    {
        return (char) buf[p.XPos + p.YPos * _consoleWidth].Char.AsciiChar;
    }

    public void WriteAtBuf(string s, int x, int y, ConsoleColor fgc, ConsoleColor bgc)
    {
        Position aPos = new Position(x, y);
        WriteAtBuf(s, aPos, fgc, bgc);
    }

    private void WriteAtBuf(string s, Position aPos, ConsoleColor fgc, ConsoleColor bgc)
    {
            for (int i = 0; i < s.Length; i++)
            {
                buf[(aPos.YPos * _consoleWidth) + aPos.XPos + i].Attributes = (short) ( (int) fgc  | ((int) bgc << 4));
                buf[(aPos.YPos * _consoleWidth) + aPos.XPos + i].Char.AsciiChar = Encoding.ASCII.GetBytes(s)[i];
            }
    }
    
    
    private void WriteAt(string s, Position aPos, ConsoleColor fgc, ConsoleColor bgc)
    {
        lock (_lockWriting)  // Only one thread at a time here
        {
          WriteAtBuf(s, aPos, fgc, bgc);
          WriteCon(buf);
        }
    }

    public void WriteAtBuf(char c, Position aPos, ConsoleColor fgc, ConsoleColor bgc)
    {
        buf[(aPos.YPos * _consoleWidth) + aPos.XPos].Attributes = (short) ( (int) fgc  | ((int) bgc << 4));
        buf[(aPos.YPos * _consoleWidth) + aPos.XPos].Char.UnicodeChar = (ushort)c;
    }

    public void WriteAtBuf(char c, int x, int y, ConsoleColor fgc, ConsoleColor bgc)
    {
        buf[(y * _consoleWidth) + x].Attributes = (short) ( (int) fgc  | ((int) bgc << 4));
        buf[(y * _consoleWidth) + x].Char.UnicodeChar = (ushort)c;
    }

    public void WriteAt(char c, Position aPos, ConsoleColor fgc, ConsoleColor bgc)
    {
        lock (_lockWriting)  // Only one thread at a time here
        {
            WriteAtBuf(c, aPos, fgc, bgc);
            // buf[(aPos.YPos * _consoleWidth) + aPos.XPos].Attributes = (short) ( (int) fgc  | ((int) bgc << 4));
            // buf[(aPos.YPos * _consoleWidth) + aPos.XPos].Char.UnicodeChar = (ushort)c;
            WriteCon(buf);
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

    public void DrawFrame(int x, int y, int Width, int Height, ConsoleColor fgc, ConsoleColor bgc)
    {
        WriteAtBuf('╔', x, y, fgc, bgc);           
        WriteAtBuf('╚', x, y + Height, fgc, bgc);
        WriteAtBuf('╗', x + Width, y, fgc, bgc); 
        WriteAtBuf('╝', x + Width, y + Height, fgc, bgc);
        for (byte b = 1; b < Width; b++)  { WriteAtBuf('═', x + b, y, fgc, bgc); WriteAtBuf('═', x + b, y + Height, fgc, bgc); }
        for (byte b = 1; b < Height; b++) { WriteAtBuf('║', x, y + b, fgc, bgc); WriteAtBuf('║', x + Width, y + b, fgc, bgc);  }
        for(byte i=1; i<Width; i++)
            for(byte j=1; j<Height; j++)
                WriteAtBuf(Space,i+x,j+y, fgc, bgc);
    }

    public void RestoreConsole()  // ScreenChar[,] sc
    {
        Array.Copy(bufCopy, buf, buf.Length);
        WriteCon(buf);
    }

    public void BackupConsole() // ScreenChar[,]
    {
        Array.Copy(buf, bufCopy, buf.Length);
    }

    public ConsoleKey WaitForKey(ConsoleKey[] keys)
    {
        ConsoleKey k;
        do { k = Console.ReadKey(true).Key; } while (!keys.Contains(k));
        return k;
    }
    
    public ConsoleKey PopUpQuestion(int Width, int Height, String message, ConsoleKey[] ValidKeys)
    {  // TODO Needs work to stay in use
        BackupConsole();
        int x = (_consoleWidth / 2) - (Width / 2);
        int y = (_consoleHeight / 2) - (Height / 2);
        DrawFrame(x, y, Width, Height, ConsoleColor.Yellow, ConsoleColor.Green);
        WriteAt(message, x+2, y+2, ConsoleColor.Yellow, ConsoleColor.Green);
        ConsoleKey k = WaitForKey(ValidKeys);
        RestoreConsole();
        return k;
    }
}