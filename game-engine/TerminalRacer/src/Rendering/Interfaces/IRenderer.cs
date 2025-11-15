using TerminalRacer.Rendering.FFI;

namespace TerminalRacer.Rendering.Interfaces;

public interface IRenderer
{
    bool Initialize();
    void Cleanup();
    bool PollInput(ref InputState input);
    bool Render(ref GameState state);
    bool RenderMenu(string title, string[] options, int selected);
}