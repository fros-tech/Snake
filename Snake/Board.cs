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
            // make up to 5 attempts at placing a treat on the Board
            Position tempPos = new Position();
            int count = 0;
            bool placeTreat;
            do
            {
                tempPos.XPos = rand.Next(1,c.getWidth());
                tempPos.YPos = rand.Next(1,c.getHeight());
                placeTreat = c.isBlank(tempPos.XPos, tempPos.YPos);
                count++;
            } while ((count < 5) && (!placeTreat));
            if (placeTreat)
                c.WriteAt('¤', tempPos);
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
