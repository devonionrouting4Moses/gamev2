using TerminalRacer.Core.Enums;

namespace TerminalRacer.App.UI;

public static class MenuSystem
{
    public static (GameMode mode, bool networkMode, bool isHost, string? hostIp) ShowMainMenu()
    {
        Console.Clear();
        Console.WriteLine("╔════════════════════════════════════════╗");
        Console.WriteLine("║   TERMINAL RACER - ULTIMATE EDITION    ║");
        Console.WriteLine("╚════════════════════════════════════════╝");
        Console.WriteLine("\n🏎️  Select Game Mode:");
        Console.WriteLine("  1. Single Player");
        Console.WriteLine("  2. Split-Screen (Local)");
        Console.WriteLine("  3. Career Mode");
        Console.WriteLine("  4. Replay Mode");
        Console.WriteLine("  5. Network Multiplayer");
        Console.WriteLine("  6. Exit");
        Console.Write("\nChoice: ");
        
        var choice = Console.ReadLine();
        
        if (choice == "6") 
            Environment.Exit(0);
        
        var mode = choice switch
        {
            "1" => GameMode.Single,
            "2" => GameMode.Splitscreen,
            "3" => GameMode.Career,
            "4" => GameMode.Replay,
            "5" => GameMode.Splitscreen,
            _ => GameMode.Single
        };
        
        bool networkMode = choice == "5";
        bool isHost = false;
        string? hostIp = null;
        
        if (networkMode)
        {
            Console.Write("\nHost (h) or Join (j)? ");
            var netChoice = Console.ReadLine()?.ToLower();
            isHost = netChoice == "h";
            
            if (!isHost)
            {
                Console.Write("Enter host IP: ");
                hostIp = Console.ReadLine();
            }
        }
        
        return (mode, networkMode, isHost, hostIp);
    }
}