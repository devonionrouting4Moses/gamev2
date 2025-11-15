using Xunit;
using FluentAssertions;
using TerminalRacer.Core.Models;
using TerminalRacer.Core.Enums;

namespace TerminalRacer.Tests.Models;

public class CarTests
{
    [Fact]
    public void Constructor_InitializesPropertiesCorrectly()
    {
        // Act
        var car = new Car(1, 50.0f, 100.0f, true, CarType.Sports, false);
        
        // Assert
        car.Lane.Should().Be(1);
        car.Distance.Should().Be(50.0f);
        car.Speed.Should().Be(100.0f);
        car.IsPlayer.Should().BeTrue();
        car.Type.Should().Be(CarType.Sports);
        car.IsBoss.Should().BeFalse();
        car.Health.Should().Be(100);
    }
    
    [Fact]
    public void Update_IncreasesDistanceBasedOnSpeed()
    {
        // Arrange
        var car = new Car(1, 0, 100);
        
        // Act
        car.Update(1.0f);
        
        // Assert
        car.Distance.Should().Be(100.0f);
    }
    
    [Fact]
    public void Update_WithHalfSecond_MovesHalfDistance()
    {
        // Arrange
        var car = new Car(1, 0, 100);
        
        // Act
        car.Update(0.5f);
        
        // Assert
        car.Distance.Should().Be(50.0f);
    }
    
    [Fact]
    public void IsAlive_WhenHealthAboveZero_ReturnsTrue()
    {
        // Arrange
        var car = new Car(1, 0, 0) { Health = 50 };
        
        // Assert
        car.IsAlive.Should().BeTrue();
    }
    
    [Fact]
    public void IsAlive_WhenHealthIsZero_ReturnsFalse()
    {
        // Arrange
        var car = new Car(1, 0, 0) { Health = 0 };
        
        // Assert
        car.IsAlive.Should().BeFalse();
    }
    
    [Fact]
    public void IsAlive_WhenHealthBelowZero_ReturnsFalse()
    {
        // Arrange
        var car = new Car(1, 0, 0) { Health = -10 };
        
        // Assert
        car.IsAlive.Should().BeFalse();
    }
}