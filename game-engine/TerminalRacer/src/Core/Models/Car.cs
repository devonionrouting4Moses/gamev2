using TerminalRacer.Core.Enums;

namespace TerminalRacer.Core.Models;

public class Car
{
    public int Lane { get; set; }
    public float Distance { get; set; }
    public float Speed { get; set; }
    public bool IsPlayer { get; set; }
    public CarType Type { get; set; }
    public bool IsBoss { get; set; }
    public int Health { get; set; } = 100;
    
    public Car(int lane, float distance, float speed, bool isPlayer = false, 
               CarType type = CarType.Sports, bool isBoss = false)
    {
        Lane = lane;
        Distance = distance;
        Speed = speed;
        IsPlayer = isPlayer;
        Type = type;
        IsBoss = isBoss;
    }
    
    public void Update(float deltaTime)
    {
        Distance += Speed * deltaTime;
    }
    
    public bool IsAlive => Health > 0;
}