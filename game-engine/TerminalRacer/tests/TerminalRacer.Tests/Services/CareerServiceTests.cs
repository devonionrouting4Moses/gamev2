 using Xunit;
using FluentAssertions;
using TerminalRacer.GameLogic.Services;

namespace TerminalRacer.Tests.Services;

public class CareerServiceTests
{
    [Fact]
    public void Initialize_CreatesAllLevels()
    {
        // Arrange
        var service = new CareerService();
        
        // Act
        service.Initialize();
        
        // Assert
        service.Levels.Should().HaveCount(9);
        service.CurrentLevel.Should().Be(1);
    }
    
    [Fact]
    public void UpdateProgress_CalculatesCorrectPercentage()
    {
        // Arrange
        var service = new CareerService();
        service.Initialize();
        var level = service.GetCurrentLevel();
        
        // Act
        service.UpdateProgress(level!.TargetScore / 2);
        
        // Assert
        service.Progress.Should().BeApproximately(50f, 0.1f);
    }
    
    [Fact]
    public void IsLevelComplete_ReturnsTrueWhenTargetReached()
    {
        // Arrange
        var service = new CareerService();
        service.Initialize();
        var level = service.GetCurrentLevel();
        
        // Act
        var result = service.IsLevelComplete(level!.TargetScore);
        
        // Assert
        result.Should().BeTrue();
    }
    
    [Fact]
    public void AdvanceLevel_MovesToNextLevel()
    {
        // Arrange
        var service = new CareerService();
        service.Initialize();
        
        // Act
        service.AdvanceLevel();
        
        // Assert
        service.CurrentLevel.Should().Be(2);
        service.Progress.Should().Be(0);
    }
    
    [Fact]
    public void GetNextLevel_ReturnsCorrectLevel()
    {
        // Arrange
        var service = new CareerService();
        service.Initialize();
        
        // Act
        var nextLevel = service.GetNextLevel();
        
        // Assert
        nextLevel.Should().NotBeNull();
        nextLevel!.Level.Should().Be(2);
    }
}