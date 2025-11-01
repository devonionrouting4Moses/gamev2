using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace TerminalRacer
{
    // Enhanced FFI structures
    [StructLayout(LayoutKind.Sequential)]
    public struct GameState
    {
        // Player 1
        public int PlayerPosition;
        public float PlayerSpeed;
        public float PlayerDistance;
        public int PlayerHealth;
        public int PlayerScore;
        public int PlayerCarType;
        
        // Player 2
        [MarshalAs(UnmanagedType.I1)] public bool Player2Active;
        public int Player2Position;
        public float Player2Speed;
        public float Player2Distance;
        public int Player2Health;
        public int Player2Score;
        public int Player2CarType;
        
        // Game state
        public float LapTime;
        public int GameMode;
        public int TrackType;
        public int Level;
        public float CareerProgress;
        
        // Powerups
        [MarshalAs(UnmanagedType.I1)] public bool BoostActive;
        public float BoostRemaining;
        [MarshalAs(UnmanagedType.I1)] public bool ShieldActive;
        public float ShieldRemaining;
        [MarshalAs(UnmanagedType.I1)] public bool InvincibilityActive;
        public float InvincibilityRemaining;
        [MarshalAs(UnmanagedType.I1)] public bool MagnetActive;
        public float MagnetRemaining;
        [MarshalAs(UnmanagedType.I1)] public bool SlowmoActive;
        public float SlowmoRemaining;
        
        // Objects
        public int CarCount;
        public IntPtr AiPositions;
        public IntPtr AiDistances;
        public IntPtr AiTypes;
        public IntPtr AiIsBoss;
        
        public int ObstacleCount;
        public IntPtr ObstaclePositions;
        public IntPtr ObstacleDistances;
        public IntPtr ObstacleTypes;
        
        public int BuildingCount;
        public IntPtr BuildingPositions;
        public IntPtr BuildingDistances;
        public IntPtr BuildingHeights;
        public IntPtr BuildingTypes;
        
        // Environment
        public int Weather;
        public float CurveOffset;
        public float Elevation;
        public float TunnelDarkness;
        
        // Meta
        public int Combo;
        [MarshalAs(UnmanagedType.I1)] public bool ReplayMode;
        public int GhostPosition;
        public float GhostDistance;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct InputState
    {
        [MarshalAs(UnmanagedType.I1)] public bool P1Left;
        [MarshalAs(UnmanagedType.I1)] public bool P1Right;
        [MarshalAs(UnmanagedType.I1)] public bool P1Accel;
        [MarshalAs(UnmanagedType.I1)] public bool P1Brake;
        [MarshalAs(UnmanagedType.I1)] public bool P1Boost;
        
        [MarshalAs(UnmanagedType.I1)] public bool P2Left;
        [MarshalAs(UnmanagedType.I1)] public bool P2Right;
        [MarshalAs(UnmanagedType.I1)] public bool P2Accel;
        [MarshalAs(UnmanagedType.I1)] public bool P2Brake;
        [MarshalAs(UnmanagedType.I1)] public bool P2Boost;
        
        [MarshalAs(UnmanagedType.I1)] public bool Quit;
        [MarshalAs(UnmanagedType.I1)] public bool Pause;
        [MarshalAs(UnmanagedType.I1)] public bool Menu;
    }

    public static class RatatuiFFI
    {
        private const string LibName = "rust_renderer";
        
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool ratatui_init();
        
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ratatui_cleanup();
        
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool ratatui_poll_input(ref InputState input);
        
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool ratatui_render(ref GameState state);
        
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool ratatui_render_menu(
            [MarshalAs(UnmanagedType.LPStr)] string title,
            [MarshalAs(UnmanagedType.LPArray)] string[] options,
            int optionCount,
            int selected);
    }

    public enum GameMode { Single = 0, Splitscreen = 1, Career = 2, Replay = 3 }
    public enum CarType { Sports = 0, Police = 1, Racer = 2, Truck = 3, Taxi = 4, Van = 5, Muscle = 6, Convertible = 7, Limo = 8 }
    public enum ObstacleType { Cone = 0, Oil = 1, Boost = 2, Star = 3, Magnet = 4, Clock = 5 }
    public enum Weather { Clear = 0, Rain = 1, Fog = 2, Night = 3 }
    public enum TrackType { Highway = 0, City = 1, Mountain = 2, Desert = 3, Tunnel = 4 }

    public class Car
    {
        public int Lane { get; set; }
        public float Distance { get; set; }
        public float Speed { get; set; }
        public bool IsPlayer { get; set; }
        public CarType Type { get; set; }
        public bool IsBoss { get; set; }
        public int Health { get; set; } = 100;
        
        public Car(int lane, float distance, float speed, bool isPlayer = false, CarType type = CarType.Sports, bool isBoss = false)
        {
            Lane = lane;
            Distance = distance;
            Speed = speed;
            IsPlayer = isPlayer;
            Type = type;
            IsBoss = isBoss;
        }
        
        public void Update(float deltaTime) => Distance += Speed * deltaTime;
    }

    public class Obstacle
    {
        public int Lane { get; set; }
        public float Distance { get; set; }
        public ObstacleType Type { get; set; }
        public bool Collected { get; set; }
        
        public Obstacle(int lane, float distance, ObstacleType type)
        {
            Lane = lane;
            Distance = distance;
            Type = type;
        }
    }

    public class Building
    {
        public int Position { get; set; }  // -1 left, 1 right
        public float Distance { get; set; }
        public int Height { get; set; }
        public int Type { get; set; }
        
        public Building(int position, float distance, int height, int type)
        {
            Position = position;
            Distance = distance;
            Height = height;
            Type = type;
        }
    }

    public class ReplayFrame
    {
        public float Time { get; set; }
        public int Position { get; set; }
        public float Distance { get; set; }
        public float Speed { get; set; }
        public int Score { get; set; }
    }

    public class CareerLevel
    {
        public int Level { get; set; }
        public TrackType Track { get; set; }
        public Weather Weather { get; set; }
        public int TargetScore { get; set; }
        public bool HasBoss { get; set; }
        public string Objective { get; set; } = "";
        public int Reward { get; set; }
    }

    public class AudioManager
    {
        private Process? currentPlayer;
        
        public void PlaySound(string soundFile, float volume = 0.5f)
        {
            Task.Run(() =>
            {
                try
                {
                    // Try mpg123 first, fall back to aplay
                    var playerCmd = File.Exists($"/usr/bin/mpg123") ? "mpg123" : "aplay";
                    var args = playerCmd == "mpg123" ? $"-q --gain {(int)(volume * 100)} {soundFile}" : soundFile;
                    
                    var process = Process.Start(new ProcessStartInfo
                    {
                        FileName = playerCmd,
                        Arguments = args,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    });
                    
                    process?.WaitForExit(1000);
                }
                catch { /* Audio is optional */ }
            });
        }
        
        public void PlayMusic(string musicFile, bool loop = true)
        {
            StopMusic();
            
            Task.Run(() =>
            {
                try
                {
                    var args = loop ? $"-q --loop -1 {musicFile}" : $"-q {musicFile}";
                    currentPlayer = Process.Start(new ProcessStartInfo
                    {
                        FileName = "mpg123",
                        Arguments = args,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    });
                }
                catch { /* Music is optional */ }
            });
        }
        
        public void StopMusic()
        {
            if (currentPlayer != null && !currentPlayer.HasExited)
            {
                try
                {
                    currentPlayer.Kill();
                    currentPlayer.Dispose();
                }
                catch { }
                currentPlayer = null;
            }
        }
    }

    public class MultiplayerManager
    {
        private TcpListener? server;
        private TcpClient? client;
        private NetworkStream? stream;
        
        public bool IsServer { get; private set; }
        public bool IsConnected => stream != null;
        
        public async Task<bool> StartServer(int port = 9999)
        {
            try
            {
                server = new TcpListener(IPAddress.Any, port);
                server.Start();
                IsServer = true;
                
                // Wait for client connection
                client = await server.AcceptTcpClientAsync();
                stream = client.GetStream();
                return true;
            }
            catch
            {
                return false;
            }
        }
        
        public async Task<bool> ConnectToServer(string host, int port = 9999)
        {
            try
            {
                client = new TcpClient();
                await client.ConnectAsync(host, port);
                stream = client.GetStream();
                IsServer = false;
                return true;
            }
            catch
            {
                return false;
            }
        }
        
        public async Task SendGameState(Car car, int score)
        {
            if (stream == null) return;
            
            try
            {
                var data = JsonSerializer.Serialize(new { car.Lane, car.Distance, car.Speed, car.Health, score });
                var bytes = Encoding.UTF8.GetBytes(data + "\n");
                await stream.WriteAsync(bytes, 0, bytes.Length);
            }
            catch { }
        }
        
        public async Task<(int lane, float distance, float speed, int health, int score)?> ReceiveGameState()
        {
            if (stream == null) return null;
            
            try
            {
                var buffer = new byte[1024];
                var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                var data = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
                var state = JsonSerializer.Deserialize<JsonElement>(data);
                
                return (
                    state.GetProperty("Lane").GetInt32(),
                    state.GetProperty("Distance").GetSingle(),
                    state.GetProperty("Speed").GetSingle(),
                    state.GetProperty("Health").GetInt32(),
                    state.GetProperty("score").GetInt32()
                );
            }
            catch
            {
                return null;
            }
        }
        
        public void Disconnect()
        {
            stream?.Close();
            client?.Close();
            server?.Stop();
        }
    }

    public class RacingGame
    {
        // Constants
        private const float MaxSpeed = 200.0f;
        private const float BoostSpeed = 250.0f;
        private const float Acceleration = 60.0f;
        private const float Deceleration = 40.0f;
        private const float BrakeForce = 100.0f;
        private const int NumLanes = 3;
        
        // Powerup durations
        private const float BoostDuration = 3.0f;
        private const float ShieldDuration = 5.0f;
        private const float StarDuration = 7.0f;
        private const float MagnetDuration = 10.0f;
        private const float SlowmoDuration = 5.0f;
        
        // Game objects
        private Car? player1Car;
        private Car? player2Car;
        private List<Car> aiCars = new();
        private List<Obstacle> obstacles = new();
        private List<Building> buildings = new();
        
        // Game state
        private Stopwatch gameTimer = new();
        private Random random = new();
        private GameMode currentMode;
        private TrackType currentTrack;
        private Weather currentWeather;
        private int currentLevel = 1;
        
        // Spawn tracking
        private float nextAiSpawn;
        private float nextObstacleSpawn;
        private float nextBuildingSpawn;
        
        // Player stats
        private int player1Score, player2Score;
        private int combo;
        private float comboTimer;
        
        // Powerups
        private Dictionary<string, (bool active, float remaining, float cooldown)> powerups = new();
        
        // Environment
        private float trackCurve;
        private float curveTimer;
        private float elevation;
        private float elevationTimer;
        private float tunnelDarkness;
        private float weatherChangeTimer = 30f;
        
        // Features
        private AudioManager audio = new();
        private MultiplayerManager? multiplayer;
        private List<ReplayFrame> replayData = new();
        private List<ReplayFrame>? ghostData;
        private int ghostIndex = 0;
        private List<CareerLevel> careerLevels = new();
        private float careerProgress = 0;
        
        private const string ReplayFolder = "replays";
        private const string ScoreFile = "highscores.json";
        
        public RacingGame(GameMode mode = GameMode.Single)
        {
            currentMode = mode;
            currentTrack = TrackType.Highway;
            currentWeather = Weather.Clear;
            
            InitializePowerups();
            InitializeCareerLevels();
            LoadGhostData();
        }
        
        private void InitializePowerups()
        {
            powerups["boost"] = (false, 0, 0);
            powerups["shield"] = (false, 0, 0);
            powerups["star"] = (false, 0, 0);
            powerups["magnet"] = (false, 0, 0);
            powerups["slowmo"] = (false, 0, 0);
        }
        
        private void InitializeCareerLevels()
        {
            careerLevels.AddRange(new[]
            {
                new CareerLevel { Level = 1, Track = TrackType.Highway, Weather = Weather.Clear, TargetScore = 5000, Objective = "Complete first race" },
                new CareerLevel { Level = 2, Track = TrackType.City, Weather = Weather.Clear, TargetScore = 8000, Objective = "Navigate city streets" },
                new CareerLevel { Level = 3, Track = TrackType.Highway, Weather = Weather.Rain, TargetScore = 10000, Objective = "Race in the rain" },
                new CareerLevel { Level = 4, Track = TrackType.Mountain, Weather = Weather.Clear, TargetScore = 15000, HasBoss = true, Objective = "Defeat mountain boss" },
                new CareerLevel { Level = 5, Track = TrackType.Desert, Weather = Weather.Clear, TargetScore = 20000, Objective = "Conquer the desert" },
                new CareerLevel { Level = 6, Track = TrackType.Tunnel, Weather = Weather.Night, TargetScore = 25000, Objective = "Master the tunnel" },
                new CareerLevel { Level = 7, Track = TrackType.City, Weather = Weather.Fog, TargetScore = 30000, HasBoss = true, Objective = "City fog boss battle" },
                new CareerLevel { Level = 8, Track = TrackType.Mountain, Weather = Weather.Rain, TargetScore = 40000, HasBoss = true, Objective = "Ultimate mountain challenge" },
                new CareerLevel { Level = 9, Track = TrackType.Highway, Weather = Weather.Night, TargetScore = 50000, HasBoss = true, Objective = "Final boss showdown!" },
            });
        }
        
        public async Task<bool> Initialize()
        {
            if (!RatatuiFFI.ratatui_init())
                throw new Exception("Failed to initialize terminal");
            
            // Initialize players
            player1Car = new Car(1, 0, 0, true, CarType.Sports);
            
            if (currentMode == GameMode.Splitscreen)
            {
                player2Car = new Car(1, 0, 0, true, CarType.Racer);
                
                // Optional: Setup multiplayer
                if (multiplayer != null && multiplayer.IsConnected)
                {
                    // Multiplayer is ready
                }
            }
            
            if (currentMode == GameMode.Career)
            {
                var level = careerLevels[currentLevel - 1];
                currentTrack = level.Track;
                currentWeather = level.Weather;
            }
            
            gameTimer.Start();
            SpawnInitialContent();
            
            // Start background music
            audio.PlayMusic("sounds/race_music.mp3", true);
            
            return true;
        }
        
        private void SpawnInitialContent()
        {
            nextAiSpawn = 50f;
            nextObstacleSpawn = 30f;
            nextBuildingSpawn = 40f;
            
            for (int i = 0; i < 5; i++)
            {
                SpawnAiCar();
                SpawnObstacle();
                
                if (currentTrack == TrackType.City)
                    SpawnBuilding();
            }
        }
        
        private void SpawnAiCar()
        {
            var lane = random.Next(NumLanes);
            var distance = (player1Car?.Distance ?? 0) + random.Next(40, 100);
            var speed = random.Next(60, 140);
            
            var type = CarType.Sports;
            var isBoss = false;
            
            if (currentMode == GameMode.Career && careerLevels[currentLevel - 1].HasBoss && aiCars.Count == 0)
            {
                isBoss = true;
                type = CarType.Limo;
                speed = 180;
            }
            else
            {
                var roll = random.Next(100);
                type = roll < 15 ? CarType.Police : roll < 30 ? CarType.Racer : (CarType)random.Next(9);
                speed = type == CarType.Police ? random.Next(120, 160) : type == CarType.Racer ? random.Next(140, 180) : speed;
            }
            
            aiCars.Add(new Car(lane, distance, speed, false, type, isBoss));
        }
        
        private void SpawnObstacle()
        {
            var lane = random.Next(NumLanes);
            var distance = (player1Car?.Distance ?? 0) + random.Next(50, 120);
            
            var type = ObstacleType.Cone;
            var roll = random.Next(100);
            
            if (roll < 15) type = ObstacleType.Boost;
            else if (roll < 30) type = ObstacleType.Oil;
            else if (roll < 40) type = ObstacleType.Star;
            else if (roll < 50) type = ObstacleType.Magnet;
            else if (roll < 60) type = ObstacleType.Clock;
            
            obstacles.Add(new Obstacle(lane, distance, type));
        }
        
        private void SpawnBuilding()
        {
            var position = random.Next(2) == 0 ? -1 : 1;
            var distance = (player1Car?.Distance ?? 0) + random.Next(60, 150);
            var height = random.Next(5, 15);
            var type = random.Next(4);
            
            buildings.Add(new Building(position, distance, height, type));
        }
        
        public void Update(float deltaTime, InputState input)
        {
            if (player1Car == null) return;
            
            // Update environment
            UpdateEnvironment(deltaTime);
            
            // Update powerups
            UpdatePowerups(deltaTime);
            
            // Handle player input
            HandlePlayerInput(player1Car, ref player1Score, input.P1Left, input.P1Right, input.P1Accel, input.P1Brake, input.P1Boost, deltaTime);
            
            if (currentMode == GameMode.Splitscreen && player2Car != null)
            {
                HandlePlayerInput(player2Car, ref player2Score, input.P2Left, input.P2Right, input.P2Accel, input.P2Brake, input.P2Boost, deltaTime);
            }
            
            // Update AI
            UpdateAI(deltaTime);
            
            // Check collisions
            CheckCollisions(player1Car, ref player1Score);
            if (player2Car != null) CheckCollisions(player2Car, ref player2Score);
            
            // Spawn new content
            SpawnContent();
            
            // Cleanup old objects
            Cleanup(player1Car.Distance);
            
            // Record replay
            if (currentMode != GameMode.Replay)
            {
                RecordReplayFrame();
            }
            
            // Career progress
            if (currentMode == GameMode.Career)
            {
                UpdateCareerProgress();
            }
        }
        
        private void HandlePlayerInput(Car car, ref int score, bool left, bool right, bool accel, bool brake, bool boost, float dt)
        {
            if (left && car.Lane > 0) car.Lane--;
            if (right && car.Lane < NumLanes - 1) car.Lane++;
            
            var maxSpeed = powerups["boost"].active ? BoostSpeed : MaxSpeed;
            
            if (accel) car.Speed = Math.Min(car.Speed + Acceleration * dt, maxSpeed);
            else if (brake) car.Speed = Math.Max(car.Speed - BrakeForce * dt, 0);
            else car.Speed = Math.Max(car.Speed - Deceleration * dt, 0);
            
            if (currentWeather == Weather.Rain) car.Speed *= 0.95f;
            if (currentWeather == Weather.Fog) car.Speed *= 0.90f;
            
            if (boost && powerups["boost"].remaining > 0 && !powerups["boost"].active)
            {
                ActivatePowerup("boost", BoostDuration);
                audio.PlaySound("sounds/boost.wav");
            }
            
            car.Update(dt);
            score += (int)(car.Speed * dt * 0.1f);
        }
        
        private void UpdateEnvironment(float dt)
        {
            curveTimer += dt;
            trackCurve = (float)(Math.Sin(curveTimer * 0.3) * 2.0 + Math.Sin(curveTimer * 0.15) * 1.5);
            
            elevationTimer += dt * 0.5f;
            elevation = currentTrack == TrackType.Mountain ? (float)Math.Sin(elevationTimer) : 0;
            
            tunnelDarkness = currentTrack == TrackType.Tunnel ? (float)(Math.Sin(curveTimer * 0.5) * 0.3 + 0.5) : 0;
            
            weatherChangeTimer -= dt;
            if (weatherChangeTimer <= 0 && currentMode != GameMode.Career)
            {
                ChangeWeather();
                weatherChangeTimer = random.Next(30, 60);
            }
            
            comboTimer -= dt;
            if (comboTimer <= 0) combo = 0;
        }
        
        private void UpdatePowerups(float dt)
        {
            var keys = powerups.Keys.ToList();
            foreach (var key in keys)
            {
                var (active, remaining, cooldown) = powerups[key];
                
                if (active)
                {
                    remaining -= dt;
                    if (remaining <= 0)
                    {
                        active = false;
                        remaining = 0;
                    }
                }
                
                if (cooldown > 0)
                {
                    cooldown -= dt;
                }
                
                powerups[key] = (active, remaining, cooldown);
            }
        }
        
        private void UpdateAI(float dt)
        {
            foreach (var car in aiCars)
            {
                if (random.NextDouble() < 0.002)
                {
                    var newLane = car.Lane + (random.Next(2) == 0 ? -1 : 1);
                    if (newLane >= 0 && newLane < NumLanes) car.Lane = newLane;
                }
                
                car.Update(dt);
            }
        }
        
        private void CheckCollisions(Car playerCar, ref int score)
        {
            // Car collisions
            foreach (var ai in aiCars)
            {
                if (ai.Lane == playerCar.Lane && Math.Abs(ai.Distance - playerCar.Distance) < 8.0f)
                {
                    if (!powerups["star"].active)
                    {
                        playerCar.Health = Math.Max(0, playerCar.Health - 20);
                        combo = 0;
                        audio.PlaySound("sounds/crash.wav");
                    }
                    
                    playerCar.Speed *= 0.6f;
                    if (ai.Distance > playerCar.Distance) ai.Distance += 10;
                }
            }
            
            // Obstacle collection
            foreach (var obs in obstacles.Where(o => !o.Collected && o.Lane == playerCar.Lane))
            {
                if (Math.Abs(obs.Distance - playerCar.Distance) < 5.0f)
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
                    if (!powerups["star"].active)
                    {
                        car.Health = Math.Max(0, car.Health - 15);
                        car.Speed *= 0.7f;
                        combo = 0;
                        audio.PlaySound("sounds/hit.wav");
                    }
                    break;
                    
                case ObstacleType.Oil:
                    if (!powerups["star"].active)
                    {
                        car.Speed *= 0.5f;
                        combo = 0;
                    }
                    break;
                    
                case ObstacleType.Boost:
                    powerups["boost"] = (false, Math.Min(powerups["boost"].remaining + 100, 200), 0);
                    score += 100 * (combo + 1);
                    combo++;
                    comboTimer = 3.0f;
                    audio.PlaySound("sounds/powerup.wav");
                    break;
                    
                case ObstacleType.Star:
                    ActivatePowerup("star", StarDuration);
                    score += 200 * (combo + 1);
                    combo++;
                    comboTimer = 3.0f;
                    audio.PlaySound("sounds/star.wav");
                    break;
                    
                case ObstacleType.Magnet:
                    ActivatePowerup("magnet", MagnetDuration);
                    score += 150 * (combo + 1);
                    combo++;
                    comboTimer = 3.0f;
                    audio.PlaySound("sounds/magnet.wav");
                    break;
                    
                case ObstacleType.Clock:
                    ActivatePowerup("slowmo", SlowmoDuration);
                    score += 250 * (combo + 1);
                    combo++;
                    comboTimer = 3.0f;
                    audio.PlaySound("sounds/slowmo.wav");
                    break;
            }
        }
        
        private void ActivatePowerup(string name, float duration)
        {
            powerups[name] = (true, duration, 0);
        }
        
        private void SpawnContent()
        {
            if (player1Car == null) return;
            
            if (player1Car.Distance > nextAiSpawn)
            {
                SpawnAiCar();
                nextAiSpawn = player1Car.Distance + random.Next(30, 60);
            }
            
            if (player1Car.Distance > nextObstacleSpawn)
            {
                SpawnObstacle();
                nextObstacleSpawn = player1Car.Distance + random.Next(40, 80);
            }
            
            if (currentTrack == TrackType.City && player1Car.Distance > nextBuildingSpawn)
            {
                SpawnBuilding();
                nextBuildingSpawn = player1Car.Distance + random.Next(50, 100);
            }
        }
        
        private void Cleanup(float playerDistance)
        {
            aiCars.RemoveAll(c => c.Distance < playerDistance - 120);
            obstacles.RemoveAll(o => o.Distance < playerDistance - 100 || o.Collected);
            buildings.RemoveAll(b => b.Distance < playerDistance - 150);
        }
        
        private void ChangeWeather()
        {
            var roll = random.Next(100);
            currentWeather = roll < 40 ? Weather.Clear : roll < 65 ? Weather.Rain : roll < 85 ? Weather.Fog : Weather.Night;
        }
        
        private void RecordReplayFrame()
        {
            if (player1Car == null) return;
            
            replayData.Add(new ReplayFrame
            {
                Time = (float)gameTimer.Elapsed.TotalSeconds,
                Position = player1Car.Lane,
                Distance = player1Car.Distance,
                Speed = player1Car.Speed,
                Score = player1Score
            });
        }
        
        private void SaveReplay()
        {
            if (replayData.Count == 0) return;
            
            try
            {
                Directory.CreateDirectory(ReplayFolder);
                var filename = $"{ReplayFolder}/replay_{DateTime.Now:yyyyMMdd_HHmmss}.json";
                var json = JsonSerializer.Serialize(replayData, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(filename, json);
            }
            catch { }
        }
        
        private void LoadGhostData()
        {
            try
            {
                if (Directory.Exists(ReplayFolder))
                {
                    var files = Directory.GetFiles(ReplayFolder, "*.json");
                    if (files.Length > 0)
                    {
                        var bestReplay = files.OrderByDescending(f => new FileInfo(f).LastWriteTime).First();
                        var json = File.ReadAllText(bestReplay);
                        ghostData = JsonSerializer.Deserialize<List<ReplayFrame>>(json);
                    }
                }
            }
            catch { }
        }
        
        private void UpdateCareerProgress()
        {
            if (currentLevel > careerLevels.Count) return;
            
            var level = careerLevels[currentLevel - 1];
            careerProgress = Math.Min(100f, (player1Score / (float)level.TargetScore) * 100f);
            
            if (player1Score >= level.TargetScore)
            {
                // Level complete!
                currentLevel++;
                if (currentLevel <= careerLevels.Count)
                {
                    var nextLevel = careerLevels[currentLevel - 1];
                    currentTrack = nextLevel.Track;
                    currentWeather = nextLevel.Weather;
                    careerProgress = 0;
                    
                    // Reset for next level
                    aiCars.Clear();
                    obstacles.Clear();
                    buildings.Clear();
                    SpawnInitialContent();
                }
            }
        }
        
        public bool Render()
        {
            if (player1Car == null) return false;
            
            var state = BuildGameState();
            return RatatuiFFI.ratatui_render(ref state);
        }
        
        private GameState BuildGameState()
        {
            if (player1Car == null) return new GameState();
            
            // Prepare AI data
            var aiPositions = aiCars.Select(c => c.Lane).ToArray();
            var aiDistances = aiCars.Select(c => c.Distance).ToArray();
            var aiTypes = aiCars.Select(c => (int)c.Type).ToArray();
            var aiIsBoss = aiCars.Select(c => c.IsBoss).ToArray();
            
            // Prepare obstacle data
            var obsPositions = obstacles.Select(o => o.Lane).ToArray();
            var obsDistances = obstacles.Select(o => o.Distance).ToArray();
            var obsTypes = obstacles.Select(o => (int)o.Type).ToArray();
            
            // Prepare building data
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
            
            // Get ghost position
            int ghostPos = 0;
            float ghostDist = 0;
            if (ghostData != null && currentMode == GameMode.Single)
            {
                var currentTime = (float)gameTimer.Elapsed.TotalSeconds;
                while (ghostIndex < ghostData.Count && ghostData[ghostIndex].Time <= currentTime)
                {
                    ghostPos = ghostData[ghostIndex].Position;
                    ghostDist = ghostData[ghostIndex].Distance;
                    ghostIndex++;
                }
            }
            
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
                GameMode = (int)currentMode,
                TrackType = (int)currentTrack,
                Level = currentLevel,
                CareerProgress = careerProgress,
                
                // Powerups
                BoostActive = powerups["boost"].active,
                BoostRemaining = powerups["boost"].remaining,
                ShieldActive = powerups["shield"].active,
                ShieldRemaining = powerups["shield"].remaining,
                InvincibilityActive = powerups["star"].active,
                InvincibilityRemaining = powerups["star"].remaining,
                MagnetActive = powerups["magnet"].active,
                MagnetRemaining = powerups["magnet"].remaining,
                SlowmoActive = powerups["slowmo"].active,
                SlowmoRemaining = powerups["slowmo"].remaining,
                
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
                Weather = (int)currentWeather,
                CurveOffset = trackCurve,
                Elevation = elevation,
                TunnelDarkness = tunnelDarkness,
                
                // Meta
                Combo = combo,
                ReplayMode = currentMode == GameMode.Replay,
                GhostPosition = ghostPos,
                GhostDistance = ghostDist,
            };
            
            // Free handles after render
            Task.Run(async () =>
            {
                await Task.Delay(100);
                foreach (var h in handles) h.Free();
            });
            
            return state;
        }
        
        public void Run()
        {
            var frameTimer = Stopwatch.StartNew();
            float lastFrameTime = 0;
            bool paused = false;
            
            while (player1Car != null && player1Car.Health > 0)
            {
                float currentTime = (float)frameTimer.Elapsed.TotalSeconds;
                float deltaTime = currentTime - lastFrameTime;
                lastFrameTime = currentTime;
                
                if (deltaTime > 0.1f) deltaTime = 0.1f;
                
                var input = new InputState();
                if (!RatatuiFFI.ratatui_poll_input(ref input)) break;
                
                if (input.Quit) break;
                
                if (input.Pause)
                {
                    paused = !paused;
                    Thread.Sleep(200); // Debounce
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
                    if (multiplayer?.IsConnected == true)
                    {
                        _ = multiplayer.SendGameState(player1Car, player1Score);
                        var p2State = multiplayer.ReceiveGameState().Result;
                        if (p2State.HasValue && player2Car != null)
                        {
                            player2Car.Lane = p2State.Value.lane;
                            player2Car.Distance = p2State.Value.distance;
                            player2Car.Speed = p2State.Value.speed;
                            player2Car.Health = p2State.Value.health;
                            player2Score = p2State.Value.score;
                        }
                    }
                }
                
                if (!Render())
                {
                    Console.Error.WriteLine("Render failed!");
                    break;
                }
                
                Thread.Sleep(16); // ~60 FPS
            }
            
            GameOver();
        }
        
        private void ShowInGameMenu()
        {
            var options = new[] { "Resume", "Restart", "Change Track", "Save Replay", "Quit" };
            int selected = 0;
            
            while (true)
            {
                RatatuiFFI.ratatui_render_menu("PAUSE MENU", options, options.Length, selected);
                
                var input = new InputState();
                RatatuiFFI.ratatui_poll_input(ref input);
                
                if (input.P1Accel) selected = Math.Max(0, selected - 1);
                if (input.P1Brake) selected = Math.Min(options.Length - 1, selected + 1);
                
                if (input.P1Boost) // Enter
                {
                    switch (selected)
                    {
                        case 0: return; // Resume
                        case 1: Restart(); return;
                        case 2: ChangeTrack(); return;
                        case 3: SaveReplay(); return;
                        case 4: Environment.Exit(0); break;
                    }
                }
                
                if (input.Quit || input.Pause) return;
                
                Thread.Sleep(50);
            }
        }
        
        private void Restart()
        {
            player1Car = new Car(1, 0, 0, true, CarType.Sports);
            if (player2Car != null) player2Car = new Car(1, 0, 0, true, CarType.Racer);
            
            aiCars.Clear();
            obstacles.Clear();
            buildings.Clear();
            replayData.Clear();
            
            player1Score = 0;
            player2Score = 0;
            combo = 0;
            
            InitializePowerups();
            gameTimer.Restart();
            SpawnInitialContent();
        }
        
        private void ChangeTrack()
        {
            currentTrack = (TrackType)(((int)currentTrack + 1) % 5);
            buildings.Clear();
            if (currentTrack == TrackType.City) SpawnInitialContent();
        }
        
        private void GameOver()
        {
            Thread.Sleep(2000);
            
            SaveReplay();
            SaveHighScore();
            
            RatatuiFFI.ratatui_cleanup();
            
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\n╔════════════════════════════════════════╗");
            Console.WriteLine("║          GAME OVER!                    ║");
            Console.WriteLine("╚════════════════════════════════════════╝\n");
            Console.ResetColor();
            
            Console.WriteLine($"  Final Score:    {player1Score:N0}");
            Console.WriteLine($"  Distance:       {player1Car?.Distance:N0}m");
            Console.WriteLine($"  Time:           {gameTimer.Elapsed.TotalSeconds:F2}s");
            Console.WriteLine($"  Max Combo:      x{combo}");
            Console.WriteLine($"  Level:          {currentLevel}");
            
            if (currentMode == GameMode.Splitscreen && player2Car != null)
            {
                Console.WriteLine("\n  Player 2 Score: {0:N0}", player2Score);
                Console.WriteLine("  Winner:         {0}", player1Score > player2Score ? "Player 1" : "Player 2");
            }
            
            ShowHighScores();
            
            audio.StopMusic();
        }
        
        private void SaveHighScore()
        {
            try
            {
                var scores = LoadHighScores();
                scores.Add(new Dictionary<string, object>
                {
                    ["player"] = Environment.UserName,
                    ["score"] = player1Score,
                    ["distance"] = player1Car?.Distance ?? 0,
                    ["time"] = gameTimer.Elapsed.TotalSeconds,
                    ["level"] = currentLevel,
                    ["mode"] = currentMode.ToString(),
                    ["date"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                });
                
                scores = scores.OrderByDescending(s => Convert.ToInt32(s["score"])).Take(10).ToList();
                
                var json = JsonSerializer.Serialize(scores, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(ScoreFile, json);
            }
            catch { }
        }
        
        private List<Dictionary<string, object>> LoadHighScores()
        {
            try
            {
                if (File.Exists(ScoreFile))
                {
                    var json = File.ReadAllText(ScoreFile);
                    return JsonSerializer.Deserialize<List<Dictionary<string, object>>>(json) ?? new();
                }
            }
            catch { }
            
            return new List<Dictionary<string, object>>();
        }
        
        private void ShowHighScores()
        {
            var scores = LoadHighScores();
            
            Console.WriteLine("\n╔════════════════════════════════════════╗");
            Console.WriteLine("║          HIGH SCORES                   ║");
            Console.WriteLine("╠════════════════════════════════════════╣");
            
            for (int i = 0; i < Math.Min(5, scores.Count); i++)
            {
                var s = scores[i];
                
                // Handle JsonElement values from JSON deserialization
                int score = 0;
                int level = 0;
                string player = "Unknown";
                
                if (s["score"] is JsonElement scoreElem)
                    score = scoreElem.GetInt32();
                else
                    score = Convert.ToInt32(s["score"]);
                
                if (s["level"] is JsonElement levelElem)
                    level = levelElem.GetInt32();
                else
                    level = Convert.ToInt32(s["level"]);
                
                if (s["player"] is JsonElement playerElem)
                    player = playerElem.GetString() ?? "Unknown";
                else
                    player = s["player"]?.ToString() ?? "Unknown";
                
                Console.WriteLine($"║ {i+1}. {player,-10} {score,8:N0}  Lv.{level,-2} ║");
            }
            
            Console.WriteLine("╚════════════════════════════════════════╝\n");
        }
        
        public void Cleanup()
        {
            audio.StopMusic();
            multiplayer?.Disconnect();
            RatatuiFFI.ratatui_cleanup();
        }
    }
    
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("╔════════════════════════════════════════╗");
            Console.WriteLine("║   TERMINAL RACER - ULTIMATE EDITION    ║");
            Console.WriteLine("╚════════════════════════════════════════╝");
            Console.WriteLine("\n🏎️  Select Game Mode:");
            Console.WriteLine("  1. Single Player");
            Console.WriteLine("  2. Split-Screen (Local)");
            Console.WriteLine("  3. Career Mode");
            Console.WriteLine("  4. Replay Mode");
            Console.WriteLine("  5. Network Multiplayer");
            Console.WriteLine("  6. Exit");
            Console.Write("\nChoice: ");
            
            var choice = Console.ReadLine();
            
            if (choice == "6") return;
            
            GameMode mode = choice switch
            {
                "1" => GameMode.Single,
                "2" => GameMode.Splitscreen,
                "3" => GameMode.Career,
                "4" => GameMode.Replay,
                "5" => GameMode.Splitscreen, // Will setup network
                _ => GameMode.Single
            };
            
            RacingGame game = new(mode);
            
            // Network multiplayer setup
            MultiplayerManager? mp = null;
            if (choice == "5")
            {
                Console.Write("\nHost (h) or Join (j)? ");
                var netChoice = Console.ReadLine()?.ToLower();
                
                mp = new MultiplayerManager();
                
                if (netChoice == "h")
                {
                    Console.WriteLine("Starting server on port 9999...");
                    if (await mp.StartServer())
                    {
                        Console.WriteLine("✓ Waiting for player 2...");
                    }
                    else
                    {
                        Console.WriteLine("❌ Failed to start server");
                        return;
                    }
                }
                else
                {
                    Console.Write("Enter host IP: ");
                    var host = Console.ReadLine() ?? "localhost";
                    Console.WriteLine($"Connecting to {host}...");
                    
                    if (await mp.ConnectToServer(host))
                    {
                        Console.WriteLine("✓ Connected!");
                    }
                    else
                    {
                        Console.WriteLine("❌ Connection failed");
                        return;
                    }
                }
                
                Thread.Sleep(1000);
            }
            
            Console.WriteLine("\n🚀 Starting game...\n");
            Thread.Sleep(500);
            
            try
            {
                await game.Initialize();
                game.Run();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"\n❌ Error: {ex.Message}");
                Console.Error.WriteLine($"Stack: {ex.StackTrace}");
            }
            finally
            {
                game.Cleanup();
            }
            
            Console.WriteLine("\n🏁 Thanks for playing Terminal Racer Ultimate Edition!");
        }
    }
}