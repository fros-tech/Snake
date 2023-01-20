namespace Snake;

public class Position
{
    public int XPos { get; set; }
    public int YPos { get; set; }

    public Position() { } 

    public Position(int x, int y)
    {
        XPos = x;
        YPos = y;
    }
}