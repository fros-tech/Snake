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
        public Board(MyConsole c)
        {
            this.c = c;
            Treats= new List<Treat>();
        }

        public void AddTreat()
        {
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
