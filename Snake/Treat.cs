using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snake
{
    internal class Treat
    {
        // TODO refactor to also hold the number of points the treat gives
        private static char[] TreatChars = { '~', '$', '£' };
        private static int[] TreatPoints = { 1, 2, 3 };
        
        public Position position { get; set; }
        public int numPoints { get; set; } = 0;
        public char character { get; set; }

        private Treat(Position position, char character, int numPoints)
        {
            this.position = position;
            this.character = character;
            this.numPoints = numPoints;
        }

        public static Treat GenerateTreat(Position p)
        {
            Random rand = new Random();
            int TreatType = rand.Next(0, TreatChars.Length);
            return new Treat(p, TreatChars[TreatType], TreatPoints[TreatType]);
        }

        public Position GetPosition() { return position; }
    }
}
