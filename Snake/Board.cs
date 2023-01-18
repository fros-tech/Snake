using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snake
{
    internal class Board
    {
        List<Treat> Treats;
        MyConsole c;
        Random rand;
        public Board(MyConsole c)
        {
            this.c = c;
            Treats = new List<Treat>();
            rand = new Random();
        }

        public void AddTreat()
        {
            int tempXPos, tempYPos;
            for(int i=0; i<5; i++)
            {
                tempXPos = rand.Next(1,c.getWidth());
                tempYPos = rand.Next(1,c.getHeight());
                if (c.isBlank(tempXPos,tempYPos)) 
                { 
                    c.WriteAt('+', new Position(tempXPos, tempYPos));
                    break;
                }

            }
            // TODO add Treat at random blank position
        }

        public bool HasTreat(Position position)
        {
            foreach(Treat t in Treats) 
            {
                if (t.GetPosition() == position)
                    return true;
            }
            return false;
        }
    }
}
