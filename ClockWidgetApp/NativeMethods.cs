using System.Runtime.InteropServices;

namespace ClockWidgetApp
{
    /// <summary>
    /// Класс для вызова WinAPI методов.
    /// </summary>
    internal static class NativeMethods
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool SetForegroundWindow(IntPtr hWnd);
    }
} 