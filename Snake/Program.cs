
namespace Snake
{
    internal class Program
    {
        // TODO Add portals in the game
          // Will require the collision code in the snake to be rewritten
        // TODO Add animation to portals and treats when they appear

        private readonly int _numSnakes = 2;  // Initial expected number of snakes
        private MyConsole console;
        private Board board;
        private readonly List<Snake> _snakes = new List<Snake>();
        
        private GameState state;
        private Thread boardThread;
        
        private void ShowEndGameStats()
        {
            console.WriteAt("* GAME OVER *", 10, 10);
            for (int i=0; i < _snakes.Count; i++) { console.WriteAt(" Snake #"+i+ ": "+_snakes[i].SnakeLength(),10, 11+i); }
            console.WriteAt(state.CauseOfDeath, 10, 15);
            console.WriteAt("Winner is snake #: "+state.MaxSnakeLength+", Length: "+state.MaxSnakeLength, 10, 16);
        }

        private bool GoAgain()
        {
            console.WriteAt("Want another try ? J/N :", 10, 14);
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
            for (int i=0; i < _snakes.Count; i++) { console.WriteAt(" Snake #"+i+ ": "+_snakes[i].SnakeLength()+" ",55+(i*15), 0); }
        }

        private void SetupGame()
        {
            console = MyConsole.GetInstance();
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
            board.ResetBoard();
            ResetSnakes();
            ActivateSnakes();
            board.ActivateTreats();
        }

        public void ResetSnakes() { foreach (Snake s in _snakes) { s.ResetSnake();} }
        public void DeActivateSnakes() { foreach (Snake s in _snakes) { s.DeActivate();} }
        public void ActivateSnakes() { foreach (Snake s in _snakes) { s.Activate();} }
        public void KillSnakes() { foreach (Snake s in _snakes) { s.KillSnake();}}
        
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
                            case ConsoleKey.Q:          { state.GameOver = true; state.CauseOfDeath = "aborted by user!"; break; }
                            case ConsoleKey.Spacebar:   { state.TogglePaused(); break; } // PauseGame
                        }
                    }
                    GameStatus();                       // Update gamestats on the console
                    Thread.Sleep(50);    // Give the CPU a break
                } while (!state.GameOver);
                ShowEndGameStats();                      // Show final gamestats
                board.DeActivateTreats();
                DeActivateSnakes();
                state.EndProgram = !GoAgain();           // Check if we are going to have another try at it
            } while (!state.EndProgram);
            KillSnakes();
            boardThread.Join();
        }

        public static void Main(string[] args) 
        {
            new Program().Go(); 
        }
    }
}