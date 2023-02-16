
namespace Snake
{
    internal class Program
    {
        // TODO Add animation to portals and treats when they appear
        // TODO Refine Game Over/retry? sequence. Both UX and Post mortem

        private static int _numSnakes = 1;  // Default number of snakes. Maximum is 3
        private MyConsole console;
        private Board board;
        private readonly List<Snake> _snakes = new List<Snake>();
        
        private GameState state;
        private Thread boardThread;
        
        private bool GoAgain()
        {  // TODO Needs work
            console.BackupConsole();
            console.WriteAtBuf("╔════════════════════════════════════════════════════════════════════════════════════════╗", 5, 5,  ConsoleColor.Yellow, ConsoleColor.DarkGreen);
            console.WriteAtBuf("╟──────────────────────────────────────┤ Stats ├─────────────────────────────────────────╢", 5, 6,  ConsoleColor.Yellow, ConsoleColor.DarkGreen);
            console.WriteAtBuf("║--  GAME ENDED BECAUSE: "+state.CauseOfDeath+" --+",5, 7,  ConsoleColor.Yellow, ConsoleColor.DarkGreen);
            console.WriteAtBuf("║--  Snake lengths                                                                     --║", 5, 8,  ConsoleColor.Yellow, ConsoleColor.DarkGreen);
            for (int i=0; i < _snakes.Count; i++) { console.WriteAtBuf("+--   Snake# : "+i+",  "+_snakes[i].SnakeLength()+"                                                                   --║",5, 9+i, ConsoleColor.Yellow, ConsoleColor.DarkGreen); }
            console.WriteAtBuf("║--                                                                                    --║", 5, 12, ConsoleColor.Yellow, ConsoleColor.DarkGreen);
            console.WriteAtBuf("║--  Winner is snake #: "+state.LongestSnake +"                                                              --║", 5, 11, ConsoleColor.Yellow, ConsoleColor.Green);
            console.WriteAtBuf("╚════════════════════════════════════════════════════════════════════════════════════════╝", 5, 13, ConsoleColor.Yellow, ConsoleColor.DarkGreen);
            console.WriteCon();
            ConsoleKey k = console.WaitForKey(new [] {ConsoleKey.J, ConsoleKey.N});
            console.RestoreConsole();
            return (k == ConsoleKey.J);
        }


        private void ShowHelp()
        {
            bool _paused = state.GamePaused;
            Thread.Sleep(400);  // Need threads to discover the new state
            state.GamePaused = true;
            console.BackupConsole();
            console.WriteAtBuf("+------------------------------------------------------------+", 5, 5,  ConsoleColor.Yellow, ConsoleColor.DarkGreen);
            console.WriteAtBuf("+------------------------- Snake V2.0 -----------------------+", 5, 6,  ConsoleColor.Yellow, ConsoleColor.DarkGreen);
            console.WriteAtBuf("+--  CONTROLS:                               TREATS:       --+", 5, 7,  ConsoleColor.Yellow, ConsoleColor.DarkGreen);
            console.WriteAtBuf("+--  Snake #0   Use 'A', 'W', 'S', 'D'       '~' 1 point   --+", 5, 8,  ConsoleColor.Yellow, ConsoleColor.DarkGreen);
            console.WriteAtBuf("+--  Snake #1   Use Arrow keys               '$' 2 points  --+", 5, 9,  ConsoleColor.Yellow, ConsoleColor.DarkGreen);
            console.WriteAtBuf("+--  Snake #2   Use Numpad keys              '£' 3 points  --+", 5, 10, ConsoleColor.Yellow, ConsoleColor.DarkGreen);
            console.WriteAtBuf("+--  Press 'Q' or <Esc> to quit game         '#' 5 points  --+", 5, 11, ConsoleColor.Yellow, ConsoleColor.DarkGreen);
            console.WriteAtBuf("+--                                                        --+", 5, 12, ConsoleColor.Yellow, ConsoleColor.DarkGreen);
            console.WriteAtBuf("+--  PORTALS: '@'                                          --+", 5, 13, ConsoleColor.Yellow, ConsoleColor.DarkGreen);
            console.WriteAtBuf("+------------------------------------------------------------+", 5, 14, ConsoleColor.Yellow, ConsoleColor.DarkGreen);
            console.WriteAtBuf("+----------------- <Esc> to leave help screen ---------------+", 5, 15, ConsoleColor.Yellow, ConsoleColor.DarkGreen);
            console.WriteCon();
            ConsoleKey k = console.WaitForKey(new [] {ConsoleKey.Escape});
            state.GamePaused = _paused;
            console.RestoreConsole();
        }
        
        private void GameStatus()
        {
            int ch = console.GetHeight()-1;
            console.WriteAt(" Snake Delay  : " + state.SnakeDelay + " ", 2, ch);
            console.WriteAt(" Treat Delay  : " + state.TreatDelay + " ", 27, ch);
            for (int i=0; i < _snakes.Count; i++) { console.WriteAt(" Snake #"+i+ ": "+_snakes[i].SnakeLength()+" ",52+(i*15), ch); }
            console.WriteAt("Fluke :"+state.Fluke, 70, ch);
        }

        private void SetupGame()
        {
            console = MyConsole.GetInstance();
            state = new GameState();
            board = new Board(console, state);          // Create a new board and a thread to add treats
            boardThread = new Thread(board.AddTreats);
            boardThread.IsBackground = true;
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
            console.WriteAt(" H : Help    <Esc>/Q : Quit ",  console.GetWidth()-31, console.GetHeight()-1);
        }

        private void ResetSnakes() { foreach (Snake s in _snakes) { s.ResetSnake();} }
        private void DeActivateSnakes() { foreach (Snake s in _snakes) { s.DeActivate();} Thread.Sleep(100); }
        private void ActivateSnakes() { foreach (Snake s in _snakes) { s.Activate();} }
        
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
                        foreach(Snake s in _snakes)     // Update the snakes directions
                            s.SetDirection(keyPressed);
                        switch (keyPressed)             // true causes the console NOT to echo the key pressed onto the console
                        {
                            case ConsoleKey.Q:
                            case ConsoleKey.Escape:     { state.GameOver = true; state.CauseOfDeath = "aborted by user!"; break; }
                            case ConsoleKey.Spacebar:   { state.TogglePaused(); break; } // PauseGame
                            case ConsoleKey.H:          { ShowHelp(); break; }
                        }
                    }
                    board.Fluke();
                    GameStatus();                       // Update game stats on the console
                    Thread.Sleep(50);    // Give the CPU a break
                } while (!state.GameOver);
                board.DeActivateBoard();
                DeActivateSnakes();
                state.EndProgram = !GoAgain();           // Check if we are going to have another go at it
            } while (!state.EndProgram);
            Console.Clear();
        }
        
        public static void Main(string[] args)
        {
            if(args.Length > 0)
            {
                int parm;
                if(int.TryParse(args[0], out parm))
                  if (parm > 0 && parm < 4) _numSnakes = parm;
                
            }
            new Program().Go(); 
        }
    }
}