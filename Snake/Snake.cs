using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Snake
{
    internal class Snake
    {
        // This class contains code that draws a snake on the console.
        // It also contains a method that will let the snake move
        // according to a defined direction
        // The snake detects collisions with console 'objects'
        // These objects can be food, which cause the snake to increase
        // in length or obstacles which cause the game to end.
        // The snake will start looking like this: '******O'
        private static ConsoleKey[,] KeySet = {{ConsoleKey.W,       ConsoleKey.S,         ConsoleKey.A,         ConsoleKey.D },
                                               {ConsoleKey.UpArrow, ConsoleKey.DownArrow, ConsoleKey.LeftArrow, ConsoleKey.RightArrow},
                                               {ConsoleKey.NumPad8, ConsoleKey.NumPad5,   ConsoleKey.NumPad4,   ConsoleKey.NumPad6}
        };

        public enum Directions { Left = 0, Right = 1, Up = 2, Down = 3 }

        const char snakeHeadChar = 'O';
        const char snakeBodyChar = '*';
        private const ConsoleColor snakeHeadfgColor = ConsoleColor.Red;
        private const ConsoleColor snakeBodyfgColor = ConsoleColor.Magenta;
        private ConsoleKey[] snakesKeys;
        bool SnakeAlive = true;
        private int snakeID; // indicates which number snake it is. Determines initiating coordinates, and keyboard keys
                             // used to control the snake
        const byte initialSnakeLength = 7;
        const int minSnakeDelay = 80;
        Position? nextPos;
        GameState state;
        MyConsole console;
        Directions dir = Directions.Right;
        private int linksToBeAdded = 0;
        Board board;
        List<Position> positions;
        
        public Snake(MyConsole console, Board board, GameState state, int snakeID)
        {
            this.console = console;
            this.board = board;
            positions = new List<Position>();
            // DrawInitialSnake(console);
            this.state = state;
            this.snakeID = snakeID;
        }

        public void killSnake()
        {
            SnakeAlive = false;
        }

        private void RemoveSnake()
        {
            foreach(Position p in positions)
                console.WriteAt(' ', p);
            positions = new List<Position>();  // Leave any previous positions to the garbage collector
        }

        public void DrawInitialSnake() 
        {
            // Needs to do some finagling and fiddling to place more snakes on the board
            // Places snakes relative to each corner and with a direction towards the middle of the board
            // to give the player a chance to adjust before the snake slams into the border or
            // another snake. Apologies for code that may offend the eye.
            int initXPos, initYPos;
            switch (snakeID)
            {
                case 0:    // Starts in upper left corner
                {
                    initXPos = 5;
                    initYPos = 5;
                    dir = Directions.Right;
                    for (int i = 0; i < initialSnakeLength; i++) { positions.Add(new Position(initXPos + i, initYPos)); }
                    break;
                }
                case 1:    // Starts in upper right corner
                {
                    initXPos = console.getWidth()-initialSnakeLength-5;
                    initYPos = 5;
                    dir = Directions.Left;
                    for (int i = 0; i < initialSnakeLength; i++) { positions.Add(new Position(initXPos - i, initYPos)); }
                    break;
                }
                case 2:    // Starts in lower right corner
                {
                    initXPos = console.getWidth()-initialSnakeLength-5;
                    initYPos = console.getHeight()-5;
                    dir = Directions.Left;
                    for (int i = 0; i < initialSnakeLength; i++) { positions.Add(new Position(initXPos - i, initYPos)); }
                    break;
                }
            }
            for (int i = 0; i < positions.Count - 1; i++)
            {
                console.WriteAt(snakeBodyChar, positions[i], snakeBodyfgColor, ConsoleColor.Black);
            }
            console.WriteAt(snakeHeadChar, positions.Last(), snakeHeadfgColor, ConsoleColor.Black);
        }

        public void SetDirection(Directions dir)
        {
            this.dir = dir;
        }

        public void SetDirection(ConsoleKey key)
        {  // Set direction according to snakeID and the ConsoleKey
            if (key == KeySet[snakeID, 0]) { this.dir = Directions.Up;  return; }  // No need to do more checks
            if (key == KeySet[snakeID, 1]) { this.dir = Directions.Down; return; }
            if (key == KeySet[snakeID, 2]) { this.dir = Directions.Left;    return; }
            if (key == KeySet[snakeID, 3]) { this.dir = Directions.Right; }
        }

        public int SnakeLength()
        {
            return positions.Count;
        }

        private void DoPostMortem()
        {
            if ((console.CharAt(nextPos) == snakeHeadChar) || (console.CharAt(nextPos) == snakeBodyChar))
                state.CauseOfDeath = "Snake became a cannibal. Started eating itself ....";
            else
                state.CauseOfDeath = "Snake hit an obstacle and died a miserable death. RIP.";
        }

        public void MoveSnake()
        {
            int linksToAdd;

            do
            {
                //  Find next coordinate for the head, based on the direction
                switch (dir)
                {
                    case Directions.Up:    nextPos = new Position(positions.Last().XPos, positions.Last().YPos - 1); break;
                    case Directions.Down:  nextPos = new Position(positions.Last().XPos, positions.Last().YPos + 1); break;
                    case Directions.Left:  nextPos = new Position(positions.Last().XPos - 1, positions.Last().YPos); break;
                    case Directions.Right: nextPos = new Position(positions.Last().XPos + 1, positions.Last().YPos); break;
                }
                // growSnake = board.HasTreat(nextPos);
                linksToAdd = board.TreatPoints(nextPos);  // Did we hit a treat; get the number of snake links to add
                if (linksToAdd == 0 && !console.isBlank(nextPos))  // // Collided with something tha was not a treat. Game over!!
                {  
                    SnakeAlive = false;
                    break;
                }
                linksToBeAdded += linksToAdd;  // There may be links already to be added, so we add them up
                if (linksToBeAdded == 0) // if snake is not growing we need to remove the first entry in positions and blank the position
                {
                    console.WriteAt(' ', positions[0]);
                    positions.RemoveAt(0);
                }
                else // snake is growing so we need to increase the speed, and decrease linksToBeAdded
                {
                    state.SnakeDelay = Math.Max(GameState.MinSnakeDelay, state.SnakeDelay - SnakeLength());
                    state.TreatDelay = Math.Max(GameState.MinTreatDelay, state.TreatDelay - SnakeLength()*10);
                    linksToBeAdded--;
                }
                // Now move the snake head
                console.WriteAt(snakeBodyChar, positions.Last(), snakeBodyfgColor, ConsoleColor.Black);
                console.WriteAt(snakeHeadChar, nextPos, snakeHeadfgColor, ConsoleColor.Black);
                positions.Add(nextPos);
                Thread.Sleep(state.SnakeDelay);
            } while (SnakeAlive);
            DoPostMortem();
            RemoveSnake();
            state.GameOver = true;
        }
    }
}
