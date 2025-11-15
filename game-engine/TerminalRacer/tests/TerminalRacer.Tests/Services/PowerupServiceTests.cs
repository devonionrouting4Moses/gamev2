using Xunit;
using FluentAssertions;
using TerminalRacer.Core.Constants;
using TerminalRacer.GameLogic.Services;

namespace TerminalRacer.Tests.Services;

public class PowerupServiceTests
{
    [Fact]
    public void Initialize_CreatesAllPowerups()
    {
        // Arrange
        var service = new PowerupService();
        
        // Act
        service.Initialize();
        
        // Assert
        service.Powerups.Should().ContainKeys("boost", "shield", "star", "magnet", "slowmo");
        service.Powerups.Values.Should().OnlyContain(p => !p.Active && p.Remaining == 0);
    }
    
    [Fact]
    public void ActivatePowerup_SetsPowerupActive()
    {
        // Arrange
        var service = new PowerupService();
        service.Initialize();
        
        // Act
        service.ActivatePowerup("boost", 3.0f);
        
        // Assert
        service.IsBoostActive.Should().BeTrue();
        service.BoostRemaining.Should().Be(3.0f);
    }
    
    [Fact]
    public void Update_ReducesPowerupTime()
    {
        // Arrange
        var service = new PowerupService();
        service.Initialize();
        service.ActivatePowerup("boost", 3.0f);
        
        // Act
        service.Update(1.0f);
        
        // Assert
        service.BoostRemaining.Should().Be(2.0f);
        service.IsBoostActive.Should().BeTrue();
    }
    
    [Fact]
    public void Update_DeactivatesPowerupWhenExpired()
    {
        // Arrange
        var service = new PowerupService();
        service.Initialize();
        service.ActivatePowerup("boost", 1.0f);
        
        // Act
        service.Update(1.5f);
        
        // Assert
        service.IsBoostActive.Should().BeFalse();
        service.BoostRemaining.Should().Be(0);
    }
    
    [Fact]
    public void IncrementCombo_IncreasesComboCounter()
    {
        // Arrange
        var service = new PowerupService();
        service.Initialize();
        
        // Act
        service.IncrementCombo();
        service.IncrementCombo();
        service.IncrementCombo();
        
        // Assert
        service.Combo.Should().Be(3);
    }
    
    [Fact]
    public void ResetCombo_ClearsCombo()
    {
        // Arrange
        var service = new PowerupService();
        service.Initialize();
        service.IncrementCombo();
        service.IncrementCombo();
        
        // Act
        service.ResetCombo();
        
        // Assert
        service.Combo.Should().Be(0);
    }
    
    [Fact]
    public void Update_ResetsComboAfterTimeout()
    {
        // Arrange
        var service = new PowerupService();
        service.Initialize();
        service.IncrementCombo();
        
        // Act
        service.Update(GameConstants.ComboDuration + 0.1f);
        
        // Assert
        service.Combo.Should().Be(0);
    }
    
    [Fact]
    public void AddBoostCharge_IncreasesBoostRemaining()
    {
        // Arrange
        var service = new PowerupService();
        service.Initialize();
        
        // Act
        service.AddBoostCharge(50);
        
        // Assert
        service.BoostRemaining.Should().Be(50);
    }
    
    [Fact]
    public void AddBoostCharge_CapsAtMaximum()
    {
        // Arrange
        var service = new PowerupService();
        service.Initialize();
        
        // Act
        service.AddBoostCharge(300);
        
        // Assert
        service.BoostRemaining.Should().Be(GameConstants.MaxBoostCharge);
    }
    
    [Fact]
    public void TryActivateBoost_ActivatesWhenChargeAvailable()
    {
        // Arrange
        var service = new PowerupService();
        service.Initialize();
        service.AddBoostCharge(100);
        
        // Act
        var result = service.TryActivateBoost();
        
        // Assert
        result.Should().BeTrue();
        service.IsBoostActive.Should().BeTrue();
    }
    
    [Fact]
    public void TryActivateBoost_FailsWhenNoCharge()
    {
        // Arrange
        var service = new PowerupService();
        service.Initialize();
        
        // Act
        var result = service.TryActivateBoost();
        
        // Assert
        result.Should().BeFalse();
        service.IsBoostActive.Should().BeFalse();
    }
}