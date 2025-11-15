using System.Text.Json;
using TerminalRacer.Core.Constants;
using TerminalRacer.GameLogic.Interfaces;

namespace TerminalRacer.GameLogic.Services;

public class ScoreService : IScoreService
{
    public void SaveHighScore(string player, int score, float distance, double time, int level, string mode)
    {
        try
        {
            var scores = LoadHighScores();
            scores.Add(new Dictionary<string, object>
            {
                ["player"] = player,
                ["score"] = score,
                ["distance"] = distance,
                ["time"] = time,
                ["level"] = level,
                ["mode"] = mode,
                ["date"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            });
            
            scores = scores.OrderByDescending(s => Convert.ToInt32(s["score"])).Take(10).ToList();
            
            var json = JsonSerializer.Serialize(scores, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(GameConstants.ScoreFile, json);
        }
        catch { }
    }
    
    public List<Dictionary<string, object>> LoadHighScores()
    {
        try
        {
            if (File.Exists(GameConstants.ScoreFile))
            {
                var json = File.ReadAllText(GameConstants.ScoreFile);
                return JsonSerializer.Deserialize<List<Dictionary<string, object>>>(json) ?? new();
            }
        }
        catch { }
        
        return new List<Dictionary<string, object>>();
    }
    
    public void DisplayHighScores()
    {
        var scores = LoadHighScores();
        
        Console.WriteLine("\n╔════════════════════════════════════════╗");
        Console.WriteLine("║          HIGH SCORES                   ║");
        Console.WriteLine("╠════════════════════════════════════════╣");
        
        for (int i = 0; i < Math.Min(5, scores.Count); i++)
        {
            var s = scores[i];
            
            int score = GetIntValue(s, "score");
            int level = GetIntValue(s, "level");
            string player = GetStringValue(s, "player");
            
            Console.WriteLine($"║ {i+1}. {player,-10} {score,8:N0}  Lv.{level,-2} ║");
        }
        
        Console.WriteLine("╚════════════════════════════════════════╝\n");
    }
    
    private static int GetIntValue(Dictionary<string, object> dict, string key)
    {
        if (dict[key] is JsonElement elem)
            return elem.GetInt32();
        return Convert.ToInt32(dict[key]);
    }
    
    private static string GetStringValue(Dictionary<string, object> dict, string key)
    {
        if (dict[key] is JsonElement elem)
            return elem.GetString() ?? "Unknown";
        return dict[key]?.ToString() ?? "Unknown";
    }
}