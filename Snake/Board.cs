using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snake
{
    internal class Board
    {
        const char TreatChar = '@';
        List<Treat> Treats;
        MyConsole c;
        Random rand;
        public Board(MyConsole c)
        {
            this.c = c;
            Treats = new List<Treat>();
            rand = new Random();
            
            c.WriteAt("+", 0, 0);
            c.WriteAt("+", 0, c.getHeight()-1);
            c.WriteAt("+", c.getWidth()-1, 0);
            c.WriteAt("+", c.getWidth()-1, c.getHeight()-1);
            for (byte b = 1; b < c.getWidth()-1; b++)
            {
                c.WriteAt("-", b, 0);
                c.WriteAt("-", b, c.getHeight()-1);
            }
            for (byte b=1; b < c.getHeight()-1; b++)
            {
                c.WriteAt("|", 0, b);
                c.WriteAt("|", c.getWidth()-1, b);
            }

        }

        public void AddTreat()
        {
            // make up to 5 attempts at placing a treat on the Board
            Position tempPos = new Position();
            int count = 0;
            bool placeTreat;
            do
            {
                tempPos.XPos = rand.Next(1,c.getWidth()-1);
                tempPos.YPos = rand.Next(1,c.getHeight()-1);
                placeTreat = c.isBlank(tempPos.XPos, tempPos.YPos);
                count++;
            } while ((count < 5) && (!placeTreat));
            if (placeTreat)
                c.WriteAt(TreatChar, tempPos);
        }

        public bool HasTreat(Position position)
        {
            foreach(Treat t in Treats) 
            {
                if ((t.GetPosition().XPos == position.XPos) && (t.GetPosition().YPos == position.YPos))
                    return true;
            }
            return false;
        }
    }
}
