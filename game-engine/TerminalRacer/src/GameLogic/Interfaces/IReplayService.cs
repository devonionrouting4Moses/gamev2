using TerminalRacer.Core.Models;

namespace TerminalRacer.GameLogic.Interfaces;

public interface IReplayService
{
    void RecordFrame(float time, int position, float distance, float speed, int score);
    void SaveReplay();
    void LoadGhostData();
    (int position, float distance)? GetGhostPosition(float currentTime);
    void ClearRecording();
    
    IReadOnlyList<ReplayFrame> CurrentRecording { get; }
    bool HasGhostData { get; }
}