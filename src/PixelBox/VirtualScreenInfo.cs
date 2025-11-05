using Windows.Win32.UI.WindowsAndMessaging;
using Windows.Win32;
using System.Windows;

namespace PixelBox;

/// <summary>
/// Provides information about the system's virtual screen.
/// </summary>
internal static class VirtualScreenInfo
{
    private static int
        _left,
        _top,
        _right,
        _bottom;

    private static Int32Rect _bounds;

    /// <summary>
    /// Gets the x-coordinate of the left edge of the virtual screen.
    /// </summary>
    public static int Left => _left;

    /// <summary>
    /// Gets the y-coordinate of the top edge of the virtual screen.
    /// </summary>
    public static int Top => _top;

    /// <summary>
    /// Gets the x-coordinate of the right edge of the virtual screen.
    /// </summary>
    public static int Right => _right;

    /// <summary>
    /// Gets the y-coordinate of the bottom edge of the virtual screen.
    /// </summary>
    public static int Bottom => _bottom;

    /// <summary>
    /// Gets the bounding rectangle of the virtual screen.
    /// </summary>
    public static Int32Rect Bounds => _bounds;

    /// <summary>
    /// Updates the cached virtual screen information by querying it again.
    /// </summary>
    public static void Refresh()
    {
        _left = PInvoke.GetSystemMetrics(SYSTEM_METRICS_INDEX.SM_XVIRTUALSCREEN);
        _top = PInvoke.GetSystemMetrics(SYSTEM_METRICS_INDEX.SM_YVIRTUALSCREEN);

        var vWidth = PInvoke.GetSystemMetrics(SYSTEM_METRICS_INDEX.SM_CXVIRTUALSCREEN);
        var vHeight = PInvoke.GetSystemMetrics(SYSTEM_METRICS_INDEX.SM_CYVIRTUALSCREEN);

        _right = _left + vWidth;
        _bottom = _top + vHeight;

        _bounds.X = _left;
        _bounds.Y = _top;
        _bounds.Width = vWidth;
        _bounds.Height = vHeight;
    }

    /// <summary>
    /// Initializes static members of <see cref="VirtualScreenInfo"/>.
    /// </summary>
    static VirtualScreenInfo()
    {
        Refresh();
    }
}