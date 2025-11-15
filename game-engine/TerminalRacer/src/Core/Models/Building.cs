namespace TerminalRacer.Core.Models;

public class Building
{
    public int Position { get; set; }  // -1 left, 1 right
    public float Distance { get; set; }
    public int Height { get; set; }
    public int Type { get; set; }
    
    public Building(int position, float distance, int height, int type)
    {
        Position = position;
        Distance = distance;
        Height = height;
        Type = type;
    }
}