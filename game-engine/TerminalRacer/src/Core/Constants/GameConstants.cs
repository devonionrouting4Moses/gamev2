namespace TerminalRacer.Core.Constants;

public static class GameConstants
{
    // Physics
    public const float MaxSpeed = 200.0f;
    public const float BoostSpeed = 250.0f;
    public const float Acceleration = 60.0f;
    public const float Deceleration = 40.0f;
    public const float BrakeForce = 100.0f;
    
    // Track
    public const int NumLanes = 3;
    
    // Powerup durations
    public const float BoostDuration = 3.0f;
    public const float ShieldDuration = 5.0f;
    public const float StarDuration = 7.0f;
    public const float MagnetDuration = 10.0f;
    public const float SlowmoDuration = 5.0f;
    
    // Collision distances
    public const float CarCollisionDistance = 8.0f;
    public const float ObstacleCollisionDistance = 5.0f;
    
    // Damage values
    public const int CarCollisionDamage = 20;
    public const int ConeCollisionDamage = 15;
    
    // Speed modifiers
    public const float RainSpeedMultiplier = 0.95f;
    public const float FogSpeedMultiplier = 0.90f;
    public const float CarCollisionSpeedMultiplier = 0.6f;
    public const float ConeCollisionSpeedMultiplier = 0.7f;
    public const float OilSlickSpeedMultiplier = 0.5f;
    
    // Spawning
    public const int InitialSpawnCount = 5;
    public const int MaxBoostCharge = 200;
    
    // Timing
    public const float ComboDuration = 3.0f;
    public const int TargetFPS = 60;
    public const int FrameDelayMs = 16; // ~60 FPS
    
    // Files
    public const string ReplayFolder = "replays";
    public const string ScoreFile = "highscores.json";
    
    // Network
    public const int DefaultPort = 9999;
    public const int ConnectionTimeoutSeconds = 120;
    public const int ConnectAttempts = 5;
    public const int ConnectDelayMs = 3000;
}