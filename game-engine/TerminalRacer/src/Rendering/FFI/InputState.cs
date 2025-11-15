using System.Runtime.InteropServices;

namespace TerminalRacer.Rendering.FFI;

[StructLayout(LayoutKind.Sequential)]
public struct InputState
{
    [MarshalAs(UnmanagedType.I1)] public bool P1Left;
    [MarshalAs(UnmanagedType.I1)] public bool P1Right;
    [MarshalAs(UnmanagedType.I1)] public bool P1Accel;
    [MarshalAs(UnmanagedType.I1)] public bool P1Brake;
    [MarshalAs(UnmanagedType.I1)] public bool P1Boost;
    
    [MarshalAs(UnmanagedType.I1)] public bool P2Left;
    [MarshalAs(UnmanagedType.I1)] public bool P2Right;
    [MarshalAs(UnmanagedType.I1)] public bool P2Accel;
    [MarshalAs(UnmanagedType.I1)] public bool P2Brake;
    [MarshalAs(UnmanagedType.I1)] public bool P2Boost;
    
    [MarshalAs(UnmanagedType.I1)] public bool Quit;
    [MarshalAs(UnmanagedType.I1)] public bool Pause;
    [MarshalAs(UnmanagedType.I1)] public bool Menu;
}