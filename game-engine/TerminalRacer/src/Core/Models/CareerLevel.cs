using TerminalRacer.Core.Enums;

namespace TerminalRacer.Core.Models;

public class CareerLevel
{
    public int Level { get; set; }
    public TrackType Track { get; set; }
    public Weather Weather { get; set; }
    public int TargetScore { get; set; }
    public bool HasBoss { get; set; }
    public string Objective { get; set; } = string.Empty;
    public int Reward { get; set; }
}