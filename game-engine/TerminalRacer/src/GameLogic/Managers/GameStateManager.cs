using System.Diagnostics;
using System.Runtime.InteropServices;
using TerminalRacer.Core.Enums;
using TerminalRacer.Core.Models;
using TerminalRacer.GameLogic.Interfaces;
using TerminalRacer.Rendering.FFI;

namespace TerminalRacer.GameLogic.Managers;

public class GameStateManager
{
    private readonly ISpawnService _spawnService;
    private readonly IPowerupService _powerupService;
    private readonly IEnvironmentService _environmentService;
    private readonly GameMode _gameMode;
    private readonly TrackType _trackType;
    
    public GameStateManager(ISpawnService spawnService, IPowerupService powerupService,
                           IEnvironmentService environmentService, GameMode gameMode, 
                           TrackType trackType)
    {
        _spawnService = spawnService;
        _powerupService = powerupService;
        _environmentService = environmentService;
        _gameMode = gameMode;
        _trackType = trackType;
    }
    
    public GameState BuildGameState(Car? player1Car, Car? player2Car, 
                                   int player1Score, int player2Score,
                                   Stopwatch gameTimer, int currentLevel, 
                                   float careerProgress, (int pos, float dist)? ghost)
    {
        if (player1Car == null) return new GameState();
        
        // Prepare AI data
        var aiCars = _spawnService.AiCars;
        var aiPositions = aiCars.Select(c => c.Lane).ToArray();
        var aiDistances = aiCars.Select(c => c.Distance).ToArray();
        var aiTypes = aiCars.Select(c => (int)c.Type).ToArray();
        var aiIsBoss = aiCars.Select(c => c.IsBoss).ToArray();
        
        // Prepare obstacle data
        var obstacles = _spawnService.Obstacles;
        var obsPositions = obstacles.Select(o => o.Lane).ToArray();
        var obsDistances = obstacles.Select(o => o.Distance).ToArray();
        var obsTypes = obstacles.Select(o => (int)o.Type).ToArray();
        
        // Prepare building data
        var buildings = _spawnService.Buildings;
        var bldPositions = buildings.Select(b => b.Position).ToArray();
        var bldDistances = buildings.Select(b => b.Distance).ToArray();
        var bldHeights = buildings.Select(b => b.Height).ToArray();
        var bldTypes = buildings.Select(b => b.Type).ToArray();
        
        // Pin arrays
        var handles = new List<GCHandle>();
        
        GCHandle PinArray<T>(T[] arr)
        {
            var handle = GCHandle.Alloc(arr, GCHandleType.Pinned);
            handles.Add(handle);
            return handle;
        }
        
        var aiPosH = PinArray(aiPositions);
        var aiDistH = PinArray(aiDistances);
        var aiTypeH = PinArray(aiTypes);
        var aiBossH = PinArray(aiIsBoss);
        
        var obsPosH = PinArray(obsPositions);
        var obsDistH = PinArray(obsDistances);
        var obsTypeH = PinArray(obsTypes);
        
        var bldPosH = PinArray(bldPositions);
        var bldDistH = PinArray(bldDistances);
        var bldHeightH = PinArray(bldHeights);
        var bldTypeH = PinArray(bldTypes);
        
        var powerups = _powerupService.Powerups;
        
        var state = new GameState
        {
            // Player 1
            PlayerPosition = player1Car.Lane,
            PlayerSpeed = player1Car.Speed,
            PlayerDistance = player1Car.Distance,
            PlayerHealth = player1Car.Health,
            PlayerScore = player1Score,
            PlayerCarType = (int)player1Car.Type,
            
            // Player 2
            Player2Active = player2Car != null,
            Player2Position = player2Car?.Lane ?? 0,
            Player2Speed = player2Car?.Speed ?? 0,
            Player2Distance = player2Car?.Distance ?? 0,
            Player2Health = player2Car?.Health ?? 0,
            Player2Score = player2Score,
            Player2CarType = player2Car != null ? (int)player2Car.Type : 0,
            
            // Game state
            LapTime = (float)gameTimer.Elapsed.TotalSeconds,
            GameMode = (int)_gameMode,
            TrackType = (int)_trackType,
            Level = currentLevel,
            CareerProgress = careerProgress,
            
            // Powerups
            BoostActive = powerups["boost"].Active,
            BoostRemaining = powerups["boost"].Remaining,
            ShieldActive = powerups["shield"].Active,
            ShieldRemaining = powerups["shield"].Remaining,
            InvincibilityActive = powerups["star"].Active,
            InvincibilityRemaining = powerups["star"].Remaining,
            MagnetActive = powerups["magnet"].Active,
            MagnetRemaining = powerups["magnet"].Remaining,
            SlowmoActive = powerups["slowmo"].Active,
            SlowmoRemaining = powerups["slowmo"].Remaining,
            
            // Objects
            CarCount = aiCars.Count,
            AiPositions = aiPosH.AddrOfPinnedObject(),
            AiDistances = aiDistH.AddrOfPinnedObject(),
            AiTypes = aiTypeH.AddrOfPinnedObject(),
            AiIsBoss = aiBossH.AddrOfPinnedObject(),
            
            ObstacleCount = obstacles.Count,
            ObstaclePositions = obsPosH.AddrOfPinnedObject(),
            ObstacleDistances = obsDistH.AddrOfPinnedObject(),
            ObstacleTypes = obsTypeH.AddrOfPinnedObject(),
            
            BuildingCount = buildings.Count,
            BuildingPositions = bldPosH.AddrOfPinnedObject(),
            BuildingDistances = bldDistH.AddrOfPinnedObject(),
            BuildingHeights = bldHeightH.AddrOfPinnedObject(),
            BuildingTypes = bldTypeH.AddrOfPinnedObject(),
            
            // Environment
            Weather = (int)_environmentService.CurrentWeather,
            CurveOffset = _environmentService.TrackCurve,
            Elevation = _environmentService.Elevation,
            TunnelDarkness = _environmentService.TunnelDarkness,
            
            // Meta
            Combo = _powerupService.Combo,
            ReplayMode = _gameMode == GameMode.Replay,
            GhostPosition = ghost?.pos ?? 0,
            GhostDistance = ghost?.dist ?? 0,
        };
        
        // Free handles after render
        Task.Run(async () =>
        {
            await Task.Delay(100);
            foreach (var h in handles) h.Free();
        });
        
        return state;
    }
}