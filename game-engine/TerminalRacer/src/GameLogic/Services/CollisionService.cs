using TerminalRacer.Audio.Interfaces;
using TerminalRacer.Core.Constants;
using TerminalRacer.Core.Enums;
using TerminalRacer.Core.Models;
using TerminalRacer.GameLogic.Interfaces;

namespace TerminalRacer.GameLogic.Services;

public class CollisionService : ICollisionService
{
    private readonly IAudioManager _audio;
    private readonly IPowerupService _powerupService;
    
    public CollisionService(IAudioManager audio, IPowerupService powerupService)
    {
        _audio = audio;
        _powerupService = powerupService;
    }
    
    public void CheckCollisions(Car playerCar, ref int score, 
                               IReadOnlyList<Car> aiCars, 
                               IReadOnlyList<Obstacle> obstacles)
    {
        CheckCarCollisions(playerCar, aiCars);
        CheckObstacleCollisions(playerCar, ref score, obstacles);
    }
    
    private void CheckCarCollisions(Car playerCar, IReadOnlyList<Car> aiCars)
    {
        foreach (var ai in aiCars)
        {
            if (ai.Lane == playerCar.Lane && 
                Math.Abs(ai.Distance - playerCar.Distance) < GameConstants.CarCollisionDistance)
            {
                if (!_powerupService.IsInvincible)
                {
                    playerCar.Health = Math.Max(0, playerCar.Health - GameConstants.CarCollisionDamage);
                    _powerupService.ResetCombo();
                    _audio.PlaySound("sounds/crash.wav");
                }
                
                playerCar.Speed *= GameConstants.CarCollisionSpeedMultiplier;
                if (ai.Distance > playerCar.Distance) 
                    ai.Distance += 10;
            }
        }
    }
    
    private void CheckObstacleCollisions(Car playerCar, ref int score, 
                                        IReadOnlyList<Obstacle> obstacles)
    {
        foreach (var obs in obstacles.Where(o => !o.Collected && o.Lane == playerCar.Lane))
        {
            if (Math.Abs(obs.Distance - playerCar.Distance) < GameConstants.ObstacleCollisionDistance)
            {
                obs.Collected = true;
                HandleObstacleCollection(obs, playerCar, ref score);
            }
        }
    }
    
    private void HandleObstacleCollection(Obstacle obs, Car car, ref int score)
    {
        switch (obs.Type)
        {
            case ObstacleType.Cone:
                if (!_powerupService.IsInvincible)
                {
                    car.Health = Math.Max(0, car.Health - GameConstants.ConeCollisionDamage);
                    car.Speed *= GameConstants.ConeCollisionSpeedMultiplier;
                    _powerupService.ResetCombo();
                    _audio.PlaySound("sounds/hit.wav");
                }
                break;
                
            case ObstacleType.Oil:
                if (!_powerupService.IsInvincible)
                {
                    car.Speed *= GameConstants.OilSlickSpeedMultiplier;
                    _powerupService.ResetCombo();
                }
                break;
                
            case ObstacleType.Boost:
                _powerupService.AddBoostCharge(100);
                score += 100 * (_powerupService.Combo + 1);
                _powerupService.IncrementCombo();
                _audio.PlaySound("sounds/powerup.wav");
                break;
                
            case ObstacleType.Star:
                _powerupService.ActivatePowerup("star", GameConstants.StarDuration);
                score += 200 * (_powerupService.Combo + 1);
                _powerupService.IncrementCombo();
                _audio.PlaySound("sounds/star.wav");
                break;
                
            case ObstacleType.Magnet:
                _powerupService.ActivatePowerup("magnet", GameConstants.MagnetDuration);
                score += 150 * (_powerupService.Combo + 1);
                _powerupService.IncrementCombo();
                _audio.PlaySound("sounds/magnet.wav");
                break;
                
            case ObstacleType.Clock:
                _powerupService.ActivatePowerup("slowmo", GameConstants.SlowmoDuration);
                score += 250 * (_powerupService.Combo + 1);
                _powerupService.IncrementCombo();
                _audio.PlaySound("sounds/slowmo.wav");
                break;
        }
    }
}