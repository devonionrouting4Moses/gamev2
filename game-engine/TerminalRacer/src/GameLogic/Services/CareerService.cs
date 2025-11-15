using TerminalRacer.Core.Enums;
using TerminalRacer.Core.Models;
using TerminalRacer.GameLogic.Interfaces;

namespace TerminalRacer.GameLogic.Services;

public class CareerService : ICareerService
{
    private readonly List<CareerLevel> _levels = new();
    private int _currentLevel = 1;
    private float _progress;
    
    public int CurrentLevel => _currentLevel;
    public float Progress => _progress;
    public IReadOnlyList<CareerLevel> Levels => _levels.AsReadOnly();
    
    public void Initialize()
    {
        _levels.AddRange(new[]
        {
            new CareerLevel { Level = 1, Track = TrackType.Highway, Weather = Weather.Clear, 
                TargetScore = 5000, Objective = "Complete first race" },
            new CareerLevel { Level = 2, Track = TrackType.City, Weather = Weather.Clear, 
                TargetScore = 8000, Objective = "Navigate city streets" },
            new CareerLevel { Level = 3, Track = TrackType.Highway, Weather = Weather.Rain, 
                TargetScore = 10000, Objective = "Race in the rain" },
            new CareerLevel { Level = 4, Track = TrackType.Mountain, Weather = Weather.Clear, 
                TargetScore = 15000, HasBoss = true, Objective = "Defeat mountain boss" },
            new CareerLevel { Level = 5, Track = TrackType.Desert, Weather = Weather.Clear, 
                TargetScore = 20000, Objective = "Conquer the desert" },
            new CareerLevel { Level = 6, Track = TrackType.Tunnel, Weather = Weather.Night, 
                TargetScore = 25000, Objective = "Master the tunnel" },
            new CareerLevel { Level = 7, Track = TrackType.City, Weather = Weather.Fog, 
                TargetScore = 30000, HasBoss = true, Objective = "City fog boss battle" },
            new CareerLevel { Level = 8, Track = TrackType.Mountain, Weather = Weather.Rain, 
                TargetScore = 40000, HasBoss = true, Objective = "Ultimate mountain challenge" },
            new CareerLevel { Level = 9, Track = TrackType.Highway, Weather = Weather.Night, 
                TargetScore = 50000, HasBoss = true, Objective = "Final boss showdown!" },
        });
    }
    
    public void UpdateProgress(int currentScore)
    {
        var level = GetCurrentLevel();
        if (level != null)
        {
            _progress = Math.Min(100f, (currentScore / (float)level.TargetScore) * 100f);
        }
    }
    
    public bool IsLevelComplete(int currentScore)
    {
        var level = GetCurrentLevel();
        return level != null && currentScore >= level.TargetScore;
    }
    
    public CareerLevel? GetCurrentLevel()
    {
        return _currentLevel <= _levels.Count ? _levels[_currentLevel - 1] : null;
    }
    
    public CareerLevel? GetNextLevel()
    {
        return _currentLevel < _levels.Count ? _levels[_currentLevel] : null;
    }
    
    public void AdvanceLevel()
    {
        if (_currentLevel < _levels.Count)
        {
            _currentLevel++;
            _progress = 0;
        }
    }
}