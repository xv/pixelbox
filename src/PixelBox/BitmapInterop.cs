using System.Diagnostics.CodeAnalysis;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows;

using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32;

namespace PixelBox;

/// <summary>
/// Contains interop methods for screen capture and bitmap conversion.
/// </summary>
internal static class BitmapInterop
{
    /// <summary>
    /// Captures a rectangular region of the screen into a <see cref="HBITMAP"/>.
    /// </summary>
    /// 
    /// <param name="left">
    /// The X-coordinate, in screen pixels, of the upper-left corner of the
    /// capture area.
    /// </param>
    /// 
    /// <param name="top">
    /// The Y-coordinate, in screen pixels, of the upper-left corner of the
    /// capture area.
    /// </param>
    /// 
    /// <param name="width">
    /// The width, in pixels, of the area to capture.
    /// </param>
    /// 
    /// <param name="height">
    /// The height, in pixels, of the area to capture.
    /// </param>
    /// 
    /// <returns>
    /// Handle to a <see cref="HBITMAP"/> containing the captured image if
    /// successful; otherwise, <see cref="HBITMAP.Null"/> if the operation fails.
    /// </returns>
    /// 
    /// <remarks>
    /// It is the caller's responsibility to manually delete the returned
    /// <see cref="HBITMAP"/> via a call to <c>DeleteObject()</c> when it is
    /// no longer needed.
    /// </remarks>
    [SuppressMessage(
        "Performance",
        "CA1806:Do not ignore method results",
        Justification = "Unnecessary ReleaseDC() return results.")]
    public static HBITMAP CaptureRectToHBitmap(int left, int top, int width, int height)
    {
        var hdcScreen = PInvoke.GetDC(HWND.Null);
        if (hdcScreen == HDC.Null)
            return HBITMAP.Null;

        var hdcMem = PInvoke.CreateCompatibleDC(hdcScreen);
        if (hdcMem == HDC.Null)
        {
            PInvoke.ReleaseDC(HWND.Null, hdcScreen);
            return HBITMAP.Null;
        }

        var hBitmap = PInvoke.CreateCompatibleBitmap(hdcScreen, width, height);
        if (hBitmap == HBITMAP.Null)
        {
            PInvoke.DeleteDC(hdcMem);
            PInvoke.ReleaseDC(HWND.Null, hdcScreen);
            return HBITMAP.Null;
        }

        var hOld = PInvoke.SelectObject(hdcMem, hBitmap);

        var success = PInvoke.BitBlt(
            hdcMem,
            0, 0, width, height,
            hdcScreen,
            left, top,
            ROP_CODE.SRCCOPY | ROP_CODE.CAPTUREBLT);

        PInvoke.SelectObject(hdcMem, hOld);
        PInvoke.DeleteDC(hdcMem);
        PInvoke.ReleaseDC(HWND.Null, hdcScreen);

        if (!success)
        {
            PInvoke.DeleteObject(hBitmap);
            hBitmap = HBITMAP.Null;
        }

        return hBitmap;
    }

    /// <inheritdoc cref="CaptureRectToHBitmap(int, int, int, int)"/>
    /// 
    /// <param name="rect">
    /// Rectangle containing the region of the screen to capture.
    /// </param>
    public static HBITMAP CaptureRectToHBitmap(Int32Rect rect) =>
        CaptureRectToHBitmap(rect.X, rect.Y, rect.Width, rect.Height);

    /// <summary>
    /// Creates a <see cref="BitmapSource"/> from a Win32 <see cref="HBITMAP"/>.
    /// </summary>
    /// 
    /// <param name="hBitmap">
    /// <see cref="HBITMAP"/> to create a <see cref="BitmapSource"/> from.
    /// </param>
    /// 
    /// <param name="freeze">
    /// Specifies whether the created <see cref="BitmapSource"/> should be made
    /// unmodifiable.
    /// </param>
    /// 
    /// <returns>
    /// A <see cref="BitmapSource"/> created from <paramref name="hBitmap"/>.
    /// </returns>
    public static BitmapSource ToBitmapSource(this HBITMAP hBitmap, bool freeze)
    {
        var bsrc = Imaging.CreateBitmapSourceFromHBitmap(
            hBitmap,
            nint.Zero,
            Int32Rect.Empty,
            BitmapSizeOptions.FromEmptyOptions());

        if (freeze && bsrc.CanFreeze)
            bsrc.Freeze();

        return bsrc;
    }
}