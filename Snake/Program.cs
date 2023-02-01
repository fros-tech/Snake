
using Microsoft.VisualBasic;

namespace Snake
{
    internal class Program
    {
        // TODO Add animation to portals and treats when they appear
        // TODO Refine Game Over/retry? sequence. Both UX and Post mortem

        private readonly int _numSnakes = 1;  // Initial expected number of snakes
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
            return console.PopUpQuestion(20, 4, "Spil igen (J/N):", "jJnN") == 'j';
        }
        
        private void GameStatus()
        {
            console.WriteAt(" Snake Delay  : " + state.SnakeDelay + " ", 5, console.GetHeight()-1);
            console.WriteAt(" Treat Delay  : " + state.TreatDelay + " ", 30, console.GetHeight()-1);
            for (int i=0; i < _snakes.Count; i++) { console.WriteAt(" Snake #"+i+ ": "+_snakes[i].SnakeLength()+" ",55+(i*15), console.GetHeight()-1); }
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
            board.ActivateBoard();
        }

        private void ResetSnakes() { foreach (Snake s in _snakes) { s.ResetSnake();} }
        private void DeActivateSnakes() { foreach (Snake s in _snakes) { s.DeActivate();} }
        private void ActivateSnakes() { foreach (Snake s in _snakes) { s.Activate();} }
        private void KillSnakes() { foreach (Snake s in _snakes) { s.KillSnake();}}
        
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
                board.DeActivateBoard();
                DeActivateSnakes();
                state.EndProgram = !GoAgain();           // Check if we are going to have another try at it
            } while (!state.EndProgram);
            KillSnakes();
            boardThread.Join();
        }
        
        public void PlayAround()
        {
            MyConsole c = MyConsole.GetInstance();
            c.InitializeConsole();
            //c.DrawFrame(10, 10, 35, 35);
            char ch = c.PopUpQuestion(20, 10, "Nyt Spil J/N:", "jJnN");
        }

        public static void Main(string[] args)
        {
            new Program().Go(); 
        }
    }
}