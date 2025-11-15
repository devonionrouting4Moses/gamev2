using Xunit;
using FluentAssertions;
using Moq;
using TerminalRacer.Core.Models;
using TerminalRacer.Core.Enums;
using TerminalRacer.GameLogic.Services;
using TerminalRacer.GameLogic.Interfaces;
using TerminalRacer.Audio.Interfaces;
using TerminalRacer.Core.Constants;

namespace TerminalRacer.Tests.Services;

[Collection("Test Collection")]
public class CollisionServiceTests
{
    private readonly Mock<IAudioManager> _audioMock;
    private readonly Mock<IPowerupService> _powerupMock;
    private readonly CollisionService _sut;
    
    public CollisionServiceTests()
    {
        _audioMock = new Mock<IAudioManager>();
        _powerupMock = new Mock<IPowerupService>();
        _sut = new CollisionService(_audioMock.Object, _powerupMock.Object);
    }
    
    [Fact]
    public void CheckCollisions_WhenCarCollidesWithAI_ReducesHealth()
    {
        // Arrange
        var playerCar = new Car(1, 100, 50, true);
        var aiCar = new Car(1, 102, 40); // Same lane, close distance
        int score = 0;
        
        _powerupMock.Setup(x => x.IsInvincible).Returns(false);
        
        // Act
        _sut.CheckCollisions(playerCar, ref score, new[] { aiCar }, Array.Empty<Obstacle>());
        
        // Assert
        playerCar.Health.Should().Be(100 - GameConstants.CarCollisionDamage);
        _powerupMock.Verify(x => x.ResetCombo(), Times.Once);
        _audioMock.Verify(x => x.PlaySound("sounds/crash.wav", It.IsAny<float>()), Times.Once);
    }
    
    [Fact]
    public void CheckCollisions_WhenInvincible_DoesNotTakeDamage()
    {
        // Arrange
        var playerCar = new Car(1, 100, 50, true) { Health = 100 };
        var aiCar = new Car(1, 102, 40);
        int score = 0;
        
        _powerupMock.Setup(x => x.IsInvincible).Returns(true);
        
        // Act
        _sut.CheckCollisions(playerCar, ref score, new[] { aiCar }, Array.Empty<Obstacle>());
        
        // Assert
        playerCar.Health.Should().Be(100);
        _powerupMock.Verify(x => x.ResetCombo(), Times.Never);
    }
    
    [Fact]
    public void CheckCollisions_WhenCollectingBoost_IncreasesScore()
    {
        // Arrange
        var playerCar = new Car(1, 100, 50, true);
        var obstacle = new Obstacle(1, 102, ObstacleType.Boost);
        int score = 1000;
        
        _powerupMock.Setup(x => x.Combo).Returns(2);
        
        // Act
        _sut.CheckCollisions(playerCar, ref score, Array.Empty<Car>(), new[] { obstacle });
        
        // Assert
        score.Should().BeGreaterThan(1000);
        obstacle.Collected.Should().BeTrue();
        _powerupMock.Verify(x => x.AddBoostCharge(100), Times.Once);
        _powerupMock.Verify(x => x.IncrementCombo(), Times.Once);
    }
    
    [Fact]
    public void CheckCollisions_WhenHittingCone_TakesDamage()
    {
        // Arrange
        var playerCar = new Car(1, 100, 50, true);
        var obstacle = new Obstacle(1, 102, ObstacleType.Cone);
        int score = 0;
        
        _powerupMock.Setup(x => x.IsInvincible).Returns(false);
        
        // Act
        _sut.CheckCollisions(playerCar, ref score, Array.Empty<Car>(), new[] { obstacle });
        
        // Assert
        playerCar.Health.Should().Be(100 - GameConstants.ConeCollisionDamage);
        playerCar.Speed.Should().BeLessThan(50);
        _powerupMock.Verify(x => x.ResetCombo(), Times.Once);
    }
    
    [Fact]
    public void CheckCollisions_WhenCollectingStar_ActivatesInvincibility()
    {
        // Arrange
        var playerCar = new Car(1, 100, 50, true);
        var obstacle = new Obstacle(1, 102, ObstacleType.Star);
        int score = 0;
        
        // Act
        _sut.CheckCollisions(playerCar, ref score, Array.Empty<Car>(), new[] { obstacle });
        
        // Assert
        _powerupMock.Verify(x => x.ActivatePowerup("star", GameConstants.StarDuration), Times.Once);
        _audioMock.Verify(x => x.PlaySound("sounds/star.wav", 0.5f), Times.Once);
    }
    
    [Fact]
    public void CheckCollisions_WhenHittingOil_ReducesSpeed()
    {
        // Arrange
        var playerCar = new Car(1, 100, 50, true);
        var obstacle = new Obstacle(1, 102, ObstacleType.Oil);
        int score = 0;
        
        _powerupMock.Setup(x => x.IsInvincible).Returns(false);
        
        // Act
        _sut.CheckCollisions(playerCar, ref score, Array.Empty<Car>(), new[] { obstacle });
        
        // Assert
        playerCar.Speed.Should().Be(50 * GameConstants.OilSlickSpeedMultiplier);
        _powerupMock.Verify(x => x.ResetCombo(), Times.Once);
    }
}