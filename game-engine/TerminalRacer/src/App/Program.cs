using TerminalRacer.App.UI;
using TerminalRacer.Audio;
using TerminalRacer.GameLogic.Engine;
using TerminalRacer.Networking;
using TerminalRacer.Rendering;

namespace TerminalRacer.App;

class Program
{
    static async Task Main(string[] args)
    {
        var (mode, networkMode, isHost, hostIp) = MenuSystem.ShowMainMenu();
        
        // Setup dependencies
        var renderer = new RatatuiRenderer();
        var audio = new AudioManager();
        MultiplayerManager? multiplayer = null;
        
        // Network multiplayer setup
        if (networkMode)
        {
            multiplayer = new MultiplayerManager();
            
            if (isHost)
            {
                if (!await multiplayer.StartServer())
                    return;
            }
            else
            {
                if (!await multiplayer.ConnectToServer(hostIp ?? "localhost"))
                    return;
            }
            
            Thread.Sleep(1000);
        }
        
        Console.WriteLine("\n🚀 Starting game...\n");
        Thread.Sleep(500);
        
        try
        {
            var game = new RacingGame(mode, renderer, audio, multiplayer);
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
            renderer.Cleanup();
            audio.StopMusic();
            multiplayer?.Disconnect();
        }
        
        Console.WriteLine("\n🏁 Thanks for playing Terminal Racer Ultimate Edition!");
    }
}