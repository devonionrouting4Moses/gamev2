using TerminalRacer.Core.Enums;

namespace TerminalRacer.GameLogic.Interfaces;

public interface IEnvironmentService
{
    void Update(float deltaTime);
    void ChangeWeather();
    
    float TrackCurve { get; }
    float Elevation { get; }
    float TunnelDarkness { get; }
    Weather CurrentWeather { get; }
}