
namespace Snake
{
    internal class Treat
    {
        // TODO Let treats have a lifetime, after which they disappear again
        private static readonly char[] TreatChars = { '~', '$', '£' };
        private static readonly int[] TreatPoints = { 1, 2, 3 };
        private static readonly ConsoleColor[] FgColors = { ConsoleColor.White, ConsoleColor.Yellow, ConsoleColor.Blue };
        private static readonly ConsoleColor[] BgColors = { ConsoleColor.Black, ConsoleColor.Black, ConsoleColor.Black };

        public Position Position { get; set; }
        public int NumPoints { get; set; }
        public char Character { get; set; }
        public ConsoleColor FgColor { get; set; }
        public ConsoleColor BgColor { get; set; }

        private Treat(Position position, char character, int numPoints, ConsoleColor fgc, ConsoleColor bgc)
        {
            Position = position;
            this.Character = character;
            this.NumPoints = numPoints;
            FgColor = fgc;
            BgColor = bgc;
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
