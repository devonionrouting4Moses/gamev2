namespace TerminalRacer.Audio.Interfaces;

public interface IAudioManager
{
    void PlaySound(string soundFile, float volume = 0.5f);
    void PlayMusic(string musicFile, bool loop = true);
    void StopMusic();
}