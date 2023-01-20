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
        const byte NumInitialTreats = 5;
        int TreatDelay = 3000;

        List<Treat> Treats;
        MyConsole console;
        Random rand;
        GameState state;

        public Board(MyConsole console, GameState state)
        {
            this.console = console;
            Treats = new List<Treat>();
            rand = new Random();
            this.state = state;
            SetupBoard();
        }

        private void SetupBoard()
        {
            console.WriteAt("+", 0, 0);
            console.WriteAt("+", 0, console.getHeight() - 1);
            console.WriteAt("+", console.getWidth() - 1, 0);
            console.WriteAt("+", console.getWidth() - 1, console.getHeight() - 1);
            for (byte b = 1; b < console.getWidth() - 1; b++)
            {
                console.WriteAt("-", b, 0);
                console.WriteAt("-", b, console.getHeight() - 1);
            }
            for (byte b = 1; b < console.getHeight() - 1; b++)
            {
                console.WriteAt("|", 0, b);
                console.WriteAt("|", console.getWidth() - 1, b);
            }
            for (byte b = 0; b < NumInitialTreats; b++)
                AddTreat();
        }

        public void AddTreat()
        {
            // make up to 5 attempts at placing a treat on the Board
            Position tempPos = new Position();
            int count = 0;
            bool placeTreat;
            do
            {
                tempPos.XPos = rand.Next(1,console.getWidth()-1);
                tempPos.YPos = rand.Next(1,console.getHeight()-1);
                placeTreat = console.isBlank(tempPos.XPos, tempPos.YPos);
                count++;
            } while ((count < 5) && (!placeTreat));
            if (placeTreat)
            {
                console.WriteAt(TreatChar, tempPos);
                Treats.Add(new Treat(tempPos, TreatChar));
            }
        }

        public void AddTreats()  // This thread method constantly adds treats to the board
        {
            while(!state.GameOver)
            {
                AddTreat();
                Thread.Sleep(TreatDelay);
            }
        }

        public bool HasTreat(Position position)  // Check if there is a treat at position
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
