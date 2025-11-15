using System.Text.Json;
using TerminalRacer.Core.Constants;
using TerminalRacer.Core.Models;
using TerminalRacer.GameLogic.Interfaces;

namespace TerminalRacer.GameLogic.Services;

public class ReplayService : IReplayService
{
    private readonly List<ReplayFrame> _replayData = new();
    private List<ReplayFrame>? _ghostData;
    private int _ghostIndex;
    
    public IReadOnlyList<ReplayFrame> CurrentRecording => _replayData.AsReadOnly();
    public bool HasGhostData => _ghostData != null && _ghostData.Count > 0;
    
    public void RecordFrame(float time, int position, float distance, float speed, int score)
    {
        _replayData.Add(new ReplayFrame
        {
            Time = time,
            Position = position,
            Distance = distance,
            Speed = speed,
            Score = score
        });
    }
    
    public void SaveReplay()
    {
        if (_replayData.Count == 0) return;
        
        try
        {
            Directory.CreateDirectory(GameConstants.ReplayFolder);
            var filename = $"{GameConstants.ReplayFolder}/replay_{DateTime.Now:yyyyMMdd_HHmmss}.json";
            var json = JsonSerializer.Serialize(_replayData, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(filename, json);
        }
        catch { }
    }
    
    public void LoadGhostData()
    {
        try
        {
            if (Directory.Exists(GameConstants.ReplayFolder))
            {
                var files = Directory.GetFiles(GameConstants.ReplayFolder, "*.json");
                if (files.Length > 0)
                {
                    var bestReplay = files.OrderByDescending(f => new FileInfo(f).LastWriteTime).First();
                    var json = File.ReadAllText(bestReplay);
                    _ghostData = JsonSerializer.Deserialize<List<ReplayFrame>>(json);
                    _ghostIndex = 0;
                }
            }
        }
        catch { }
    }
    
    public (int position, float distance)? GetGhostPosition(float currentTime)
    {
        if (_ghostData == null || _ghostData.Count == 0) 
            return null;
        
        while (_ghostIndex < _ghostData.Count && _ghostData[_ghostIndex].Time <= currentTime)
        {
            _ghostIndex++;
        }
        
        if (_ghostIndex > 0 && _ghostIndex <= _ghostData.Count)
        {
            var frame = _ghostData[_ghostIndex - 1];
            return (frame.Position, frame.Distance);
        }
        
        return null;
    }
    
    public void ClearRecording()
    {
        _replayData.Clear();
        _ghostIndex = 0;
    }
}
