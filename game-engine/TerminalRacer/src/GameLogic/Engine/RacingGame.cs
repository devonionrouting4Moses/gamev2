using System.Diagnostics;
using TerminalRacer.Audio.Interfaces;
using TerminalRacer.Core.Constants;
using TerminalRacer.Core.Enums;
using TerminalRacer.Core.Models;
using TerminalRacer.GameLogic.Interfaces;
using TerminalRacer.GameLogic.Managers;
using TerminalRacer.GameLogic.Services;
using TerminalRacer.Networking.Interfaces;
using TerminalRacer.Rendering.FFI;
using TerminalRacer.Rendering.Interfaces;

namespace TerminalRacer.GameLogic.Engine;

public class RacingGame
{
    // Core dependencies
    private readonly IRenderer _renderer;
    private readonly IAudioManager _audio;
    private readonly IMultiplayerManager? _multiplayer;
    
    // Services
    private readonly ISpawnService _spawnService;
    private readonly ICollisionService _collisionService;
    private readonly IPowerupService _powerupService;
    private readonly IEnvironmentService _environmentService;
    private readonly ICareerService? _careerService;
    private readonly IReplayService _replayService;
    private readonly IScoreService _scoreService;
    
    // Managers
    private readonly AIManager _aiManager;
    private readonly InputManager _inputManager;
    private readonly GameStateManager _gameStateManager;
    
    // Game objects
    private Car? _player1Car;
    private Car? _player2Car;
    
    // Game state
    private readonly Stopwatch _gameTimer = new();
    private readonly GameMode _currentMode;
    private TrackType _currentTrack;
    private int _currentLevel = 1;
    
    // Player stats
    private int _player1Score, _player2Score;
    
    public RacingGame(GameMode mode, IRenderer renderer, IAudioManager audio,
                     IMultiplayerManager? multiplayer = null)
    {
        _currentMode = mode;
        _renderer = renderer;
        _audio = audio;
        _multiplayer = multiplayer;
        _currentTrack = TrackType.Highway;
        
        // Initialize services
        _powerupService = new PowerupService();
        _powerupService.Initialize();
        
        _environmentService = new EnvironmentService(_currentTrack, Weather.Clear, mode);
        _replayService = new ReplayService();
        _scoreService = new ScoreService();
        
        if (mode == GameMode.Career)
        {
            _careerService = new CareerService();
            _careerService.Initialize();
            var level = _careerService.GetCurrentLevel();
            if (level != null)
            {
                _currentTrack = level.Track;
            }
        }
        
        _spawnService = new SpawnService(_player1Car, mode, _currentTrack, 
            _careerService?.Levels.ToList() ?? new List<CareerLevel>(), _currentLevel);
        
        _collisionService = new CollisionService(audio, _powerupService);
        
        // Initialize managers
        _aiManager = new AIManager();
        _inputManager = new InputManager(audio, _powerupService);
        _gameStateManager = new GameStateManager(_spawnService, _powerupService, 
            _environmentService, mode, _currentTrack);
        
        _replayService.LoadGhostData();
    }
    
    public async Task<bool> Initialize()
    {
        if (!_renderer.Initialize())
            throw new Exception("Failed to initialize renderer");
        
        // Initialize players
        _player1Car = new Car(1, 0, 0, true, CarType.Sports);
        
        if (_currentMode == GameMode.Splitscreen)
        {
            _player2Car = new Car(1, 0, 0, true, CarType.Racer);
        }
        
        _gameTimer.Start();
        _spawnService.SpawnInitialContent();
        
        // Start background music
        _audio.PlayMusic("sounds/race_music.mp3", true);
        
        return true;
    }
    
    public void Update(float deltaTime, InputState input)
    {
        if (_player1Car == null) return;
        
        // Update environment
        _environmentService.Update(deltaTime);
        
        // Update powerups
        _powerupService.Update(deltaTime);
        
        // Handle player input
        _inputManager.HandlePlayerInput(_player1Car, ref _player1Score, 
            input.P1Left, input.P1Right, input.P1Accel, input.P1Brake, input.P1Boost,
            _environmentService.CurrentWeather, deltaTime);
        
        if (_currentMode == GameMode.Splitscreen && _player2Car != null)
        {
            _inputManager.HandlePlayerInput(_player2Car, ref _player2Score,
                input.P2Left, input.P2Right, input.P2Accel, input.P2Brake, input.P2Boost,
                _environmentService.CurrentWeather, deltaTime);
        }
        
        // Update AI
        _aiManager.UpdateAI(_spawnService.AiCars, deltaTime);
        
        // Check collisions
        _collisionService.CheckCollisions(_player1Car, ref _player1Score, 
            _spawnService.AiCars, _spawnService.Obstacles);
        
        if (_player2Car != null)
            _collisionService.CheckCollisions(_player2Car, ref _player2Score,
                _spawnService.AiCars, _spawnService.Obstacles);
        
        // Spawn new content
        _spawnService.UpdateSpawning(_player1Car.Distance);
        
        // Cleanup old objects
        _spawnService.Cleanup(_player1Car.Distance);
        
        // Record replay
        if (_currentMode != GameMode.Replay)
        {
            _replayService.RecordFrame((float)_gameTimer.Elapsed.TotalSeconds,
                _player1Car.Lane, _player1Car.Distance, _player1Car.Speed, _player1Score);
        }
        
        // Career progress
        if (_currentMode == GameMode.Career && _careerService != null)
        {
            _careerService.UpdateProgress(_player1Score);
            
            if (_careerService.IsLevelComplete(_player1Score))
            {
                _careerService.AdvanceLevel();
                var nextLevel = _careerService.GetCurrentLevel();
                if (nextLevel != null)
                {
                    _currentTrack = nextLevel.Track;
                    _spawnService.ClearAll();
                    _spawnService.SpawnInitialContent();
                }
            }
        }
    }
    
    public bool Render()
    {
        if (_player1Car == null) return false;
        
        var ghost = _replayService.GetGhostPosition((float)_gameTimer.Elapsed.TotalSeconds);
        var state = _gameStateManager.BuildGameState(_player1Car, _player2Car,
            _player1Score, _player2Score, _gameTimer, _currentLevel,
            _careerService?.Progress ?? 0, ghost);
        
        return _renderer.Render(ref state);
    }
    
    public void Run()
    {
        var frameTimer = Stopwatch.StartNew();
        float lastFrameTime = 0;
        bool paused = false;
        
        while (_player1Car != null && _player1Car.IsAlive)
        {
            float currentTime = (float)frameTimer.Elapsed.TotalSeconds;
            float deltaTime = currentTime - lastFrameTime;
            lastFrameTime = currentTime;
            
            if (deltaTime > 0.1f) deltaTime = 0.1f;
            
            var input = new InputState();
            if (!_renderer.PollInput(ref input)) break;
            
            if (input.Quit) break;
            
            if (input.Pause)
            {
                paused = !paused;
                Thread.Sleep(200);
            }
            
            if (input.Menu)
            {
                ShowInGameMenu();
                Thread.Sleep(200);
            }
            
            if (!paused)
            {
                Update(deltaTime, input);
                
                // Multiplayer sync
                if (_multiplayer?.IsConnected == true && _player1Car != null)
                {
                    _ = _multiplayer.SendGameState(_player1Car, _player1Score);
                    var p2State = _multiplayer.ReceiveGameState().Result;
                    if (p2State.HasValue && _player2Car != null)
                    {
                        _player2Car.Lane = p2State.Value.lane;
                        _player2Car.Distance = p2State.Value.distance;
                        _player2Car.Speed = p2State.Value.speed;
                        _player2Car.Health = p2State.Value.health;
                        _player2Score = p2State.Value.score;
                    }
                }
            }
            
            if (!Render())
            {
                Console.Error.WriteLine("Render failed!");
                break;
            }
            
            Thread.Sleep(GameConstants.FrameDelayMs);
        }
        
        GameOver();
    }
    
    private void ShowInGameMenu()
    {
        var options = new[] { "Resume", "Restart", "Change Track", "Save Replay", "Quit" };
        int selected = 0;
        
        while (true)
        {
            _renderer.RenderMenu("PAUSE MENU", options, selected);
            
            var input = new InputState();
            _renderer.PollInput(ref input);
            
            if (input.P1Accel) selected = Math.Max(0, selected - 1);
            if (input.P1Brake) selected = Math.Min(options.Length - 1, selected + 1);
            
            if (input.P1Boost)
            {
                switch (selected)
                {
                    case 0: return;
                    case 1: Restart(); return;
                    case 2: ChangeTrack(); return;
                    case 3: _replayService.SaveReplay(); return;
                    case 4: Environment.Exit(0); break;
                }
            }
            
            if (input.Quit || input.Pause) return;
            
            Thread.Sleep(50);
        }
    }
    
    private void Restart()
    {
        _player1Car = new Car(1, 0, 0, true, CarType.Sports);
        if (_player2Car != null) 
            _player2Car = new Car(1, 0, 0, true, CarType.Racer);
        
        _spawnService.ClearAll();
        _replayService.ClearRecording();
        
        _player1Score = 0;
        _player2Score = 0;
        
        _powerupService.Initialize();
        _gameTimer.Restart();
        _spawnService.SpawnInitialContent();
    }
    
    private void ChangeTrack()
    {
        _currentTrack = (TrackType)(((int)_currentTrack + 1) % 5);
        _spawnService.ClearAll();
        if (_currentTrack == TrackType.City)
            _spawnService.SpawnInitialContent();
    }
    
    private void GameOver()
    {
        Thread.Sleep(2000);
        
        _replayService.SaveReplay();
        _scoreService.SaveHighScore(Environment.UserName, _player1Score,
            _player1Car?.Distance ?? 0, _gameTimer.Elapsed.TotalSeconds,
            _currentLevel, _currentMode.ToString());
        
        _renderer.Cleanup();
        
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("\n╔════════════════════════════════════════╗");
        Console.WriteLine("║          GAME OVER!                    ║");
        Console.WriteLine("╚════════════════════════════════════════╝\n");
        Console.ResetColor();
        
        Console.WriteLine($"  Final Score:    {_player1Score:N0}");
        Console.WriteLine($"  Distance:       {_player1Car?.Distance:N0}m");
        Console.WriteLine($"  Time:           {_gameTimer.Elapsed.TotalSeconds:F2}s");
        Console.WriteLine($"  Max Combo:      x{_powerupService.Combo}");
        Console.WriteLine($"  Level:          {_currentLevel}");
        
        if (_currentMode == GameMode.Splitscreen && _player2Car != null)
        {
            Console.WriteLine($"\n  Player 2 Score: {_player2Score:N0}");
            Console.WriteLine($"  Winner:         {(_player1Score > _player2Score ? "Player 1" : "Player 2")}");
        }
        
        _scoreService.DisplayHighScores();
        _audio.StopMusic();
    }
    
    public void Cleanup()
    {
        _audio.StopMusic();
        _multiplayer?.Disconnect();
        _renderer.Cleanup();
    }
}