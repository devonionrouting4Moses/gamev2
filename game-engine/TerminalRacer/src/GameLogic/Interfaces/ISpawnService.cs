using TerminalRacer.Core.Models;

namespace TerminalRacer.GameLogic.Interfaces;

public interface ISpawnService
{
    void SpawnInitialContent();
    void SpawnAiCar();
    void SpawnObstacle();
    void SpawnBuilding();
    void UpdateSpawning(float playerDistance);
    void Cleanup(float playerDistance);
    void ClearAll();
    
    IReadOnlyList<Car> AiCars { get; }
    IReadOnlyList<Obstacle> Obstacles { get; }
    IReadOnlyList<Building> Buildings { get; }
}