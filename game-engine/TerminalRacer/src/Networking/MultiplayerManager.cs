using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using TerminalRacer.Core.Constants;
using TerminalRacer.Core.Models;
using TerminalRacer.Networking.Interfaces;

namespace TerminalRacer.Networking;

public class MultiplayerManager : IMultiplayerManager
{
    private TcpListener? _server;
    private TcpClient? _client;
    private NetworkStream? _stream;
    
    public bool IsServer { get; private set; }
    public bool IsConnected => _stream != null;
    
    public async Task<bool> StartServer(int port = GameConstants.DefaultPort)
    {
        try
        {
            _server = new TcpListener(IPAddress.Any, port);
            _server.Start();
            IsServer = true;
            
            var localIP = GetLocalIPAddress();
            
            PrintServerStartMessage(port, localIP);
            
            var acceptTask = _server.AcceptTcpClientAsync();
            var timeoutTask = Task.Delay(TimeSpan.FromSeconds(GameConstants.ConnectionTimeoutSeconds));
            
            var completedTask = await Task.WhenAny(acceptTask, timeoutTask);
            
            if (completedTask == timeoutTask)
            {
                Console.WriteLine("❌ Connection timeout. No player connected.");
                return false;
            }
            
            _client = await acceptTask;
            _stream = _client.GetStream();
            
            var remoteIP = ((IPEndPoint?)_client.Client.RemoteEndPoint)?.Address.ToString() ?? "Unknown";
            Console.WriteLine($"\n✓ Player 2 connected from {remoteIP}");
            Console.WriteLine("🎮 Starting multiplayer game...\n");
            
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Server error: {ex.Message}");
            return false;
        }
    }
    
    public async Task<bool> ConnectToServer(string host, int port = GameConstants.DefaultPort)
    {
        PrintConnectingMessage(host, port);
        
        await TryResolveHostname(host);
        
        for (int attempt = 1; attempt <= GameConstants.ConnectAttempts; attempt++)
        {
            if (await TryConnect(host, port, attempt))
            {
                Console.WriteLine(" ✓ Connected!\n");
                Console.WriteLine($"✓ Successfully connected to {host}:{port}");
                Console.WriteLine("🎮 Starting multiplayer game...\n");
                return true;
            }
            
            if (attempt < GameConstants.ConnectAttempts)
            {
                await Task.Delay(GameConstants.ConnectDelayMs);
            }
        }
        
        PrintConnectionFailedMessage(host);
        return false;
    }
    
    public async Task SendGameState(Car car, int score)
    {
        if (_stream == null) return;
        
        try
        {
            var data = JsonSerializer.Serialize(new 
            { 
                car.Lane, 
                car.Distance, 
                car.Speed, 
                car.Health, 
                score 
            });
            var bytes = Encoding.UTF8.GetBytes(data + "\n");
            await _stream.WriteAsync(bytes, 0, bytes.Length);
        }
        catch { }
    }
    
    public async Task<(int lane, float distance, float speed, int health, int score)?> ReceiveGameState()
    {
        if (_stream == null) return null;
        
        try
        {
            var buffer = new byte[1024];
            var bytesRead = await _stream.ReadAsync(buffer, 0, buffer.Length);
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
        _stream?.Close();
        _client?.Close();
        _server?.Stop();
    }
    
    private static string GetLocalIPAddress()
    {
        var hostName = Dns.GetHostName();
        var hostEntry = Dns.GetHostEntry(hostName);
        return hostEntry.AddressList
            .FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork)?
            .ToString() ?? "localhost";
    }
    
    private async Task TryResolveHostname(string host)
    {
        try
        {
            Console.WriteLine($"Resolving hostname: {host}...");
            var addresses = await Dns.GetHostAddressesAsync(host);
            if (addresses.Length > 0)
            {
                Console.WriteLine($"✓ Resolved to: {addresses[0]}\n");
            }
        }
        catch (Exception dnsEx)
        {
            Console.WriteLine($"⚠️  DNS resolution failed: {dnsEx.Message}\n");
        }
    }
    
    private async Task<bool> TryConnect(string host, int port, int attempt)
    {
        try
        {
            Console.Write($"Attempt {attempt}/{GameConstants.ConnectAttempts}: Connecting...");
            
            _client = new TcpClient
            {
                ReceiveTimeout = 15000,
                SendTimeout = 15000
            };
            
            var connectTask = _client.ConnectAsync(host, port);
            var timeoutTask = Task.Delay(TimeSpan.FromSeconds(15));
            
            var completedTask = await Task.WhenAny(connectTask, timeoutTask);
            
            if (completedTask == timeoutTask)
            {
                Console.WriteLine(" ⏱️  Timeout (15s)");
                try { _client.Close(); } catch { }
                return false;
            }
            
            if (!connectTask.IsCompletedSuccessfully)
            {
                await connectTask;
            }
            
            _stream = _client.GetStream();
            IsServer = false;
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($" ❌ Failed: {ex.GetType().Name}");
            try { _client?.Close(); } catch { }
            return false;
        }
    }
    
    private static void PrintServerStartMessage(int port, string localIP)
    {
        Console.WriteLine("\n╔════════════════════════════════════════╗");
        Console.WriteLine("║   🌐 SERVER STARTED                    ║");
        Console.WriteLine("╠════════════════════════════════════════╣");
        Console.WriteLine($"║ Port: {port,-32} ║");
        Console.WriteLine($"║ Local IP: {localIP,-28} ║");
        Console.WriteLine("║ Status: Waiting for connection...      ║");
        Console.WriteLine("╚════════════════════════════════════════╝\n");
    }
    
    private static void PrintConnectingMessage(string host, int port)
    {
        Console.WriteLine("\n╔════════════════════════════════════════╗");
        Console.WriteLine("║   🌐 CONNECTING TO HOST                ║");
        Console.WriteLine("╠════════════════════════════════════════╣");
        Console.WriteLine($"║ Host: {host,-32} ║");
        Console.WriteLine($"║ Port: {port,-32} ║");
        Console.WriteLine("╚════════════════════════════════════════╝\n");
    }
    
    private static void PrintConnectionFailedMessage(string host)
    {
        Console.WriteLine("\n╔════════════════════════════════════════╗");
        Console.WriteLine("║   ❌ CONNECTION FAILED                 ║");
        Console.WriteLine("╠════════════════════════════════════════╣");
        Console.WriteLine($"║ Could not connect after {GameConstants.ConnectAttempts} attempts      ║");
        Console.WriteLine("╠════════════════════════════════════════╣");
        Console.WriteLine("║ Possible causes:                       ║");
        Console.WriteLine("║ 1. Host is not running the game        ║");
        Console.WriteLine("║ 2. Wrong IP address entered            ║");
        Console.WriteLine("║ 3. Firewall blocking port              ║");
        Console.WriteLine("║ 4. Network connectivity issue          ║");
        Console.WriteLine("╠════════════════════════════════════════╣");
        Console.WriteLine("║ Solutions:                             ║");
        Console.WriteLine($"║ • Verify host IP: {host,-24} ║");
        Console.WriteLine("║ • Check host is running multiplayer    ║");
        Console.WriteLine("║ • Disable firewall temporarily         ║");
        Console.WriteLine("╚════════════════════════════════════════╝\n");
    }
}