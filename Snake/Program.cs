using System;

namespace Snake
{
    internal class Program
    {
        MyConsole c;
        public void go()
        {
            c = new MyConsole();
            c.InitializeConsole();
            Board b = new Board(c);
            Snake s = new Snake(c,b);
            bool weDied = false;

            Thread t = new Thread(s.MoveSnake);
            t.Start();

            b.AddTreat();
            b.AddTreat();
            b.AddTreat();
            b.AddTreat();
            b.AddTreat();

            var endProgram = false;
            ConsoleKey k;

            DateTime timeStamp;
            TimeSpan timeElapsed;
            timeStamp = DateTime.Now;
            // Position p = new Position();

            do
            {
                timeElapsed = DateTime.Now - timeStamp;
                //if (timeElapsed.Milliseconds > 200)
                //{
                //    c.WriteAt("Snake Length :" , 1, 1);  // TODO Add snake length update on console
                //    c.WriteAt("Console frozen     :" + c.FreezeConsole + " ", 1, 2);
                //    c.WriteAt("Asterisks hidden   :" + c.HideAsterisks + " ", 1, 3);
                //    timeStamp = DateTime.Now;
                //}
                endProgram = weDied;
                if (Console.KeyAvailable)
                {
                    k = Console.ReadKey(true).Key;  // true causes the console NOT to echo the key pressed onto the console
                    switch (k)
                    {
                        case ConsoleKey.Q:          { endProgram = true; break; }
                        case ConsoleKey.Spacebar:   { break; } // PauseGame
                        case ConsoleKey.UpArrow:    { s.SetDirection(Snake.Directions.Up);    break; } // TODO Snake direction up
                        case ConsoleKey.DownArrow:  { s.SetDirection(Snake.Directions.Down);  break; } // TODO Snake direction down
                        case ConsoleKey.RightArrow: { s.SetDirection(Snake.Directions.Right); break; } // TODO Snake Direction Right
                        case ConsoleKey.LeftArrow:  { s.SetDirection(Snake.Directions.Left);  break; } // TODO Snake Direction Left
                        case ConsoleKey.R:          { break; } // ??
                    }
                }
            } while (!endProgram);
            if (weDied)
            {
                Console.WriteLine("Arrgghh!");
                Console.ReadLine();
            }
            c.CloseConsole();


        }

        static void Main(string[] args) 
        {
            new Program().go(); 
        }
    }
}