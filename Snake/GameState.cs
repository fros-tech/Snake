namespace Snake
{
    internal class GameState
    {
        public const int MinSnakeDelay = 80;
        public const int MinTreatDelay = 1500;
        public long maxTreatLifetime { get; set; } = 12000;
        public int NumPoints { get; set; } = 0;
        public bool GameOver { get; set; }
        public bool GamePaused { get; set; }
        public bool EndProgram { get; set; }
        public int TotalSnakeLength { get; set; } = 0;
        public int SnakeDelay { get; set; } = 180;
        public int TreatDelay { get; set; } = 3000;
        public string CauseOfDeath { get; set; } = "";

        public void TogglePaused()
        {
            GamePaused = !GamePaused;
        }

        public void Reset()
        {
            NumPoints = 0;
            GameOver = false;
            EndProgram = false;
            TotalSnakeLength = 0;
            SnakeDelay = 180;
            TreatDelay = 3000;
            GamePaused = false;
        }
    }
}
