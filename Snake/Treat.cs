using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snake
{
    internal class Treat
    {
        // TODO Let treats have a lifetime, after which they disappear again
        private static char[] TreatChars = { '~', '$', '£' };
        private static int[] TreatPoints = { 1, 2, 3 };
        private static ConsoleColor[] fgColors = { ConsoleColor.White, ConsoleColor.Yellow, ConsoleColor.Blue };
        private static ConsoleColor[] bgColors = { ConsoleColor.Black, ConsoleColor.Black, ConsoleColor.Black };

        public Position position { get; set; }
        public int numPoints { get; set; } = 0;
        public char character { get; set; }
        public ConsoleColor fgColor { get; set; }
        public ConsoleColor bgColor { get; set; }

        private Treat(Position position, char character, int numPoints, ConsoleColor fgc, ConsoleColor bgc)
        {
            this.position = position;
            this.character = character;
            this.numPoints = numPoints;
            fgColor = fgc;
            bgColor = bgc;
        }

        public static Treat GenerateTreat(Position p)
        {
            Random rand = new Random();
            int TreatType = rand.Next(0, TreatChars.Length);
            return new Treat(p, TreatChars[TreatType], TreatPoints[TreatType], fgColors[TreatType], bgColors[TreatType]);
        }

        public Position GetPosition() { return position; }
        
    }
}
