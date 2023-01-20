using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Snake
{
    internal class Snake
    {
        // Thias class contains code that draws a snake on the console.
        // It also contains a method that will let the snake move
        // according to a defined direction
        // The snake detects collisions with console 'objects'
        // These objects can be food, whic causes the snake to increase
        // in length or obstacles whic cause the game to end.
        // The snake will start looking like this: '******O'

        const char snakeHead = 'O';
        const char snakeBodyChar = '*';
        bool SnakeAlive = true;
        const Byte InitialSnakeLength = 7;
        GameState state;
        public enum Directions
        {
            Left = 0, Right = 1, Up = 2, Down = 3
        }

        MyConsole console;
        Directions dir = Directions.Right;
        Board board;
        List<Position> positions;
        public Snake(MyConsole console, Board board, GameState state)
        {
            this.console = console;
            this.board = board;
            positions = new List<Position>();
            DrawInitialSnake(console);
            this.state = state;
        }

        public void killSnake()
        {
            SnakeAlive = false;
        }

        private void RemoveSnake()
        {
            foreach(Position p in positions)
                console.WriteAt(' ', p);
            positions = new List<Position>();  // Leave the previous positions to the garbage collector
        }

        private void DrawInitialSnake(MyConsole c)
        {
            // populate first part of the snake
            int MidX = c.getWidth() / 2;
            int MidY = c.getHeight() / 2;
            for (int i = 0; i < InitialSnakeLength; i++)
            {
                positions.Add(new Position(MidX + i, MidY));
            }
            for (int i = 0; i < positions.Count - 1; i++)
            {
                c.WriteAt(snakeBodyChar, positions[i]);
            }
            c.WriteAt(snakeHead, positions.Last());
        }

        public void SetDirection(Directions dir)
        {
            this.dir = dir;
        }

        public int SnakeLength()
        {
            return positions.Count;
        }

        public void MoveSnake()
        {
            do
            {
                bool growSnake;
                //   Find next coordinate for the head, based on the direction
                Position nextPos = null;
                switch (dir)
                {
                    case Directions.Up:    nextPos = new Position(positions.Last().XPos, positions.Last().YPos - 1); break;
                    case Directions.Down:  nextPos = new Position(positions.Last().XPos, positions.Last().YPos + 1); break;
                    case Directions.Left:  nextPos = new Position(positions.Last().XPos - 1, positions.Last().YPos); break;
                    case Directions.Right: nextPos = new Position(positions.Last().XPos + 1, positions.Last().YPos); break;
                }
                growSnake = board.HasTreat(nextPos);
                if (!growSnake && !console.isBlank(nextPos))  // Is it end of the game
                {
                    SnakeAlive = false;
                    break;
                }
                if (!growSnake) // if snake is not growing we need to remove the first entry in positions and blank the position
                {
                    console.WriteAt(' ', positions[0]);
                    positions.RemoveAt(0);
                }
                // Now move the snake head
                console.WriteAt('*', positions.Last());
                console.WriteAt('O', nextPos);
                positions.Add(nextPos);
                console.WriteAt(" Snake Length :" + SnakeLength() + " ", 5, 0);  // TODO Add snake length update on console
                Thread.Sleep(100);
            } while (SnakeAlive);
            state.GameOver = true;
            RemoveSnake();
        }
    }
}
