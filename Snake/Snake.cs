namespace Snake
{
    internal class Snake
    {
        //
        // This class contains code that draws a snake on the console.
        // It also contains a method that will let the snake move
        // according to a defined direction
        // The snake detects collisions with console 'objects'
        // These objects can be food, which cause the snake to increase
        // in length or obstacles which cause the game to end.
        // The snake will start looking like this: '******O'
        //
        private static ConsoleKey[,] _keySet = {{ConsoleKey.W,       ConsoleKey.S,         ConsoleKey.A,         ConsoleKey.D },
                                               {ConsoleKey.UpArrow, ConsoleKey.DownArrow, ConsoleKey.LeftArrow, ConsoleKey.RightArrow},
                                               {ConsoleKey.NumPad8, ConsoleKey.NumPad5,   ConsoleKey.NumPad4,   ConsoleKey.NumPad6} };
        private static readonly ConsoleColor[] _headColors = { ConsoleColor.Cyan, ConsoleColor.Yellow, ConsoleColor.Red };
        private static readonly ConsoleColor[] _bodyColors = { ConsoleColor.Magenta, ConsoleColor.Blue, ConsoleColor.Green };

        private enum Directions { Left = 0, Right = 1, Up = 2, Down = 3 }

        private const char SnakeHeadChar = 'O';
        private const char SnakeBodyChar = '*';
        private readonly ConsoleColor _headColor;
        private readonly ConsoleColor _bodyColor;
        private bool _snakeAlive = true;
        private readonly int _snakeId; // indicates which number snake it is. Determines initiating coordinates, and keyboard keys
                                       // used to control the snake
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
            _thread = new Thread(MoveSnake);
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

        private void DoPostMortem()
        {
            if ((_console.CharAt(_nextPos) == SnakeHeadChar) || (_console.CharAt(_nextPos) == SnakeBodyChar))
                _state.CauseOfDeath = "Snake became a cannibal. Started eating itself ....";
            else
                _state.CauseOfDeath = "Snake hit an obstacle and died a miserable death. RIP.";
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
                        Directions.Up => new Position(_positions.Last().XPos, _positions.Last().YPos - 1),
                        Directions.Down => new Position(_positions.Last().XPos, _positions.Last().YPos + 1),
                        Directions.Left => new Position(_positions.Last().XPos - 1, _positions.Last().YPos),
                        Directions.Right => new Position(_positions.Last().XPos + 1, _positions.Last().YPos),
                        _ => _nextPos
                    };
                    
                    // Let board do the checking in the future.
                    // return will be either PORTAL, BLANK, WALL, OTHER SNAKE, OR TREAT POINTS
                    // public char CheckForCollision(Position checkPos, out int TreatPoints, out Position PortalPosition) {}

                    // **** First lets see if we collided with something; A treat or something else ****
                    int linksToAdd =
                        _board.TreatPoints(_nextPos); // Did we hit a treat; get the number of snake links to add
                    if (linksToAdd == 0)
                    {
                        if (!_console.IsBlank(_nextPos)) // Collided with something tha was not a treat. Game over!!
                        {
                            _state.GameOver = true;
                            DoPostMortem();
                        }
                    }
                    else
                    {
                        // Lets remember to remove the treat, since it has been eaten
                        _board.RemoveTreat(_nextPos);
                    }

                    // **** If we didnt collide with an obstacle, lets see what to do with the snake.
                    if (!_state.GameOver)
                    {
                        _linksToBeAdded += linksToAdd; // There may be links already to be added, so we add them up
                        if (_linksToBeAdded ==
                            0) // if snake is not growing we need to remove the first entry in positions and blank the position
                        {
                            _console.WriteAt(' ', _positions[0]);
                            _positions.RemoveAt(0);
                        }
                        else // snake is growing so we need to increase the speed, and decrease linksToBeAdded
                        {
                            _state.SnakeDelay = Math.Max(GameState.MinSnakeDelay, _state.SnakeDelay - SnakeLength());
                            _state.TreatDelay = Math.Max(GameState.MinTreatDelay,
                                _state.TreatDelay - SnakeLength() * 10);
                            _linksToBeAdded--;
                        }

                        // Now move the snake head
                        _console.WriteAt(SnakeBodyChar, _positions.Last(), _bodyColor, ConsoleColor.Black);
                        _console.WriteAt(SnakeHeadChar, _nextPos, _headColor, ConsoleColor.Black);
                        _positions.Add(_nextPos);
                        if (_positions.Count > _state.MaxSnakeLength)
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
