using TerminalRacer.Rendering.FFI;
using TerminalRacer.Rendering.Interfaces;

namespace TerminalRacer.Rendering;

public class RatatuiRenderer : IRenderer
{
    public bool Initialize() => RatatuiFFI.ratatui_init();
    
    public void Cleanup() => RatatuiFFI.ratatui_cleanup();
    
    public bool PollInput(ref InputState input) => RatatuiFFI.ratatui_poll_input(ref input);
    
    public bool Render(ref GameState state) => RatatuiFFI.ratatui_render(ref state);
    
    public bool RenderMenu(string title, string[] options, int selected) =>
        RatatuiFFI.ratatui_render_menu(title, options, options.Length, selected);
}
