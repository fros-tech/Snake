namespace Snake
{
    internal class Snake
    {
        // This class draws a snake on the console. A thread keeps it moving at a pace that is proportional to its length.
        // The snake detects collisions and acts accordingly. Different treats extend the snake with different number of links
        // The snake will start looking like this: '******O'
        private static ConsoleKey[,] _keySet = {{ConsoleKey.W,       ConsoleKey.S,         ConsoleKey.A,         ConsoleKey.D },
                                               {ConsoleKey.UpArrow, ConsoleKey.DownArrow, ConsoleKey.LeftArrow, ConsoleKey.RightArrow},
                                               {ConsoleKey.NumPad8, ConsoleKey.NumPad5,   ConsoleKey.NumPad4,   ConsoleKey.NumPad6} };
        private static readonly ConsoleColor[] _headColors = { ConsoleColor.Cyan, ConsoleColor.Yellow, ConsoleColor.Red };
        private static readonly ConsoleColor[] _bodyColors = { ConsoleColor.Magenta, ConsoleColor.Blue, ConsoleColor.Green };

        private enum Directions { Left = 0, Right = 1, Up = 2, Down = 3 }

        public const char SnakeHeadChar = 'O';
        public const char SnakeBodyChar = '*';
        private readonly ConsoleColor _headColor;
        private readonly ConsoleColor _bodyColor;
        private bool _snakeAlive = true;
        private readonly int _snakeId; // Determines colors and starting coordinates at start of game
        private const byte InitialSnakeLength = 7;
        private Position? _nextPos;
        private readonly GameState _state;
        private readonly MyConsole _console;
        private Directions _dir;
        private int _linksToBeAdded;
        private readonly Board _board;
        private readonly Thread _thread;
        private bool _snakeActivated;
        private readonly List<Position> _positions;
        
        public Snake(MyConsole console, Board board, GameState state, int snakeId)
        {
            _console = console;
            _board = board;
            _positions = new List<Position>();
            _state = state;
            _snakeId = snakeId;
            _headColor = _headColors[_snakeId];
            _bodyColor = _bodyColors[_snakeId];
            _thread = new Thread(MoveSnake) { IsBackground = true };
            _thread.Start();
        }

        public int GetID() { return _snakeId; }
        public void KillSnake()
        {
            _snakeAlive = false;
            _thread.Join();  // Join to main Thread; We are leaving
        }

        public void Activate() { _snakeActivated = true; }

        public void DeActivate() { _snakeActivated = false; }

        public void ResetSnake() 
        {
            // Needs to do some finagling and fiddling to place more snakes on the board
            // Places snakes relative to each corner and with a direction towards the middle of the board
            // to give the player a chance to adjust before the snake slams into the border or
            // another snake. Apologies for code that may offend the eye.
            int initXPos, initYPos;
            _positions.Clear();  // If we ran game earlier we need to reset
            switch (_snakeId)
            {
                case 0:    // Starts in upper left corner
                {
                    initXPos = 5; initYPos = 5;
                    _dir = Directions.Right;
                    for (int i = 0; i < InitialSnakeLength; i++) { _positions.Add(new Position(initXPos + i, initYPos)); }
                    break;
                }
                case 1:    // Starts in upper right corner
                {
                    initXPos = _console.GetWidth()-InitialSnakeLength-5; initYPos = 5;
                    _dir = Directions.Left;
                    for (int i = 0; i < InitialSnakeLength; i++) { _positions.Add(new Position(initXPos - i, initYPos)); }
                    break;
                }
                case 2:    // Starts in lower right corner
                {
                    initXPos = _console.GetWidth()-InitialSnakeLength-5; initYPos = _console.GetHeight()-5;
                    _dir = Directions.Left;
                    for (int i = 0; i < InitialSnakeLength; i++) { _positions.Add(new Position(initXPos - i, initYPos)); }
                    break;
                }
            }
            for (int i = 0; i < _positions.Count - 1; i++)
            {
                _console.WriteAt(SnakeBodyChar, _positions[i], _bodyColor, ConsoleColor.Black);
            }
            _console.WriteAt(SnakeHeadChar, _positions.Last(), _headColor, ConsoleColor.Black);
        }

        public void SetDirection(ConsoleKey key)
        {  // Set direction according to snakeID and the ConsoleKey, making sure new direction is not directly opposite to the current
            if (key == _keySet[_snakeId, 0] && _dir != Directions.Down)  { _dir = Directions.Up;   return; }
            if (key == _keySet[_snakeId, 1] && _dir != Directions.Up)    { _dir = Directions.Down; return; }
            if (key == _keySet[_snakeId, 2] && _dir != Directions.Right) { _dir = Directions.Left; return; }
            if (key == _keySet[_snakeId, 3] && _dir != Directions.Left)  { _dir = Directions.Right; }
        }

        public int SnakeLength()
        {
            return _positions.Count;
        }

        private void DoPostMortem()  // TODO expand postmortem to tell more details about the cause of death
        {
            if ((_console.CharAt(_nextPos) == SnakeHeadChar) || (_console.CharAt(_nextPos) == SnakeBodyChar))
                _state.CauseOfDeath = "Snake #"+this._snakeId+" became a cannibal. Started eating snake! Yukkk! ....";
            else
                _state.CauseOfDeath = "Snake #"+this._snakeId+" hit an obstacle and died a miserable death. RIP.";
        }

        private void MoveSnake()
        {
            do
            {
                if (!_state.GamePaused && _snakeActivated)
                {
                    //  Find next coordinate for the head, based on the direction
                    _nextPos = _dir switch
                    {
                        Directions.Up    => new Position(_positions.Last().XPos, _positions.Last().YPos - 1),
                        Directions.Down  => new Position(_positions.Last().XPos, _positions.Last().YPos + 1),
                        Directions.Left  => new Position(_positions.Last().XPos - 1, _positions.Last().YPos),
                        Directions.Right => new Position(_positions.Last().XPos + 1, _positions.Last().YPos),
                        _ => _nextPos
                    };
                    // Let board do the checking in the future.
                    // return will be either PORTAL, BLANK, WALL, OTHER SNAKE, OR TREAT POINTS
                    Board.ObstacleTypes Obstacle =
                        _board.CheckForCollision(_nextPos, out int TreatPoints, out Position PortalPosition);
                    switch (Obstacle)
                    {
                        case Board.ObstacleTypes.SPACE : 
                            {
                                if (_linksToBeAdded == 0)
                                {
                                    _console.WriteAt(' ', _positions[0]);
                                    _positions.RemoveAt(0);
                                }
                                else
                                {
                                    _state.SnakeDelay = Math.Max(GameState.MinSnakeDelay, _state.SnakeDelay - SnakeLength());
                                    _state.TreatDelay = Math.Max(GameState.MinTreatDelay, _state.TreatDelay - SnakeLength() * 10);
                                    _linksToBeAdded--;
                                }
                                break;
                            }
                        case Board.ObstacleTypes.SNAKE :
                        case Board.ObstacleTypes.WALL :
                        {
                            _state.GameOver = true;
                            DoPostMortem();
                            break;
                        }
                        case Board.ObstacleTypes.TREAT :
                        {
                            _linksToBeAdded += TreatPoints;
                            _board.RemoveTreat(_nextPos);
                            break;
                        }
                        case Board.ObstacleTypes.PORTAL:
                        {
                            _board.RemovePortal(PortalPosition);  // The portal we exit from
                            _board.RemovePortal(_nextPos);        // The portal we are entering into
                            _nextPos = PortalPosition;            // Next pos is moving out at next portals position
                            break;
                        }
                    }
                    if (!_state.GameOver)  // Let's move the head of the snake to the next position
                    {
                        _console.WriteAt(SnakeBodyChar, _positions.Last(), _bodyColor, ConsoleColor.Black);
                        _console.WriteAt(SnakeHeadChar, _nextPos, _headColor, ConsoleColor.Black);
                        _positions.Add(_nextPos);
                        if (_positions.Count > _state.MaxSnakeLength)  // Update game statistics to use when game ends
                        {
                            _state.MaxSnakeLength = _positions.Count;
                            _state.LongestSnake = _snakeId;
                        }
                    }
                }
                Thread.Sleep(_state.SnakeDelay);
            } while (!_state.EndProgram);
        }
    }
}
