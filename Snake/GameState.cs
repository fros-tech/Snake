using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snake
{
    internal class GameState
    {
        public int NumPoints { get; set; } = 0;
        public bool GameOver { get; set; } = false;
        public int SnakeDelay { get; set; } = 180;
        public int TreatDelay { get; set; } = 3000;
    }
}
