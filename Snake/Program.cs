using System;

namespace Snake
{
    internal class Program
    {
        MyConsole c;
        public enum Directions
        {
            Left = 0, Right = 1, Up = 2, Down = 3
        }

        public void go()
        {
            c = new MyConsole();
            c.InitializeConsole();
            Snake s = new Snake(c);
            Board b = new Board(c);

            var endProgram = false;
            ConsoleKey k;

            DateTime timeStamp;
            TimeSpan timeElapsed;
            timeStamp = DateTime.Now;
            // Position p = new Position();

            do
            {
                timeElapsed = DateTime.Now - timeStamp;
                if (timeElapsed.Milliseconds > 200)
                {
                    c.WriteAt("Snake Length :" , 1, 1);  // TODO Add snake length update on console
                    c.WriteAt("Console frozen     :" + c.FreezeConsole + " ", 1, 2);
                    c.WriteAt("Asterisks hidden   :" + c.HideAsterisks + " ", 1, 3);
                    timeStamp = DateTime.Now;
                }
                if (Console.KeyAvailable)
                {
                    k = Console.ReadKey(true).Key;  // true causes the console NOT to echo the key pressed onto the console
                    switch (k)
                    {
                        case ConsoleKey.Q:          { endProgram = true; break; }
                        case ConsoleKey.Spacebar:   { break; } // PauseGame
                        case ConsoleKey.UpArrow:    { s.SetDirection(Directions.Up);    break; } // TODO Snake direction up
                        case ConsoleKey.DownArrow:  { s.SetDirection(Directions.Down);  break; } // TODO Snake direction down
                        case ConsoleKey.RightArrow: { s.SetDirection(Directions.Right); break; } // TODO Snake Direction Right
                        case ConsoleKey.LeftArrow:  { s.SetDirection(Directions.Left);  break; } // TODO Snake Direction Left
                        case ConsoleKey.R:          { break; } // ??
                    }
                }
            } while (!endProgram);
            c.CloseConsole();


        }

        static void Main(string[] args) 
        {
            new Program().go(); 
        }
    }
}