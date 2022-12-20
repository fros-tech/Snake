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
    public void Randomize() 
    {
        XPos = MyConsole.Random.Next(0, MyConsole.ConsoleWidth);
        YPos = MyConsole.Random.Next(0, MyConsole.ConsoleHeight);
    }
}