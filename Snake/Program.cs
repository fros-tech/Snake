using System;

namespace Snake
{
    internal class Program
    {
        MyConsole console;
        public void go()
        {
            console = new MyConsole();
            console.InitializeConsole();
            GameState state = new GameState();
            Board board = new Board(console, state);        // Creates a new board, and sets it up with a number of treats
            Snake snake = new Snake(console, board, state);

            // Setup Board and snake
            Thread snakeThread = new Thread(snake.MoveSnake);
            snakeThread.Start();
            Thread boardThread = new Thread(board.AddTreats);
            boardThread.Start();

            ConsoleKey k;
            do
            {
                if (Console.KeyAvailable)
                {
                    switch (Console.ReadKey(true).Key)  // true causes the console NOT to echo the key pressed onto the console
                    {
                        case ConsoleKey.Q:          { state.GameOver = true; break; }
                        case ConsoleKey.Spacebar:   { break; } // PauseGame
                        case ConsoleKey.UpArrow:    { snake.SetDirection(Snake.Directions.Up);    break; } // TODO Snake direction up
                        case ConsoleKey.DownArrow:  { snake.SetDirection(Snake.Directions.Down);  break; } // TODO Snake direction down
                        case ConsoleKey.RightArrow: { snake.SetDirection(Snake.Directions.Right); break; } // TODO Snake Direction Right
                        case ConsoleKey.LeftArrow:  { snake.SetDirection(Snake.Directions.Left);  break; } // TODO Snake Direction Left
                        case ConsoleKey.R:          { break; } // ??
                    }
                }
                console.WriteAt(" Snake Length :" + snake.SnakeLength() + " ", 5, 0);  // TODO Add snake length update on console
                console.WriteAt(" Snake Delay  :" + state.SnakeDelay + " ", 25, 0);
                console.WriteAt(" Treat Delay  :" + state.TreatDelay + " ", 50, 0);
            } while (!state.GameOver);
            snake.killSnake();
            console.CloseConsole();
            console.WriteAt("* GAME OVER *", 10, 10);
            console.WriteAt("* Total Snake Length :"+snake.SnakeLength(), 10, 11);
            console.WriteAt(state.CauseOfDeath, 10, 12);
            snakeThread.Join();
            boardThread.Join();
        }

        static void Main(string[] args) 
        {
            new Program().go(); 
        }
    }
}