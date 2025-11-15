using TerminalRacer.Core.Models;

namespace TerminalRacer.GameLogic.Interfaces;

public interface IPowerupService
{
    void Initialize();
    void Update(float deltaTime);
    void ActivatePowerup(string name, float duration);
    void AddBoostCharge(int amount);
    bool TryActivateBoost();
    void IncrementCombo();
    void ResetCombo();
    
    bool IsBoostActive { get; }
    bool IsInvincible { get; }
    int Combo { get; }
    float BoostRemaining { get; }
    
    IReadOnlyDictionary<string, PowerupState> Powerups { get; }
}