using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        const Byte InitialSnakeLength = 7;
        public enum Directions
        {
            Left = 0, Right = 1, Up = 2, Down = 3
        }

        MyConsole c;
        Directions dir = Directions.Right;
        Board b;
        List<Position> positions;
        public Snake(MyConsole c, Board b) 
        {
            this.c = c;
            this.b = b;
            positions= new List<Position>();

            int MidX = c.getWidth() / 2;
            int MidY = c.getHeight() / 2;
            for(int i=0; i < InitialSnakeLength; i++) 
            {
                positions.Add(new Position(MidX+i, MidY));
            }
            for (int i = 0; i < positions.Count-1; i++)
            {
                c.WriteAt('*', positions[i]);
            }
            c.WriteAt('O', positions.Last());
            // TODO populate first part of the snake
            // TODO Show initial snake on MyConsole
        }

        public void SetDirection(Directions dir)
        {
            this.dir = dir;
        }

        long SnakeLength()
        {
            return positions.Count;
        }

        public void MoveSnake()
        {
            bool suddenDeath = false;
            do
            {
                // TODO move snake
                //   Find next coordinate for the head, based on the direction
                Position nextPos = null;
                bool growSnake;
                switch (dir)
                {
                    case Directions.Up:
                        nextPos = new Position(positions.Last().XPos, positions.Last().YPos - 1);
                        break;
                    case Directions.Down:
                        nextPos = new Position(positions.Last().XPos, positions.Last().YPos + 1);
                        break;
                    case Directions.Left:
                        nextPos = new Position(positions.Last().XPos - 1, positions.Last().YPos);
                        break;
                    case Directions.Right:
                        nextPos = new Position(positions.Last().XPos + 1, positions.Last().YPos);
                        break;
                }
                growSnake = b.HasTreat(nextPos);
                if (!growSnake && !c.isBlank(nextPos))  // Is it end of the game
                {
                    suddenDeath = true;
                }
                if (!growSnake) // if snake is not growing we need to remove the first entry in positions and blank the position
                {
                    c.WriteAt(' ', positions[0]);
                    positions.RemoveAt(0);
                }
                // Now move the snake head
                c.WriteAt('*', positions.Last());
                c.WriteAt('O', nextPos);
                positions.Add(nextPos);
                Thread.Sleep(200);
            } while (!suddenDeath);
        }
    }
}
