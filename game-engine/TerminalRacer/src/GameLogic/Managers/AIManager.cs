using TerminalRacer.Core.Constants;
using TerminalRacer.Core.Models;

namespace TerminalRacer.GameLogic.Managers;

public class AIManager
{
    private readonly Random _random = new();
    
    public void UpdateAI(IReadOnlyList<Car> aiCars, float deltaTime)
    {
        foreach (var car in aiCars)
        {
            // Random lane changes
            if (_random.NextDouble() < 0.002)
            {
                var newLane = car.Lane + (_random.Next(2) == 0 ? -1 : 1);
                if (newLane >= 0 && newLane < GameConstants.NumLanes) 
                    car.Lane = newLane;
            }
            
            car.Update(deltaTime);
        }
    }
}