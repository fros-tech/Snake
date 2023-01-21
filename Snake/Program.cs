using System.ComponentModel;
using System.Globalization;

namespace Snake
{
    internal class Program
    {
        MyConsole console;
        Board board;
        Snake snake;
        GameState state;
        Thread snakeThread;
        Thread boardThread;
        private void EndGame()
        {
            snake.killSnake();
            console.CloseConsole();
            console.WriteAt("* GAME OVER *", 10, 10);
            console.WriteAt("* Total Snake Length :" + state.TotalSnakeLength, 10, 11);
            console.WriteAt(state.CauseOfDeath, 10, 12);
        }

        public bool GoAgain()
        {
            console.WriteAt("Skal du prøve igen? J/N :", 10, 14);
            while (!Console.KeyAvailable) {}
            ConsoleKeyInfo k = Console.ReadKey(true);
            return (k.Key == ConsoleKey.J);
        }
        
        private void GameStatus()
        {
            console.WriteAt(" Snake Length :" + snake.SnakeLength() + " ", 5, 0);
            console.WriteAt(" Snake Delay  :" + state.SnakeDelay + " ", 25, 0);
            console.WriteAt(" Treat Delay  :" + state.TreatDelay + " ", 50, 0);
        }

        private void go()
        {
            console = new MyConsole();
            console.InitializeConsole();
            state = new GameState();
            do
            {
                board = new Board(console, state);        // Creates a new board, and sets it up with a number of treats
                snake = new Snake(console, board, state);
                snakeThread = new Thread(snake.MoveSnake);
                boardThread = new Thread(board.AddTreats);
                snakeThread.Start();
                boardThread.Start();
                do
                {
                    if (Console.KeyAvailable)
                    {
                        switch (Console.ReadKey(true).Key) // true causes the console NOT to echo the key pressed onto the console
                        {
                            case ConsoleKey.Q:          { state.GameOver = true; break; }
                            case ConsoleKey.Spacebar:   { break; } // PauseGame
                            case ConsoleKey.UpArrow:    { snake.SetDirection(Snake.Directions.Up); break; }
                            case ConsoleKey.DownArrow:  { snake.SetDirection(Snake.Directions.Down); break; }
                            case ConsoleKey.RightArrow: { snake.SetDirection(Snake.Directions.Right); break; }
                            case ConsoleKey.LeftArrow:  { snake.SetDirection(Snake.Directions.Left); break; }
                            case ConsoleKey.R:          { break; } // ??
                        }
                    }
                    state.TotalSnakeLength = snake.SnakeLength();
                    GameStatus();
                    Thread.Sleep(50);
                } while (!state.GameOver);
                EndGame();  // TODO add possibility to have another go, or exit the program
                state.EndProgram = GoAgain();
            } while (!state.EndProgram);
            snakeThread.Join();
            boardThread.Join();
        }

        public static void Main(string[] args) 
        {
            new Program().go(); 
        }
    }
}