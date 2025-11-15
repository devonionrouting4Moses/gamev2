namespace App.UI;

public static class GameOverScreen
{
    public static void Show(int score, float distance, double time, int level, int combo)
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("\n╔════════════════════════════════════════╗");
        Console.WriteLine("║          GAME OVER!                    ║");
        Console.WriteLine("╚════════════════════════════════════════╝\n");
        Console.ResetColor();
        
        Console.WriteLine($"  Final Score:    {score:N0}");
        Console.WriteLine($"  Distance:       {distance:N0}m");
        Console.WriteLine($"  Time:           {time:F2}s");
        Console.WriteLine($"  Max Combo:      x{combo}");
        Console.WriteLine($"  Level:          {level}");
    }
}
