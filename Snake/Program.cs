
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

        private int _numSnakes = 2;  // Initial expected number of snakes
        MyConsole console;
        Board board;
        private List<Snake> _snakes = new List<Snake>();
        // private List<Thread> _snakeThreads = new List<Thread>();  // Are now handled in the snake
        
        GameState state;
        Thread boardThread;
        private void EndGame()
        {
            //foreach (Snake s in _snakes)
            //    s.KillSnake();
            //console.CloseConsole();
            console.WriteAt("* GAME OVER *", 10, 10);
            for (int i=0; i < _snakes.Count; i++)
            {
                console.WriteAt(" Snake #"+i+ ": "+_snakes[i].SnakeLength(),10, 10+i);
            }
            // TODO Find winner and announce
            console.WriteAt(state.CauseOfDeath, 10, 15);
            console.WriteAt("Winner is snake #: "+state.MaxSnakeLength+", Length: "+state.MaxSnakeLength, 10, 16);
        }

        public bool GoAgain()
        {
            console.WriteAt("Skal du prøve igen? J/N :", 10, 14);
            ConsoleKeyInfo k;
            do
            {
                while (!Console.KeyAvailable) { }
                k = Console.ReadKey(true);
            } while (k.Key != ConsoleKey.J && k.Key != ConsoleKey.N);
            return (k.Key == ConsoleKey.J);
        }
        
        private void GameStatus()
        {
            console.WriteAt(" Snake Delay  : " + state.SnakeDelay + " ", 5, 0);
            console.WriteAt(" Treat Delay  : " + state.TreatDelay + " ", 30, 0);
            for (int i=0; i < _snakes.Count; i++)
            {
                console.WriteAt(" Snake #"+i+ ": "+_snakes[i].SnakeLength(),55+(i*15), 0);
            }
        }

        public void SetupGame()
        {
            console = new MyConsole();
            state = new GameState();
            board = new Board(console, state);          // Create a new board and a thread to add treats
            boardThread = new Thread(board.AddTreats);
            console.InitializeConsole();
            for (int i = 0; i < _numSnakes; i++)        // Create the snakes and threads to move them
            {
                Snake s = new Snake(console, board, state, i);
                _snakes.Add(s);
            }
            boardThread.Start();
        }

        private void ResetGame()
        {
            state.Reset();
            console.ClearConsole();
            board.SetupBoard();
            foreach (Snake s in _snakes) { s.DrawInitialSnake(); s.Activate();} // snake itself knows where to draw initially
            board.ActivateTreats();
        }

        private void Go()
        {
            SetupGame();
            do                                          // As long as user(s) wants to have a try
            {
                ResetGame();
                do                                      // Run a snake game
                {
                    if (Console.KeyAvailable)
                    {
                        ConsoleKey keyPressed = Console.ReadKey(true).Key;
                        foreach(Snake s in _snakes)
                            s.SetDirection(keyPressed);
                        switch (keyPressed)             // true causes the console NOT to echo the key pressed onto the console
                        {
                            case ConsoleKey.Q:          { state.GameOver = true; state.CauseOfDeath = "aborted by user!"; foreach(Snake s in _snakes) s.KillSnake(); break; }
                            case ConsoleKey.Spacebar:   { state.TogglePaused(); break; } // PauseGame
                            case ConsoleKey.R:          { break; } // ??
                        }
                    }
                    GameStatus();                       // Update gamestats on the console
                    Thread.Sleep(50);
                } while (!state.GameOver);
                EndGame();                              // Show final gamestats
                state.EndProgram = !GoAgain();          // Check if we are going to have another try at it
                board.DeActivateTreats();
                foreach(Snake s in _snakes) s.DeActivate();
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