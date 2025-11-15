namespace TerminalRacer.GameLogic.Interfaces;

public interface IScoreService
{
    void SaveHighScore(string player, int score, float distance, double time, int level, string mode);
    List<Dictionary<string, object>> LoadHighScores();
    void DisplayHighScores();
}