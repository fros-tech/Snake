using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snake
{
    internal class GameState
    {
        public const int MinSnakeDelay = 80;
        public const int MinTreatDelay = 1500;
        public int NumPoints { get; set; } = 0;
        public bool GameOver { get; set; } = false;
        public bool EndProgram { get; set; } = false;
        public int TotalSnakeLength { get; set; } = 0;
        public int SnakeDelay { get; set; } = 180;
        public int TreatDelay { get; set; } = 3000;
        public string CauseOfDeath { get; set; } = "";
    }
}
