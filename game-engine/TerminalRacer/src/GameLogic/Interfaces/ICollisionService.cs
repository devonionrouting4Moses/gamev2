using TerminalRacer.Core.Models;

namespace TerminalRacer.GameLogic.Interfaces;

public interface ICollisionService
{
    void CheckCollisions(Car playerCar, ref int score, 
                        IReadOnlyList<Car> aiCars, 
                        IReadOnlyList<Obstacle> obstacles);
}