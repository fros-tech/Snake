
namespace Snake
{
    internal class Treat
    {
        public static readonly char[] TreatChars = { '~', '$', '£', '#' };
        private static readonly int[] TreatPoints = { 1, 2, 3, 5 };
        private static readonly ConsoleColor[] FgColors = { ConsoleColor.White, ConsoleColor.Yellow, ConsoleColor.Blue, ConsoleColor.Cyan };
        private static readonly ConsoleColor[] BgColors = { ConsoleColor.Black, ConsoleColor.Black, ConsoleColor.Black, ConsoleColor.Black };

        public Position Position { get; set; }
        public int NumPoints { get; set; }
        public char Character { get; set; }
        public ConsoleColor FgColor { get; set; }
        public ConsoleColor BgColor { get; set; }
        public long lifeTime { get; set; } // milliseconds

        private Treat(Position position, char character, int numPoints, ConsoleColor fgc, ConsoleColor bgc)
        {
            Position = position;
            Character = character;
            NumPoints = numPoints;
            FgColor = fgc;
            BgColor = bgc;
            lifeTime = 0;
        }

        public static Treat GenerateTreat(Position p)
        {
            Random rand = new Random();
            int treatType = rand.Next(0, TreatChars.Length);
            return new Treat(p, TreatChars[treatType], TreatPoints[treatType], FgColors[treatType], BgColors[treatType]);
        }

        public Position GetPosition() { return Position; }
        
    }
}
