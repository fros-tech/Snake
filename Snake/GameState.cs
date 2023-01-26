namespace Snake
{
    internal class GameState
    {
        public const int MinSnakeDelay = 80;
        public const int MinTreatDelay = 1500;
        public long maxTreatLifetime { get; set; } = 8000;
        public int NumPoints { get; set; } = 0;
        public bool GameOver { get; set; }
        public bool EndProgram { get; set; }
        public int TotalSnakeLength { get; set; } = 0;
        public int SnakeDelay { get; set; } = 180;
        public int TreatDelay { get; set; } = 3000;
        public string CauseOfDeath { get; set; } = "";
    }
}
