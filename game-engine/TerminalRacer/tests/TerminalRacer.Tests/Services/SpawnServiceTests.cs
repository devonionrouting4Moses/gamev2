using Xunit;
using FluentAssertions;
using TerminalRacer.Core.Models;
using TerminalRacer.Core.Enums;
using TerminalRacer.GameLogic.Services;

namespace TerminalRacer.Tests.Services;

public class SpawnServiceTests
{
    [Fact]
    public void SpawnAiCar_WhenCalled_AddsCarToList()
    {
        // Arrange
        var playerCar = new Car(1, 0, 0, true);
        var service = new SpawnService(
            playerCar, 
            GameMode.Single, 
            TrackType.Highway, 
            new List<CareerLevel>(), 
            1
        );
        
        // Act
        service.SpawnAiCar();
        
        // Assert
        service.AiCars.Should().HaveCount(1);
        service.AiCars.First().IsPlayer.Should().BeFalse();
    }
    
    [Fact]
    public void SpawnAiCar_InCareerModeWithBoss_SpawnsBossFirst()
    {
        // Arrange
        var playerCar = new Car(1, 0, 0, true);
        var careerLevels = new List<CareerLevel>
        {
            new CareerLevel { Level = 1, HasBoss = true, Track = TrackType.Highway }
        };
        
        var service = new SpawnService(
            playerCar, 
            GameMode.Career, 
            TrackType.Highway, 
            careerLevels, 
            1
        );
        
        // Act
        service.SpawnAiCar();
        
        // Assert
        service.AiCars.Should().HaveCount(1);
        service.AiCars.First().IsBoss.Should().BeTrue();
        service.AiCars.First().Type.Should().Be(CarType.Limo);
    }
    
    [Fact]
    public void SpawnObstacle_WhenCalled_AddsObstacleToList()
    {
        // Arrange
        var playerCar = new Car(1, 0, 0, true);
        var service = new SpawnService(
            playerCar, 
            GameMode.Single, 
            TrackType.Highway, 
            new List<CareerLevel>(), 
            1
        );
        
        // Act
        service.SpawnObstacle();
        
        // Assert
        service.Obstacles.Should().HaveCount(1);
    }
    
    [Fact]
    public void SpawnBuilding_WhenCalledInCityMode_AddsBuilding()
    {
        // Arrange
        var playerCar = new Car(1, 0, 0, true);
        var service = new SpawnService(
            playerCar, 
            GameMode.Single, 
            TrackType.City, 
            new List<CareerLevel>(), 
            1
        );
        
        // Act
        service.SpawnBuilding();
        
        // Assert
        service.Buildings.Should().HaveCount(1);
    }
    
    [Fact]
    public void UpdateSpawning_WhenPlayerMovesForward_SpawnsNewObjects()
    {
        // Arrange
        var playerCar = new Car(1, 0, 0, true);
        var service = new SpawnService(
            playerCar, 
            GameMode.Single, 
            TrackType.Highway, 
            new List<CareerLevel>(), 
            1
        );
        
        service.SpawnInitialContent();
        var initialCarCount = service.AiCars.Count;
        
        // Act
        service.UpdateSpawning(200f); // Player moved forward
        
        // Assert
        service.AiCars.Count.Should().BeGreaterThan(initialCarCount);
    }
    
    [Fact]
    public void Cleanup_RemovesObjectsBehindPlayer()
    {
        // Arrange
        var playerCar = new Car(1, 200, 50, true);
        var service = new SpawnService(
            playerCar, 
            GameMode.Single, 
            TrackType.Highway, 
            new List<CareerLevel>(), 
            1
        );
        
        service.SpawnInitialContent();
        var initialCount = service.AiCars.Count;
        
        // Act
        service.Cleanup(500f); // Player far ahead
        
        // Assert
        service.AiCars.Count.Should().BeLessThan(initialCount);
    }
    
    [Fact]
    public void ClearAll_RemovesAllObjects()
    {
        // Arrange
        var playerCar = new Car(1, 0, 0, true);
        var service = new SpawnService(
            playerCar, 
            GameMode.Single, 
            TrackType.Highway, 
            new List<CareerLevel>(), 
            1
        );
        
        service.SpawnInitialContent();
        
        // Act
        service.ClearAll();
        
        // Assert
        service.AiCars.Should().BeEmpty();
        service.Obstacles.Should().BeEmpty();
        service.Buildings.Should().BeEmpty();
    }
}