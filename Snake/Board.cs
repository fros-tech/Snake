namespace Snake
{
    internal class Board
    {
        private const byte NumInitialTreats = 5;
        private readonly List<Treat> _treats;
        private readonly MyConsole _console;
        private readonly Random _rand;
        private readonly GameState _state;

        public Board(MyConsole console, GameState state)
        {
            this._console = console;
            _treats = new List<Treat>();
            _rand = new Random();
            this._state = state;
            SetupBoard();
        }

        private void SetupBoard()
        {   // Draws the box on the outer edge of the board, and adds initial treats
            _console.WriteAt("+", 0, 0);
            _console.WriteAt("+", 0, _console.GetHeight() - 1);
            _console.WriteAt("+", _console.GetWidth() - 1, 0);
            _console.WriteAt("+", _console.GetWidth() - 1, _console.GetHeight() - 1);
            for (byte b = 1; b < _console.GetWidth() - 1; b++)
            {
                _console.WriteAt("-", b, 0);
                _console.WriteAt("-", b, _console.GetHeight() - 1);
            }
            for (byte b = 1; b < _console.GetHeight() - 1; b++)
            {
                _console.WriteAt("|", 0, b);
                _console.WriteAt("|", _console.GetWidth() - 1, b);
            }
            for (byte b = 0; b < NumInitialTreats; b++)
                AddTreat();
        }

        private bool FindBlankSpot(out Position pos, int margin)
        {
            // make up to 5 attempts at blank spot on the Board
            // Board becomes crowded, and the probability of finding
            // a blank spot varies as the gameplay progresses.
            // margin is the desired distance to the edge of the board
            Position tempPos = new Position();
            int count = 0;
            do
            {
                tempPos.XPos = _rand.Next(1,_console.GetWidth()-margin);
                tempPos.YPos = _rand.Next(1,_console.GetHeight()-margin);
                if (_console.IsBlank(tempPos.XPos, tempPos.YPos))
                {
                    pos = tempPos;
                    return true;
                };
                count++;
            } while (count < 5);  // If 5 attempts where unsuccessful, give up
            pos = null;
            return false;
        }
        
        private void AddTreat()
        {
            // make up to 5 attempts at placing a treat on the Board
            // Board becomes crowded, and the probability of finding
            // a blank spot decreases with the increase of number of treats
            Position tempPos;
            if (FindBlankSpot(out tempPos, 1))    // Let's see if we can find a blank spot to place a treat
            {
                Treat t = Treat.GenerateTreat(tempPos);
                _console.WriteAt(t.Character, t.GetPosition(), t.FgColor, t.BgColor);
                _treats.Add(t);
            }
        }

        public void AddTreats() // This thread method constantly adds treats to the board
        {
            while (!_state.GameOver)
            {
                AddTreat();
                foreach (Treat t in _treats)
                {
                    t.lifeTime += _state.TreatDelay;
                    if(t.lifeTime > _state.maxTreatLifetime)
                        RemoveTreat(t);
                }                Thread.Sleep(_state.TreatDelay);
            }
        }

        public void RemoveTreat(Treat t)
        {
            _console.WriteAt(' ', t.Position);
            _treats.Remove(t);
        }
        
        public int TreatPoints(Position position)  // Check if there is a treat at position
        {
            foreach(Treat t in _treats) 
            {
                if ((t.GetPosition().XPos == position.XPos) && (t.GetPosition().YPos == position.YPos))
                    return t.NumPoints;
            }
            return 0;
        }
    }
}
