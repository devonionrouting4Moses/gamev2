using TerminalRacer.Core.Enums;

namespace TerminalRacer.Core.Models;

public class Obstacle
{
    public int Lane { get; set; }
    public float Distance { get; set; }
    public ObstacleType Type { get; set; }
    public bool Collected { get; set; }
    
    public Obstacle(int lane, float distance, ObstacleType type)
    {
        Lane = lane;
        Distance = distance;
        Type = type;
    }
}