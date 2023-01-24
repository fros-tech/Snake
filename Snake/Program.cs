using System.ComponentModel;
using System.Globalization;

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

        private int numSnakes = 2;  // Initial expected number of snakes
        MyConsole console;
        Board board;
        private List<Snake> Snakes = new List<Snake>();
        private List<Thread> snakeThreads = new List<Thread>();
        
        GameState state;
        Thread boardThread;
        private void EndGame()
        {
            foreach (Snake s in Snakes)
                s.killSnake();
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

        private void go()
        {
            console = new MyConsole();
            console.InitializeConsole();
            state = new GameState();
            board = new Board(console, state);        // Creates a new board, and sets it up with a number of treats
            boardThread = new Thread(board.AddTreats);
            for (int i = 0; i < numSnakes; i++)
            {
                Snake s = new Snake(console, board, state, i);
                Thread t = new Thread(s.MoveSnake);
                s.DrawInitialSnake();  // snake itself knows where to draw initially
                Snakes.Add(s);
                snakeThreads.Add(t);
            }
            do
            {
                foreach (Snake s in Snakes) { s.DrawInitialSnake(); } // snake itself knows where to draw initially
                foreach (Thread t in snakeThreads) { t.Start();}
                boardThread.Start();
                do
                {
                    if (Console.KeyAvailable)
                    {
                        ConsoleKey keyPressed = Console.ReadKey(true).Key;
                        foreach(Snake s in Snakes)
                            s.SetDirection(keyPressed);
                        switch (keyPressed)     // true causes the console NOT to echo the key pressed onto the console
                        {
                            case ConsoleKey.Q:          { state.GameOver = true; state.CauseOfDeath = "aborted by user!"; foreach(Snake s in Snakes) s.killSnake(); break; }
                            case ConsoleKey.Spacebar:   { break; } // PauseGame
                            case ConsoleKey.R:          { break; } // ??
                        }
                    }
                    GameStatus();
                    Thread.Sleep(50);
                } while (!state.GameOver);
                EndGame();  // TODO add possibility to have another go, or exit the program
                state.EndProgram = GoAgain();
            } while (!state.EndProgram);
            foreach(Thread t in snakeThreads) t.Join();
            boardThread.Join();
        }

        public static void Main(string[] args) 
        {
            new Program().go(); 
        }
    }
}