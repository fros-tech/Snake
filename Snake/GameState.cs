namespace Snake
{
    internal class GameState
    {
        public const int MinSnakeDelay = 80;
        public const int MinTreatDelay = 1500;
        public int Fluke = 100; // Values greater than zero affects the lifetime of the treats
        public long maxTreatLifetime { get; set; } = 12000;
        public long maxPortalLifetime { get; set; } = 15000;
        public bool GameOver { get; set; }
        public bool GamePaused { get; set; }
        public int MaxSnakeLength { get; set; }
        public int LongestSnake { get; set; }
        public bool EndProgram { get; set; }
        public int SnakeDelay { get; set; } = 180;
        public int TreatDelay { get; set; } = 3000;
        public int PortalDelay { get; set; } = 3000;
        public string CauseOfDeath { get; set; } = "";

        public void TogglePaused() { GamePaused = !GamePaused; }

        public void Reset()
        {
            CauseOfDeath = "";
            MaxSnakeLength = 0;
            LongestSnake = 0;
            GameOver = false;
            EndProgram = false;
            SnakeDelay = 180;
            TreatDelay = 3000;
            GamePaused = false;
        }
    }
}
