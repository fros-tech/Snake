namespace Snake
{
    internal class Board
    {
        private const byte NumInitialTreats = 12;
        public const int FlukeTimerDelay = 15000;  // 15 seconds
        private readonly List<Treat> _treats;
        private readonly List<Portal> _portals;
        private readonly MyConsole _console;
        private readonly Random _rand;
        private readonly GameState _state;
        private readonly object _treatLock = new();
        private readonly object _portalLock = new();
        private Thread _treatThread;
        private Thread _portalThread;
        private System.Timers.Timer _flukeTimer;

        public enum ObstacleTypes { PORTAL = 0, SPACE = 1, WALL = 2, SNAKE = 3, TREAT = 4, OTHER = 5 };
        private static readonly char[] WallChars = { 'P', 'Q' }; // In the buffer ║ and ═ translates into P and Q

        private bool _boardActivated;

        public Board(MyConsole console, GameState state)
        {
            _console = console;
            _treats = new List<Treat>();
            _portals = new List<Portal>();
            _rand = new Random();
            _state = state;
            _treatThread = new Thread(AddTreats);
            _treatThread.Start();
            _portalThread = new Thread(AddPortals);
            _portalThread.Start();
            _flukeTimer = new System.Timers.Timer(5000);
        }

        public void ResetBoard()
        {   // Draws the box on the outer edge of the board, and adds initial treats
            int ch = _console.GetHeight();
            int cw = _console.GetWidth();
            _treats.Clear();
            _flukeTimer.Stop();
            _console.WriteAtBuf('╔', 0, 0, ConsoleColor.White, ConsoleColor.Black);
            _console.WriteAtBuf('╚', 0, ch - 2, ConsoleColor.White, ConsoleColor.Black);
            _console.WriteAtBuf('╗', cw - 1, 0, ConsoleColor.White, ConsoleColor.Black);
            _console.WriteAtBuf('╝', cw - 1, ch - 2, ConsoleColor.White, ConsoleColor.Black);
            for (byte b = 1; b < cw - 1; b++)
            {
                _console.WriteAtBuf('═', b, 0, ConsoleColor.White, ConsoleColor.Black);
                _console.WriteAtBuf('═', b, ch - 2, ConsoleColor.White, ConsoleColor.Black);
            }
            for (byte b = 1; b < ch - 2; b++)
            {
                _console.WriteAtBuf('║', 0, b, ConsoleColor.White, ConsoleColor.Black);
                _console.WriteAtBuf('║', cw - 1, b, ConsoleColor.White, ConsoleColor.Black);
            }
            _console.WriteCon();
            for (byte b = 0; b < NumInitialTreats; b++)
              AddTreat();
        }

        public void ActivateBoard() { _boardActivated = true; }

        public void DeActivateBoard() { _boardActivated = false; }
        
        private bool FindBlankSpot(out Position pos, int margin)
        {
            // make up to 5 attempts at blank spot on the Board
            // Board becomes crowded, and the probability of finding
            // a blank spot varies as the gameplay progresses.
            // margin is the desired distance to the edge of the board
            Position tempPos = new Position();
            int count = 0;
            int w = _console.GetWidth();
            int h = _console.GetHeight();
            do
            {
                tempPos.XPos = _rand.Next(1+margin,w-margin);
                tempPos.YPos = _rand.Next(1+margin,h-margin);
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

        public void RemoveTreat(Position pos)
        {
            for (int i=0; i < _treats.Count; i++)
            {
                if (_treats[i].Position.XPos != pos.XPos)  // Fail early!
                    continue;
                if (_treats[i].Position.YPos != pos.YPos)
                    continue;
                {
                    lock (_treatLock)
                    {
                        _console.WriteAt(' ', _treats[i].Position);
                        _treats.RemoveAt(i);
                    }
                }
            }
        }
        
        private void AddTreat()
        {
            Position tempPos;
            if (FindBlankSpot(out tempPos, 1))    // Let's see if we can find a blank spot to place a treat
            {
                lock (_treatLock)
                {
                    Treat t = Treat.GenerateTreat(tempPos);
                    _console.WriteAt(t.Character, t.Position, t.FgColor, t.BgColor);
                    _treats.Add(t);
                }
            }
        }

        public void AddPortal()
        {
            Position pos1;
            do
            {
                if (FindBlankSpot(out pos1, 7))  // Find random space for a new portal
                {
                    Portal p = new Portal(pos1);
                    lock (_portalLock)
                    {
                        _console.WriteAt(Portal.PortalChar, pos1);
                        _portals.Add(p);
                    }
                }
            } while (_portals.Count < 2);  // There must at least be 2 portals for this to work
        }

        public void RemovePortal(Position pos)
        {
            for (int i=0; i < _portals.Count; i++)
            {
                if (_portals[i].Position.XPos != pos.XPos) continue;  // Fail early!
                if (_portals[i].Position.YPos != pos.YPos) continue;
                lock (_portalLock)
                {
                    _console.WriteAt(' ', _portals[i].Position);
                    _portals.RemoveAt(i);
                }
            }
        }

        private Portal ChoosePortal(Position p)  // Choose portal at other position than p
        {
            int i;
            do 
            { i = _rand.Next(_portals.Count); }
            while ((_portals[i].Position.XPos == p.XPos && _portals[i].Position.YPos == p.YPos));
            // Find a random portal different from the one at Position p
            return _portals[i];
        }
        
        public void AddPortals()
        {
            while (!_state.EndProgram)
            {
                if (!_state.GamePaused && _boardActivated)
                {
                    AddPortal();
                    for (int i = 0; i < _portals.Count; i++)  // Check if any portals need to be removed
                    {   // Tempting to use foreach, but will cause collection modified exception
                        _portals[i].lifeTime += _state.TreatDelay;
                        if (_portals[i].lifeTime <= _state.maxPortalLifetime) continue;
                        lock (_treatLock)
                        {
                            _console.WriteAt(' ', _portals[i].Position);
                            _portals.RemoveAt(i);
                        }
                    }
                }
                Thread.Sleep(_state.PortalDelay);
            }
        }

        public void AddTreats() // This thread method constantly adds treats to the board
        {
            while (!_state.EndProgram)
            {
                if (!_state.GamePaused && _boardActivated)
                {
                    AddTreat();
                    for (int i = 0; i < _treats.Count; i++)
                    {   // Tempting to use foreach, but will cause collection modified exception
                        _treats[i].lifeTime -= _state.TreatDelay / (10 + _state.Fluke);  // Fluke affects treat timeout
                        if (_treats[i].lifeTime > 0) continue;
                        lock (_treatLock)
                        {
                            _console.WriteAt(' ', _treats[i].Position);
                            _treats.RemoveAt(i);
                        }
                    }
                }
                Thread.Sleep(_state.TreatDelay);
            }
        }
        
        public int TreatPoints(Position position)  // Check if there is a treat at position
        {
            foreach(Treat t in _treats) 
            {
                if ((t.Position.XPos == position.XPos) && (t.Position.YPos == position.YPos))
                    return t.NumPoints;
            }
            return 0;
        }

        public void ResetFluke()
        {
            _state.Fluke = 0;
            _flukeTimer.Stop();
        }
        public void Fluke()
        {
            if (_state.Fluke == 0)
            {
                _flukeTimer.Interval = FlukeTimerDelay;
                _flukeTimer.Elapsed += (sender, e) => ResetFluke();
                if (_rand.Next(100) > 10)
                {
                    if (_rand.Next(100) > 25)
                        _state.Fluke = 20;
                    else
                        _state.Fluke = -5;
                }

                _flukeTimer.Start();
            }
        }
        public ObstacleTypes CheckForCollision(Position checkPos, out int TreatPoints, out Position PortalPosition)
        // Checks a position and returns a plethora of data depending on what is found at checkPos
        // Possibilities are BLANK, TREAT, WALL, PORTAL, SNAKE(Own or Other)
        {
            char c = _console.CharAt(checkPos);
            PortalPosition = null;
            TreatPoints = 0;
            if (c == MyConsole.Space) // Ok now we have to get to work     // SPACE
                return ObstacleTypes.SPACE;
            if (Treat.TreatChars.Contains(c))                              // TREAT
            {
                TreatPoints = this.TreatPoints(checkPos);
                return ObstacleTypes.TREAT;
            }
            if (WallChars.Contains(c))                                     // WALL
                return ObstacleTypes.WALL;
            if (c is Snake.SnakeBodyChar or Snake.SnakeHeadChar)           // SNAKE
                return ObstacleTypes.SNAKE;
            if (c != Portal.PortalChar) return ObstacleTypes.OTHER;        // We really shouldn't end up here
            // If we got here, it has to be a portal
            PortalPosition = ChoosePortal(checkPos).GetPosition();         // PORTAL
            return ObstacleTypes.PORTAL;
        }
    }
}
