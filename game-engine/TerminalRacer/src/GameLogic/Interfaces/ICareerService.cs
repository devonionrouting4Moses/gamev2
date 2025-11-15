using TerminalRacer.Core.Models;

namespace TerminalRacer.GameLogic.Interfaces;

public interface ICareerService
{
    void Initialize();
    void UpdateProgress(int currentScore);
    bool IsLevelComplete(int currentScore);
    CareerLevel? GetCurrentLevel();
    CareerLevel? GetNextLevel();
    void AdvanceLevel();
    
    int CurrentLevel { get; }
    float Progress { get; }
    IReadOnlyList<CareerLevel> Levels { get; }
}