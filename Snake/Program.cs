
namespace Snake
{
    internal class Program
    {
        // TODO Add portals in the game
          // Will require the collision code in the snake to be rewritten
        // TODO Finish retry logic
          // Should be made by starting each round with a menu of settings, and a stat screen from previous round
        // TODO Add animation to portals and treats when they appear
        // TODO add lifetime for treats, so that they disappear after a given time (with animation)

        private int _numSnakes = 1;  // Initial expected number of snakes
        MyConsole console;
        Board board;
        private List<Snake> _snakes = new List<Snake>();
        // private List<Thread> _snakeThreads = new List<Thread>();  // Are now handled in the snake
        
        GameState state;
        Thread boardThread;
        private void EndGame()
        {
            foreach (Snake s in _snakes)
                s.KillSnake();
            console.CloseConsole();
            console.WriteAt("* GAME OVER *", 10, 10);
            console.WriteAt("* Total Snake Length :" + state.TotalSnakeLength, 10, 11);
            // TODO Find winner and announce
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
            // console.WriteAt(" Snake Length :" + snake.SnakeLength() + " ", 5, 0); // TODO show length of multiple snakes color code the longest
            console.WriteAt(" Snake Delay  :" + state.SnakeDelay + " ", 25, 0);
            console.WriteAt(" Treat Delay  :" + state.TreatDelay + " ", 50, 0);
        }

        private void Go()
        {
            console = new MyConsole();
            state = new GameState();
            board = new Board(console, state);          // Create a new board and a thread to add treats
            boardThread = new Thread(board.AddTreats);
            for (int i = 0; i < _numSnakes; i++)        // Create the snakes and threads to move them
            {
                Snake s = new Snake(console, board, state, i);
                _snakes.Add(s);
            }
            console.InitializeConsole();
            do
            {
                state.Reset();
                console.ClearConsole();
                board.SetupBoard();
                boardThread.Start();
                foreach (Snake s in _snakes) { s.DrawInitialSnake();} // snake itself knows where to draw initially
                state.GamePaused = false;
                do
                {
                    if (Console.KeyAvailable)
                    {
                        ConsoleKey keyPressed = Console.ReadKey(true).Key;
                        foreach(Snake s in _snakes)
                            s.SetDirection(keyPressed);
                        switch (keyPressed)     // true causes the console NOT to echo the key pressed onto the console
                        {
                            case ConsoleKey.Q:          { state.GameOver = true; state.CauseOfDeath = "aborted by user!"; foreach(Snake s in _snakes) s.KillSnake(); break; }
                            case ConsoleKey.Spacebar:   { state.TogglePaused(); break; } // PauseGame
                            case ConsoleKey.R:          { break; } // ??
                        }
                    }
                    GameStatus();
                    Thread.Sleep(50);
                } while (!state.GameOver);
                EndGame();  // TODO add possibility to have another go, or exit the program
                state.EndProgram = !GoAgain();
            } while (!state.EndProgram);
            foreach(Snake s in _snakes) s.KillSnake();
            boardThread.Join();
        }

        public static void Main(string[] args) 
        {
            new Program().Go(); 
        }
    }
}