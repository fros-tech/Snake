using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snake
{
    internal class Snake
    {
        MyConsole c;
        Program.Directions dir;
        List<Position> positions;
        public Snake(MyConsole c) 
        {
            this.c = c;
            positions= new List<Position>();
            // TODO populate first part of the snake
        }

        public void SetDirection(Program.Directions dir)
        {
            this.dir = dir;
        }

        long SnakeLength()
        {
            return positions.Count;
        }

        public void MoveSnake()
        {
            // TODO move snake
            // find next head position based on current direction
            // Add new head postion
            //   if new head position is wall or snake then end game as failed
            //   else if new head position is blank remove end of snake (otherwise the snake will grow one item
        }
    }
}
