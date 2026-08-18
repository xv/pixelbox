// Copyright 2025 Jad Altahan <xv.git@aol.com>
// SPDX-License-Identifier: MIT

using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32;

namespace PixelBox;

/// <summary>
/// Represents a persistent device-independent bitmap (DIB) section that allows
/// direct memory access to pixel data.
/// </summary>
internal sealed unsafe class PersistentDibSection : IDisposable
{
    private int
        _width,
        _height,
        _stride;

    private HDC
        _hdcScreen,
        _hdcMem;

    private HBITMAP _hBitmap;

    private void* _pBits;

    /// <summary>
    /// Gets the width of the DIB section in pixels.
    /// </summary>
    public int Width => _width;

    /// <summary>
    /// Gets the height of the DIB section in pixels.
    /// </summary>
    public int Height => _height;

    /// <summary>
    /// Gets the stride of the DIB section.
    /// </summary>
    public int Stride => _stride;

    /// <summary>
    /// Gets a pointer to the raw pixel data of the DIB section.
    /// </summary>
    public void* Bits => _pBits;

    /// <summary>
    /// Computes the stride of a bitmap given its width and bits per pixel.
    /// </summary>
    ///
    /// <param name="width">
    /// Width of the bitmap in pixels.
    /// </param>
    /// 
    /// <param name="bitsPerPixel">
    /// Number of bits per pixel.
    /// </param>
    ///
    /// <returns>
    /// The stride, rounded up to the nearest DWORD.
    /// </returns>
    private static int ComputeStride(int width, int bitsPerPixel) =>
        // https://learn.microsoft.com/en-us/windows/win32/api/wingdi/ns-wingdi-bitmapinfoheader#calculating-surface-stride
        (((width * bitsPerPixel) + 31) & ~31) >> 3;

    /// <summary>
    /// Initializes a new instance of the <see cref="PersistentDibSection"/>
    /// class.
    /// </summary>
    /// 
    /// <exception cref="InvalidOperationException"/>
    public PersistentDibSection()
    {
        _hdcScreen = PInvoke.GetDC(HWND.Null);
        if (_hdcScreen == HDC.Null)
            throw new InvalidOperationException("GetDC failed.");

        _hdcMem = PInvoke.CreateCompatibleDC(_hdcScreen);
        if (_hdcMem == HDC.Null)
        {
            PInvoke.ReleaseDC(HWND.Null, _hdcScreen);
            throw new InvalidOperationException("CreateCompatibleDC failed.");
        }
    }

    /// <inheritdoc cref="PersistentDibSection()"/>
    /// 
    /// <param name="width">
    /// Width of the bitmap in pixels. Must be greater than zero.
    /// </param>
    /// 
    /// <param name="height">
    /// Height of the bitmap in pixels. Must be greater than zero.
    /// </param>
    /// 
    /// <exception cref="ArgumentOutOfRangeException"/>
    public PersistentDibSection(int width, int height) : this()
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);

        Resize(width, height);
    }

    /// <summary>
    /// Resizes the DIB section, recreating the bitmap if necessary.
    /// </summary>
    ///
    /// <param name="newWidth">
    /// The new bitmap width in pixels.
    /// </param>
    /// 
    /// <param name="newHeight">
    /// The new bitmap height in pixels.
    /// </param>
    public void Resize(int newWidth, int newHeight)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(newWidth);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(newHeight);

        if (_hBitmap != HBITMAP.Null &&
            newWidth == _width && newHeight == _height)
            return;

        // Delete the old bitmap since there is no way to change its size
        // once it's created
        if (_hBitmap != HBITMAP.Null)
        {
            PInvoke.DeleteObject(_hBitmap);

            _hBitmap = HBITMAP.Null;
            _pBits = null;
        }

        CreateDIBSection(newWidth, newHeight);
    }

    /// <summary>
    /// Creates a persistent DIB that can be updated through
    /// <see cref="Capture(int, int)"/>.
    /// </summary>
    /// 
    /// <param name="width">
    /// Bitmap width in pixels.
    /// </param>
    /// 
    /// <param name="height">
    /// Bitmap height in pixels.
    /// </param>
    /// 
    /// <exception cref="InvalidOperationException"></exception>
    private void CreateDIBSection(int width, int height)
    {
        _width = width;
        _height = height;

        var bmi = new BITMAPINFO
        {
            bmiHeader = new BITMAPINFOHEADER
            {
                biSize = (uint)sizeof(BITMAPINFOHEADER),
                biWidth = width,
                biHeight = -height,
                biPlanes = 1,
                biBitCount = 32,
                biCompression = (uint)BI_COMPRESSION.BI_RGB
            }
        };

        _stride = ComputeStride(width, bmi.bmiHeader.biBitCount);

        void* pBits = null;

        _hBitmap = PInvoke.CreateDIBSection(
            _hdcScreen,
            &bmi,
            DIB_USAGE.DIB_RGB_COLORS,
            &pBits,
            HANDLE.Null,
            0);

        if (_hBitmap == HBITMAP.Null || pBits == null)
            throw new InvalidOperationException("Failed to create DIB section.");

        _pBits = pBits;

        PInvoke.SelectObject(_hdcMem, _hBitmap);
    }

    /// <summary>
    /// Captures a region of the screen into the DIB section.
    /// </summary>
    /// 
    /// <param name="left">
    /// The x-coordinate of the top-left corner of the screen region.
    /// </param>
    /// 
    /// <param name="top">
    /// The y-coordinate of the top-left corner of the screen region.
    /// </param>
    /// 
    /// <returns>
    /// <see langword="true"/> if the capture succeeded; <see langword="false"/>
    /// otherwise.
    /// </returns>
    public bool Capture(int left, int top)
    {
        var outOfBounds =
            (left < VirtualScreenInfo.Left) ||
            (top < VirtualScreenInfo.Top) ||
            ((left + _width) > VirtualScreenInfo.Right) ||
            ((top + _height) > VirtualScreenInfo.Bottom);

        // Because one persistent bitmap in memory and BitBlt updates its contents
        // (as opposed to retrieving a fresh DIB every capture), if BitBlt tries
        // to copy from an off-screen region (e.g., negative left or top), it will
        // quietly fail to capture those out-of-bounds pixels. This can cause visual
        // artifacts where "dragged" or "smeared" pixels from valid regions will
        // appear moving along parts of the bitmap are never updated. Therefore,
        // paint the bitmap black first before calling BitBlt for a more desirable
        // visual result
        if (outOfBounds)
        {
            // Clearing the bitmap back buffer is an alternative approach although
            // it does not appear any faster than using PatBlt, but could perhaps
            // use further testing
            // NativeMemory.Clear(_pBits, (nuint)(_stride * _height));

            PInvoke.PatBlt(
                _hdcMem,
                0, 0,
                _width, _height,
                ROP_CODE.BLACKNESS);
        }

        return PInvoke.BitBlt(
            _hdcMem,
            0, 0,
            _width, _height,
            _hdcScreen,
            left, top,
            ROP_CODE.SRCCOPY);
    }

    /// <summary>
    /// Releases all resources used by the <see cref="PersistentDibSection"/>
    /// instance.
    /// </summary>
    public void Dispose()
    {
        if (_hBitmap != HBITMAP.Null)
        {
            PInvoke.DeleteObject(_hBitmap);
            _hBitmap = HBITMAP.Null;
        }

        if (_hdcMem != HDC.Null)
        {
            PInvoke.DeleteDC(_hdcMem);
            _hdcMem = HDC.Null;
        }

        if (_hdcScreen != HDC.Null)
        {
            PInvoke.ReleaseDC(HWND.Null, _hdcScreen);
            _hdcScreen = HDC.Null;
        }

        _pBits = null;
    }
}
