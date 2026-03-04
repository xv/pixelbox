// Copyright 2025 Jad Altahan <xv.git@aol.com>
// SPDX-License-Identifier: MIT

using Windows.Win32.UI.WindowsAndMessaging;
using Windows.Win32;
using System.Windows;

namespace PixelBox;

/// <summary>
/// Provides information about the system's virtual screen.
/// </summary>
internal static class VirtualScreenInfo
{
    private static Int32Rect _bounds;

    /// <summary>
    /// Gets the x-coordinate of the left edge of the virtual screen.
    /// </summary>
    public static int Left => _bounds.X;

    /// <summary>
    /// Gets the y-coordinate of the top edge of the virtual screen.
    /// </summary>
    public static int Top => _bounds.Y;

    /// <summary>
    /// Gets the x-coordinate of the right edge of the virtual screen.
    /// </summary>
    public static int Right => _bounds.X + _bounds.Width;

    /// <summary>
    /// Gets the y-coordinate of the bottom edge of the virtual screen.
    /// </summary>
    public static int Bottom => _bounds.Y + _bounds.Height;

    /// <summary>
    /// Gets the bounding rectangle of the virtual screen.
    /// </summary>
    public static Int32Rect Bounds => _bounds;

    /// <summary>
    /// Updates the cached virtual screen information by querying it again.
    /// </summary>
    public static void Refresh()
    {
        var x = PInvoke.GetSystemMetrics(SYSTEM_METRICS_INDEX.SM_XVIRTUALSCREEN);
        var y = PInvoke.GetSystemMetrics(SYSTEM_METRICS_INDEX.SM_YVIRTUALSCREEN);
        var w = PInvoke.GetSystemMetrics(SYSTEM_METRICS_INDEX.SM_CXVIRTUALSCREEN);
        var h = PInvoke.GetSystemMetrics(SYSTEM_METRICS_INDEX.SM_CYVIRTUALSCREEN);

        _bounds = new Int32Rect(x, y, w, h);
    }

    /// <summary>
    /// Initializes static members of <see cref="VirtualScreenInfo"/>.
    /// </summary>
    static VirtualScreenInfo()
    {
        Refresh();
    }
}