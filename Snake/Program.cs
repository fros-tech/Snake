using System;

namespace Snake
{
    internal class Program
    {
        MyConsole c;
        public bool endProgram = false;
        public void go()
        {
            c = new MyConsole();
            c.InitializeConsole();
            Board b = new Board(c);
            Snake s = new Snake(c,b);
            bool weDied = false;

            Thread snakeThread = new Thread(s.MoveSnake);
            snakeThread.Start();
            Thread boardThread = new Thread(b.AddTreats);
            boardThread.Start();

            b.AddTreat();
            b.AddTreat();
            b.AddTreat();
            b.AddTreat();
            b.AddTreat();

            ConsoleKey k;

            DateTime timeStamp;
            TimeSpan timeElapsed;
            timeStamp = DateTime.Now;
            // Position p = new Position();

            do
            {
                timeElapsed = DateTime.Now - timeStamp;
                if (timeElapsed.Milliseconds > 500)
                {
                  c.WriteAt(" Snake Length :"+s.SnakeLength()+" " , 5, 0);  // TODO Add snake length update on console
                  timeStamp = DateTime.Now;
                }
                endProgram = weDied;
                if (Console.KeyAvailable)
                {
                    switch (Console.ReadKey(true).Key)  // true causes the console NOT to echo the key pressed onto the console
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
            s.killSnake();
            c.CloseConsole();
            if (weDied)
                Console.WriteLine("Arrgghh!");
            snakeThread.Join();
            boardThread.Join();
        }

        static void Main(string[] args) 
        {
            new Program().go(); 
        }
    }
}