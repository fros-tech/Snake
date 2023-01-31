namespace Snake;

public class Portal
{
    public const char PortalChar = '@';

    public ConsoleColor fgColor { get; set; } = ConsoleColor.Black;
    public ConsoleColor bgColor { get; set; } = ConsoleColor.White;

    public Position Position { get; set; }
    public long lifeTime { get; set; } // milliseconds

    public Portal(Position position)
    {
        Position = position;
        lifeTime = 0;
    }
    
    public Position GetPosition() { return Position; }
}