using System;

namespace Snake
{
    internal class Program
    {
        MyConsole console;
        Board board;
        Snake snake;
        GameState state;
        public void EndGame()
        {
            snake.killSnake();
            console.CloseConsole();
            console.WriteAt("* GAME OVER *", 10, 10);
            console.WriteAt("* Total Snake Length :" + state.TotalSnakeLength, 10, 11);
            console.WriteAt(state.CauseOfDeath, 10, 12);
        }

        public void GameStatus()
        {
            console.WriteAt(" Snake Length :" + snake.SnakeLength() + " ", 5, 0);
            console.WriteAt(" Snake Delay  :" + state.SnakeDelay + " ", 25, 0);
            console.WriteAt(" Treat Delay  :" + state.TreatDelay + " ", 50, 0);
        }

        public void go()
        {
            console = new MyConsole();
            console.InitializeConsole();
            state = new GameState();
            board = new Board(console, state);        // Creates a new board, and sets it up with a number of treats
            snake = new Snake(console, board, state);
            // Setup Board and snake
            Thread snakeThread = new Thread(snake.MoveSnake);
            snakeThread.Start();
            Thread boardThread = new Thread(board.AddTreats);
            boardThread.Start();
            do
            {
                if (Console.KeyAvailable)
                {
                    switch (Console.ReadKey(true).Key)  // true causes the console NOT to echo the key pressed onto the console
                    {
                        case ConsoleKey.Q:          { state.GameOver = true; break; }
                        case ConsoleKey.Spacebar:   { break; } // PauseGame
                        case ConsoleKey.UpArrow:    { snake.SetDirection(Snake.Directions.Up);    break; }
                        case ConsoleKey.DownArrow:  { snake.SetDirection(Snake.Directions.Down);  break; }
                        case ConsoleKey.RightArrow: { snake.SetDirection(Snake.Directions.Right); break; }
                        case ConsoleKey.LeftArrow:  { snake.SetDirection(Snake.Directions.Left);  break; }
                        case ConsoleKey.R:          { break; } // ??
                    }
                }
                state.TotalSnakeLength = snake.SnakeLength();
                GameStatus();
                Thread.Sleep(50);
            } while (!state.GameOver);
            EndGame();
            snakeThread.Join();
            boardThread.Join();
        }

        static void Main(string[] args) 
        {
            new Program().go(); 
        }
    }
}