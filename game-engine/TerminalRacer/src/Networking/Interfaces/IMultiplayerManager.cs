using TerminalRacer.Core.Models;

namespace TerminalRacer.Networking.Interfaces;

public interface IMultiplayerManager
{
    bool IsServer { get; }
    bool IsConnected { get; }
    Task<bool> StartServer(int port = 9999);
    Task<bool> ConnectToServer(string host, int port = 9999);
    Task SendGameState(Car car, int score);
    Task<(int lane, float distance, float speed, int health, int score)?> ReceiveGameState();
    void Disconnect();
}