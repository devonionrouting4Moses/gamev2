using System.Runtime.InteropServices;

namespace TerminalRacer.Rendering.FFI;

public static class RatatuiFFI
{
    private const string LibName = "rust_renderer";
    
    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    public static extern bool ratatui_init();
    
    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void ratatui_cleanup();
    
    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    public static extern bool ratatui_poll_input(ref InputState input);
    
    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    public static extern bool ratatui_render(ref GameState state);
    
    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    public static extern bool ratatui_render_menu(
        [MarshalAs(UnmanagedType.LPStr)] string title,
        [MarshalAs(UnmanagedType.LPArray)] string[] options,
        int optionCount,
        int selected);
}