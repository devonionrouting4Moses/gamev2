using TerminalRacer.Core.Constants;
using TerminalRacer.Core.Enums;
using TerminalRacer.Core.Models;
using TerminalRacer.GameLogic.Interfaces;

namespace TerminalRacer.GameLogic.Services;

public class SpawnService : ISpawnService
{
    private readonly List<Car> _aiCars = new();
    private readonly List<Obstacle> _obstacles = new();
    private readonly List<Building> _buildings = new();
    private readonly Random _random = new();
    
    private float _nextAiSpawn;
    private float _nextObstacleSpawn;
    private float _nextBuildingSpawn;
    
    private readonly Car? _playerCar;
    private readonly GameMode _gameMode;
    private readonly TrackType _trackType;
    private readonly List<CareerLevel> _careerLevels;
    private readonly int _currentLevel;
    
    public IReadOnlyList<Car> AiCars => _aiCars.AsReadOnly();
    public IReadOnlyList<Obstacle> Obstacles => _obstacles.AsReadOnly();
    public IReadOnlyList<Building> Buildings => _buildings.AsReadOnly();
    
    public SpawnService(Car? playerCar, GameMode gameMode, TrackType trackType, 
                       List<CareerLevel> careerLevels, int currentLevel)
    {
        _playerCar = playerCar;
        _gameMode = gameMode;
        _trackType = trackType;
        _careerLevels = careerLevels;
        _currentLevel = currentLevel;
    }
    
    public void SpawnInitialContent()
    {
        _nextAiSpawn = 50f;
        _nextObstacleSpawn = 30f;
        _nextBuildingSpawn = 40f;
        
        for (int i = 0; i < GameConstants.InitialSpawnCount; i++)
        {
            SpawnAiCar();
            SpawnObstacle();
            
            if (_trackType == TrackType.City)
                SpawnBuilding();
        }
    }
    
    public void SpawnAiCar()
    {
        var lane = _random.Next(GameConstants.NumLanes);
        var distance = (_playerCar?.Distance ?? 0) + _random.Next(40, 100);
        var speed = _random.Next(60, 140);
        
        var type = CarType.Sports;
        var isBoss = false;
        
        if (_gameMode == GameMode.Career && 
            _currentLevel <= _careerLevels.Count &&
            _careerLevels[_currentLevel - 1].HasBoss && 
            _aiCars.Count == 0)
        {
            isBoss = true;
            type = CarType.Limo;
            speed = 180;
        }
        else
        {
            var roll = _random.Next(100);
            type = roll < 15 ? CarType.Police : 
                   roll < 30 ? CarType.Racer : 
                   (CarType)_random.Next(9);
            
            speed = type switch
            {
                CarType.Police => _random.Next(120, 160),
                CarType.Racer => _random.Next(140, 180),
                _ => speed
            };
        }
        
        _aiCars.Add(new Car(lane, distance, speed, false, type, isBoss));
    }
    
    public void SpawnObstacle()
    {
        var lane = _random.Next(GameConstants.NumLanes);
        var distance = (_playerCar?.Distance ?? 0) + _random.Next(50, 120);
        
        var type = ObstacleType.Cone;
        var roll = _random.Next(100);
        
        if (roll < 15) type = ObstacleType.Boost;
        else if (roll < 30) type = ObstacleType.Oil;
        else if (roll < 40) type = ObstacleType.Star;
        else if (roll < 50) type = ObstacleType.Magnet;
        else if (roll < 60) type = ObstacleType.Clock;
        
        _obstacles.Add(new Obstacle(lane, distance, type));
    }
    
    public void SpawnBuilding()
    {
        var position = _random.Next(2) == 0 ? -1 : 1;
        var distance = (_playerCar?.Distance ?? 0) + _random.Next(60, 150);
        var height = _random.Next(5, 15);
        var type = _random.Next(4);
        
        _buildings.Add(new Building(position, distance, height, type));
    }
    
    public void UpdateSpawning(float playerDistance)
    {
        if (playerDistance > _nextAiSpawn)
        {
            SpawnAiCar();
            _nextAiSpawn = playerDistance + _random.Next(30, 60);
        }
        
        if (playerDistance > _nextObstacleSpawn)
        {
            SpawnObstacle();
            _nextObstacleSpawn = playerDistance + _random.Next(40, 80);
        }
        
        if (_trackType == TrackType.City && playerDistance > _nextBuildingSpawn)
        {
            SpawnBuilding();
            _nextBuildingSpawn = playerDistance + _random.Next(50, 100);
        }
    }
    
    public void Cleanup(float playerDistance)
    {
        _aiCars.RemoveAll(c => c.Distance < playerDistance - 120);
        _obstacles.RemoveAll(o => o.Distance < playerDistance - 100 || o.Collected);
        _buildings.RemoveAll(b => b.Distance < playerDistance - 150);
    }
    
    public void ClearAll()
    {
        _aiCars.Clear();
        _obstacles.Clear();
        _buildings.Clear();
    }
}