using TerminalRacer.Core.Constants;
using TerminalRacer.Core.Models;
using TerminalRacer.GameLogic.Interfaces;

namespace TerminalRacer.GameLogic.Services;

public class PowerupService : IPowerupService
{
    private readonly Dictionary<string, PowerupState> _powerups = new();
    private int _combo;
    private float _comboTimer;
    
    public bool IsBoostActive => _powerups["boost"].Active;
    public bool IsInvincible => _powerups["star"].Active;
    public int Combo => _combo;
    public float BoostRemaining => _powerups["boost"].Remaining;
    
    public IReadOnlyDictionary<string, PowerupState> Powerups => _powerups;
    
    public void Initialize()
    {
        _powerups["boost"] = new PowerupState();
        _powerups["shield"] = new PowerupState();
        _powerups["star"] = new PowerupState();
        _powerups["magnet"] = new PowerupState();
        _powerups["slowmo"] = new PowerupState();
    }
    
    public void Update(float deltaTime)
    {
        var keys = _powerups.Keys.ToList();
        foreach (var key in keys)
        {
            var state = _powerups[key];
            
            if (state.Active)
            {
                state.Remaining -= deltaTime;
                if (state.Remaining <= 0)
                {
                    state.Active = false;
                    state.Remaining = 0;
                }
            }
            
            if (state.Cooldown > 0)
            {
                state.Cooldown -= deltaTime;
            }
        }
        
        _comboTimer -= deltaTime;
        if (_comboTimer <= 0) 
            _combo = 0;
    }
    
    public void ActivatePowerup(string name, float duration)
    {
        if (_powerups.ContainsKey(name))
        {
            _powerups[name].Active = true;
            _powerups[name].Remaining = duration;
        }
    }
    
    public void AddBoostCharge(int amount)
    {
        var boost = _powerups["boost"];
        boost.Remaining = Math.Min(boost.Remaining + amount, GameConstants.MaxBoostCharge);
    }
    
    public bool TryActivateBoost()
    {
        var boost = _powerups["boost"];
        if (boost.Remaining > 0 && !boost.Active)
        {
            ActivatePowerup("boost", GameConstants.BoostDuration);
            return true;
        }
        return false;
    }
    
    public void IncrementCombo()
    {
        _combo++;
        _comboTimer = GameConstants.ComboDuration;
    }
    
    public void ResetCombo()
    {
        _combo = 0;
        _comboTimer = 0;
    }
}