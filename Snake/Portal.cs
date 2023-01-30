namespace Snake;

public class Portal
{
    public const char PortalChar = '@';

    public ConsoleColor fgColor { get; set; } = ConsoleColor.Black;
    public ConsoleColor bgColor { get; set; } = ConsoleColor.White;

    public Position Position { get; set; }
    public long lifeTime { get; set; } // milliseconds

    private Portal(Position position, char character, int numPoints, ConsoleColor fgc, ConsoleColor bgc)
    {
        Position = position;
        lifeTime = 0;
    }
    
    public Position GetPosition() { return Position; }
}