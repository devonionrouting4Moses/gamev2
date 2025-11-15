using System.Diagnostics;
using TerminalRacer.Audio.Interfaces;

namespace TerminalRacer.Audio;

public class AudioManager : IAudioManager
{
    private Process? _currentPlayer;
    
    public void PlaySound(string soundFile, float volume = 0.5f)
    {
        Task.Run(() =>
        {
            try
            {
                var playerCmd = File.Exists("/usr/bin/mpg123") ? "mpg123" : "aplay";
                var args = playerCmd == "mpg123" 
                    ? $"-q --gain {(int)(volume * 100)} {soundFile}" 
                    : soundFile;
                
                var process = Process.Start(new ProcessStartInfo
                {
                    FileName = playerCmd,
                    Arguments = args,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                });
                
                process?.WaitForExit(1000);
            }
            catch { /* Audio is optional */ }
        });
    }
    
    public void PlayMusic(string musicFile, bool loop = true)
    {
        StopMusic();
        
        Task.Run(() =>
        {
            try
            {
                var args = loop ? $"-q --loop -1 {musicFile}" : $"-q {musicFile}";
                _currentPlayer = Process.Start(new ProcessStartInfo
                {
                    FileName = "mpg123",
                    Arguments = args,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                });
            }
            catch { /* Music is optional */ }
        });
    }
    
    public void StopMusic()
    {
        if (_currentPlayer != null && !_currentPlayer.HasExited)
        {
            try
            {
                _currentPlayer.Kill();
                _currentPlayer.Dispose();
            }
            catch { }
            _currentPlayer = null;
        }
    }
}