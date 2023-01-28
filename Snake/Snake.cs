namespace Snake
{
    internal class Snake
    {
        // This class contains code that draws a snake on the console.
        // It also contains a method that will let the snake move
        // according to a defined direction
        // The snake detects collisions with console 'objects'
        // These objects can be food, which cause the snake to increase
        // in length or obstacles which cause the game to end.
        // The snake will start looking like this: '******O'
        private static ConsoleKey[,] _keySet = {{ConsoleKey.W,       ConsoleKey.S,         ConsoleKey.A,         ConsoleKey.D },
                                               {ConsoleKey.UpArrow, ConsoleKey.DownArrow, ConsoleKey.LeftArrow, ConsoleKey.RightArrow},
                                               {ConsoleKey.NumPad8, ConsoleKey.NumPad5,   ConsoleKey.NumPad4,   ConsoleKey.NumPad6}
        };

        public enum Directions { Left = 0, Right = 1, Up = 2, Down = 3 }

        const char SnakeHeadChar = 'O';
        const char SnakeBodyChar = '*';
        private const ConsoleColor SnakeHeadfgColor = ConsoleColor.Red;
        private const ConsoleColor SnakeBodyfgColor = ConsoleColor.Magenta;
        // private ConsoleKey[] _snakesKeys;
        bool _snakeAlive = true;
        private int _snakeId; // indicates which number snake it is. Determines initiating coordinates, and keyboard keys
                             // used to control the snake
        const byte InitialSnakeLength = 7;
        // const int MinSnakeDelay = 80;
        Position? _nextPos;
        GameState _state;
        MyConsole _console;
        Directions _dir = Directions.Right;
        private int _linksToBeAdded;
        Board _board;
        private Thread _thread;
        // private bool _paused = false;
        List<Position> _positions;
        
        public Snake(MyConsole console, Board board, GameState state, int snakeId)
        {
            _console = console;
            _board = board;
            _positions = new List<Position>();
            // DrawInitialSnake(console);
            _state = state;
            _snakeId = snakeId;
            _thread = new Thread(MoveSnake);
            _thread.Start();
        }

        public void KillSnake()
        {
            _snakeAlive = false;
            _thread.Join();  // Join to main Thread; We are leaving
        }

//        public void SetPaused()
//        {
//            _paused = true;
//        }

        private void RemoveSnake()
        {
            foreach(Position p in _positions)
                _console.WriteAt(' ', p);
            _positions = new List<Position>();  // Leave any previous positions to the garbage collector
        }

        public void DrawInitialSnake() 
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
                    initXPos = 5;
                    initYPos = 5;
                    _dir = Directions.Right;
                    for (int i = 0; i < InitialSnakeLength; i++) { _positions.Add(new Position(initXPos + i, initYPos)); }
                    break;
                }
                case 1:    // Starts in upper right corner
                {
                    initXPos = _console.GetWidth()-InitialSnakeLength-5;
                    initYPos = 5;
                    _dir = Directions.Left;
                    for (int i = 0; i < InitialSnakeLength; i++) { _positions.Add(new Position(initXPos - i, initYPos)); }
                    break;
                }
                case 2:    // Starts in lower right corner
                {
                    initXPos = _console.GetWidth()-InitialSnakeLength-5;
                    initYPos = _console.GetHeight()-5;
                    _dir = Directions.Left;
                    for (int i = 0; i < InitialSnakeLength; i++) { _positions.Add(new Position(initXPos - i, initYPos)); }
                    break;
                }
            }
            for (int i = 0; i < _positions.Count - 1; i++)
            {
                _console.WriteAt(SnakeBodyChar, _positions[i], SnakeBodyfgColor, ConsoleColor.Black);
            }
            _console.WriteAt(SnakeHeadChar, _positions.Last(), SnakeHeadfgColor, ConsoleColor.Black);
            _snakeAlive = true;
        }

        public void SetDirection(ConsoleKey key)
        {  // Set direction according to snakeID and the ConsoleKey
            if (key == _keySet[_snakeId, 0]) { _dir = Directions.Up;  return; }  // No need to do more checks
            if (key == _keySet[_snakeId, 1]) { _dir = Directions.Down; return; }
            if (key == _keySet[_snakeId, 2]) { _dir = Directions.Left;    return; }
            if (key == _keySet[_snakeId, 3]) { _dir = Directions.Right; }
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

        public void MoveSnake()
        {
            int linksToAdd;

            do
            {
                if (!_state.GamePaused)
                {
                    //  Find next coordinate for the head, based on the direction
                    switch (_dir)
                    {
                        case Directions.Up:
                            _nextPos = new Position(_positions.Last().XPos, _positions.Last().YPos - 1);
                            break;
                        case Directions.Down:
                            _nextPos = new Position(_positions.Last().XPos, _positions.Last().YPos + 1);
                            break;
                        case Directions.Left:
                            _nextPos = new Position(_positions.Last().XPos - 1, _positions.Last().YPos);
                            break;
                        case Directions.Right:
                            _nextPos = new Position(_positions.Last().XPos + 1, _positions.Last().YPos);
                            break;
                    }

                    // growSnake = board.HasTreat(nextPos);
                    linksToAdd =
                        _board.TreatPoints(_nextPos); // Did we hit a treat; get the number of snake links to add
                    if (linksToAdd == 0 &&
                        !_console.IsBlank(_nextPos)) // // Collided with something tha was not a treat. Game over!!
                    {
                        _snakeAlive = false;
                        break;
                    }

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
                        _state.TreatDelay = Math.Max(GameState.MinTreatDelay, _state.TreatDelay - SnakeLength() * 10);
                        _linksToBeAdded--;
                    }

                    // Now move the snake head
                    _console.WriteAt(SnakeBodyChar, _positions.Last(), SnakeBodyfgColor, ConsoleColor.Black);
                    _console.WriteAt(SnakeHeadChar, _nextPos, SnakeHeadfgColor, ConsoleColor.Black);
                    _positions.Add(_nextPos);
                }
                Thread.Sleep(_state.SnakeDelay);
            } while (_snakeAlive);
            DoPostMortem();
            RemoveSnake();
            _state.GameOver = true;
        }
    }
}
