using System.Runtime.InteropServices;

namespace TerminalRacer.Rendering.FFI;

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