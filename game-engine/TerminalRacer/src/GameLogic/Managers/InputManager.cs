using TerminalRacer.Audio.Interfaces;
using TerminalRacer.Core.Constants;
using TerminalRacer.Core.Enums;
using TerminalRacer.Core.Models;
using TerminalRacer.GameLogic.Interfaces;

namespace TerminalRacer.GameLogic.Managers;

public class InputManager
{
    private readonly IAudioManager _audio;
    private readonly IPowerupService _powerupService;
    
    public InputManager(IAudioManager audio, IPowerupService powerupService)
    {
        _audio = audio;
        _powerupService = powerupService;
    }
    
    public void HandlePlayerInput(Car car, ref int score, bool left, bool right, 
                                 bool accel, bool brake, bool boost, 
                                 Weather weather, float deltaTime)
    {
        // Lane changes
        if (left && car.Lane > 0) car.Lane--;
        if (right && car.Lane < GameConstants.NumLanes - 1) car.Lane++;
        
        // Speed control
        var maxSpeed = _powerupService.IsBoostActive ? GameConstants.BoostSpeed : GameConstants.MaxSpeed;
        
        if (accel) 
            car.Speed = Math.Min(car.Speed + GameConstants.Acceleration * deltaTime, maxSpeed);
        else if (brake) 
            car.Speed = Math.Max(car.Speed - GameConstants.BrakeForce * deltaTime, 0);
        else 
            car.Speed = Math.Max(car.Speed - GameConstants.Deceleration * deltaTime, 0);
        
        // Weather effects
        if (weather == Weather.Rain) 
            car.Speed *= GameConstants.RainSpeedMultiplier;
        if (weather == Weather.Fog) 
            car.Speed *= GameConstants.FogSpeedMultiplier;
        
        // Boost activation
        if (boost && _powerupService.TryActivateBoost())
        {
            _audio.PlaySound("sounds/boost.wav");
        }
        
        car.Update(deltaTime);
        score += (int)(car.Speed * deltaTime * 0.1f);
    }
}