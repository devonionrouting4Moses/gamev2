using TerminalRacer.Core.Enums;
using TerminalRacer.GameLogic.Interfaces;

namespace TerminalRacer.GameLogic.Services;

public class EnvironmentService : IEnvironmentService
{
    private readonly Random _random = new();
    private readonly TrackType _trackType;
    private readonly GameMode _gameMode;
    
    private float _trackCurve;
    private float _curveTimer;
    private float _elevation;
    private float _elevationTimer;
    private float _tunnelDarkness;
    private float _weatherChangeTimer = 30f;
    private Weather _currentWeather;
    
    public float TrackCurve => _trackCurve;
    public float Elevation => _elevation;
    public float TunnelDarkness => _tunnelDarkness;
    public Weather CurrentWeather => _currentWeather;
    
    public EnvironmentService(TrackType trackType, Weather initialWeather, GameMode gameMode)
    {
        _trackType = trackType;
        _currentWeather = initialWeather;
        _gameMode = gameMode;
    }
    
    public void Update(float deltaTime)
    {
        _curveTimer += deltaTime;
        _trackCurve = (float)(Math.Sin(_curveTimer * 0.3) * 2.0 + Math.Sin(_curveTimer * 0.15) * 1.5);
        
        _elevationTimer += deltaTime * 0.5f;
        _elevation = _trackType == TrackType.Mountain ? (float)Math.Sin(_elevationTimer) : 0;
        
        _tunnelDarkness = _trackType == TrackType.Tunnel 
            ? (float)(Math.Sin(_curveTimer * 0.5) * 0.3 + 0.5) 
            : 0;
        
        _weatherChangeTimer -= deltaTime;
        if (_weatherChangeTimer <= 0 && _gameMode != GameMode.Career)
        {
            ChangeWeather();
            _weatherChangeTimer = _random.Next(30, 60);
        }
    }
    
    public void ChangeWeather()
    {
        var roll = _random.Next(100);
        _currentWeather = roll < 40 ? Weather.Clear : 
                         roll < 65 ? Weather.Rain : 
                         roll < 85 ? Weather.Fog : 
                         Weather.Night;
    }
}