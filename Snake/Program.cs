using System.ComponentModel;
using System.Globalization;

namespace Snake
{
    internal class Program
    {
        // TODO Add portals in the game
        // TODO Add multiplayer option
          // One or Two players can play the game
        // TODO Finish retry logic
          // Should be made by starting each round with a menu of settings, and a stat screen from previous round

        private int numSnakes = 2;
        MyConsole console;
        Board board;
        Snake snake;
        private Snake snake2;
        GameState state;
        Thread snakeThread;
        private Thread snakeThread2;
        Thread boardThread;
        private void EndGame()
        {
            snake.killSnake();
            if(numSnakes == 2) snake2.killSnake();
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
                snake.DrawInitialSnake(console, 10, 10);
                snakeThread = new Thread(snake.MoveSnake);
                boardThread = new Thread(board.AddTreats);
                snakeThread.Start();
                boardThread.Start();
                if (numSnakes == 2)
                {
                    snake2 = new Snake(console, board, state);
                    snakeThread2 = new Thread(snake2.MoveSnake);
                    snake2.DrawInitialSnake(console, 10, 15);
                    snakeThread2.Start();
                }
                do
                {
                    if (Console.KeyAvailable)
                    {
                        switch (Console.ReadKey(true).Key) // true causes the console NOT to echo the key pressed onto the console
                        {
                            case ConsoleKey.Q:          { state.GameOver = true;
                                state.CauseOfDeath = "aborted by user!"; snake.killSnake(); break; }
                            case ConsoleKey.Spacebar:   { break; } // PauseGame
                            case ConsoleKey.UpArrow:    { snake.SetDirection(Snake.Directions.Up); break; }
                            case ConsoleKey.DownArrow:  { snake.SetDirection(Snake.Directions.Down); break; }
                            case ConsoleKey.RightArrow: { snake.SetDirection(Snake.Directions.Right); break; }
                            case ConsoleKey.LeftArrow:  { snake.SetDirection(Snake.Directions.Left); break; }
                            case ConsoleKey.A:          { snake2.SetDirection(Snake.Directions.Left); break; }
                            case ConsoleKey.D:          { snake2.SetDirection(Snake.Directions.Right); break; }
                            case ConsoleKey.W:          { snake2.SetDirection(Snake.Directions.Up); break; }
                            case ConsoleKey.S:          { snake2.SetDirection(Snake.Directions.Down); break; }
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
            if (numSnakes == 2) snakeThread2.Join();
            snakeThread.Join();
            boardThread.Join();
        }

        public static void Main(string[] args) 
        {
            new Program().go(); 
        }
    }
}