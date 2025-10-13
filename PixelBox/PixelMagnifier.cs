using System.ComponentModel;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32;

namespace PixelBox;

/// <summary>
/// Represents a pixel magnification control.
/// </summary>
public class PixelMagnifier : FrameworkElement, IDisposable
{
    #region Fields

    private readonly DispatcherTimer _refreshTimer;

    private int _pixelColumnsHalf;
    private int _gridGap = 1;

    private Color _centerPixelColor;
    private Point _lastMousePos = new(-1, -1);
    private Point _lockedPixelPos = new(-1, -1);

    private int _captureWidth, _captureHeight;
    private byte[]? _captureBuffer;
    private int _captureStride;

    private WriteableBitmap? _expandedBitmap;
    private byte[]? _expandedBuffer;
    private int _expandedDevSize = 0;

    public event EventHandler<PixelChangedEventArgs>? PixelChanged;

    #endregion
    #region Dependency Properties

    public static readonly DependencyProperty PixelColumnsProperty =
        DependencyProperty.Register(
            nameof(PixelColumns),
            typeof(int),
            typeof(PixelMagnifier),
            new FrameworkPropertyMetadata(11, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender, OnPixelColumnsChanged));

    public static readonly DependencyProperty PixelSizeProperty =
        DependencyProperty.Register(
            nameof(PixelSize),
            typeof(int),
            typeof(PixelMagnifier),
            new FrameworkPropertyMetadata(9, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender, OnPixelSizeChanged));

    public static readonly DependencyProperty ShowGridProperty =
        DependencyProperty.Register(
            nameof(ShowGrid),
            typeof(bool),
            typeof(PixelMagnifier),
            new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender, OnShowGridChanged));

    public static readonly DependencyProperty SamplingModeProperty =
        DependencyProperty.Register(
            nameof(SamplingMode),
            typeof(PixelSamplingMode),
            typeof(PixelMagnifier),
            new FrameworkPropertyMetadata(PixelSamplingMode.Single, FrameworkPropertyMetadataOptions.AffectsRender, OnSamplingModeChanged));

    public static readonly DependencyProperty RefreshIntervalProperty =
        DependencyProperty.Register(
            nameof(RefreshInterval),
            typeof(int),
            typeof(PixelMagnifier),
            new FrameworkPropertyMetadata(30, OnRefreshIntervalChanged));

    #endregion
    #region Properties

    /// <summary>
    /// Gets whether the control can capture pixels and redraw.
    /// </summary>
    [Browsable(false)]
    public bool CanCapture => _refreshTimer.IsEnabled;

    /// <summary>
    /// Gets or sets whether the current coordinate should be locked,
    /// preventing further mouse movement from tracking new pixels.
    /// </summary>
    [Browsable(false)]
    public bool LockPixelPosition
    {
        get => _lockedPixelPos.X >= 0 && _lockedPixelPos.Y >= 0;
        set => _lockedPixelPos = value ? _lastMousePos : new Point(-1, -1);
    }

    /// <summary>
    /// Gets or sets the number of pixel columns.
    /// </summary>
    /// 
    /// <remarks>
    /// It is recommended to use odd values so that the position of center
    /// pixel is symmetric horizontally and vertically.
    /// </remarks>
    [Category("Appearance")]
    [Description("Sets the number of pixel columns in the grid.")]
    public int PixelColumns
    {
        get => (int)GetValue(PixelColumnsProperty);
        set => SetValue(PixelColumnsProperty, value);
    }

    /// <summary>
    /// Gets the screen coordinates corresponding to the pixel located at
    /// the center of the grid.
    /// </summary>
    [Browsable(false)]
    public Point PixelPosition => _lastMousePos;

    /// <summary>
    /// Gets or sets the size of the pixel cells.
    /// </summary>
    [Category("Appearance")]
    [Description("Sets the size (in px) of individual pixel cells in the grid.")]
    public int PixelSize
    {
        get => (int)GetValue(PixelSizeProperty);
        set => SetValue(PixelSizeProperty, value);
    }

    /// <summary>
    /// Gets the current mouse position in screen coordinates.
    /// </summary>
    private static Point MousePosition
    {
        get
        {
            PInvoke.GetCursorPos(out System.Drawing.Point p);
            return new Point(p.X, p.Y);
        }
    }

    /// <summary>
    /// Gets or sets the pixel color sampling mode.
    /// </summary>
    [Category("Behavior")]
    [Description("Determines the pixel color sampling mode.")]
    public PixelSamplingMode SamplingMode
    {
        get => (PixelSamplingMode)GetValue(SamplingModeProperty);
        set => SetValue(SamplingModeProperty, value);
    }

    /// <summary>
    /// Gets or sets the redraw rate of the control.
    /// </summary>
    [Category("Behavior")]
    [Description("Sets refresh rate (in ms) of the control.")]
    public int RefreshInterval
    {
        get => (int)GetValue(RefreshIntervalProperty);
        set => SetValue(RefreshIntervalProperty, value);
    }

    /// <summary>
    /// Gets or sets whether the rows and columns of pixels should be
    /// visible.
    /// </summary>
    [Category("Appearance")]
    [Description("Specifies whether the pixel grid should be shown.")]
    public bool ShowGrid
    {
        get => (bool)GetValue(ShowGridProperty);
        set => SetValue(ShowGridProperty, value);
    }

    #endregion
    #region Event Handling

    private static void OnPixelColumnsChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is PixelMagnifier mag)
        {
            var value = (int)e.NewValue;
            if (value < 1)
                throw new ArgumentOutOfRangeException(null, nameof(PixelColumns));

            mag._captureWidth = value;
            mag._captureHeight = value;
            mag._pixelColumnsHalf = value / 2;

            mag.InvalidateMeasure();
        }
    }

    private static void OnPixelSizeChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is PixelMagnifier mag)
        {
            var value = (int)e.NewValue;
            if (value < 1 || value > 100)
                throw new ArgumentOutOfRangeException(null, nameof(PixelColumns));

            mag.InvalidateMeasure();
        }
    }

    private static void OnSamplingModeChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is PixelMagnifier mag)
        {
            mag.InvalidateVisual();
            mag.PixelChanged?.Invoke(mag, new PixelChangedEventArgs(mag._centerPixelColor, mag._lastMousePos));
        }
    }

    private static void OnShowGridChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is PixelMagnifier mag)
            mag.InvalidateMeasure();
    }

    private static void OnRefreshIntervalChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is PixelMagnifier mag)
        {
            var value = (int)e.NewValue;
            if (value < 1 || value > 1000)
                throw new ArgumentOutOfRangeException(null, nameof(RefreshInterval));

            mag._refreshTimer.Interval = TimeSpan.FromMilliseconds(value);
        }
    }

    private void OnRefreshTimerTick(object? sender, EventArgs e)
    {
        var isLocked = LockPixelPosition;
        var currentPos = isLocked ? _lockedPixelPos : MousePosition;

        if (!isLocked && currentPos == _lastMousePos)
            return;

        _lastMousePos = currentPos;

        CaptureAt(_lastMousePos);
        InvalidateVisual();
    }

    #endregion


    public PixelMagnifier()
    {
        SetValue(RenderOptions.EdgeModeProperty, EdgeMode.Aliased);
        RenderOptions.SetBitmapScalingMode(this, BitmapScalingMode.NearestNeighbor);

        SnapsToDevicePixels = true;
        Focusable = false;

        _captureWidth = PixelColumns;
        _captureHeight = PixelColumns;
        _pixelColumnsHalf = PixelColumns / 2;

        _refreshTimer = new DispatcherTimer(DispatcherPriority.Render)
        {
            Interval = TimeSpan.FromMilliseconds(RefreshInterval),
            IsEnabled = false
        };
        _refreshTimer.Tick += OnRefreshTimerTick;
    }

    /// <summary>
    /// Begins capturing pixels.
    /// </summary>
    public void StartCapture() => _refreshTimer.Start();

    /// <summary>
    /// Stops capturing pixels.
    /// </summary>
    public void StopCapture() => _refreshTimer.Stop();

    /// <summary>
    /// Toggles the pixel capture.
    /// </summary>
    public void ToggleCapture() => _refreshTimer.IsEnabled = !_refreshTimer.IsEnabled;

    /// <summary>
    /// Locks the specified coordinate, preventing mouse movements from
    /// tracking new pixels.
    /// </summary>
    /// 
    /// <param name="pos">
    /// The coordinate to lock.
    /// </param>
    public void LockPosition(Point pos) => _lockedPixelPos = pos;

    /// <summary>
    /// Unlocks a previously locked coordinate via <see cref="LockPosition"/>
    /// or <see cref="LockPixelPosition"/>.
    /// </summary>
    public void UnlockPosition() => _lockedPixelPos = new Point(-1, -1);

    protected override Size MeasureOverride(Size availableSize)
    {
        var dpi = VisualTreeHelper.GetDpi(this);

        var pxSize = (int)Math.Round(PixelSize * dpi.DpiScaleX);
        var devPxSize = pxSize * PixelColumns;

        if (ShowGrid)
            devPxSize += (PixelColumns - 1) * _gridGap;

        // Convert device pixels back to DIPs
        var sizeDip = devPxSize / dpi.DpiScaleX;

        return new Size(sizeDip, sizeDip);
    }

    private static HBITMAP CaptureRectToHBitmap(int left, int top, int width, int height)
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

        var result = PInvoke.BitBlt(
            hdcMem, 
            0, 0, width, height, 
            hdcScreen, 
            left, top, 
            ROP_CODE.SRCCOPY | ROP_CODE.CAPTUREBLT);

        // restore and cleanup
        PInvoke.SelectObject(hdcMem, hOld);
        PInvoke.DeleteDC(hdcMem);
        PInvoke.ReleaseDC(HWND.Null, hdcScreen);

        return result ? hBitmap : HBITMAP.Null;
    }

    private Color SampleColor(PixelSamplingMode mode, byte[] buffer, int stride)
    {
        var idx = (_pixelColumnsHalf * stride) + _pixelColumnsHalf * 4;

        var b = buffer[idx + 0];
        var g = buffer[idx + 1];
        var r = buffer[idx + 2];

        return Color.FromRgb(r, g, b);
    }

    private void CaptureAt(Point screenPoint)
    {
        // ensure integer screen coords
        var cx = (int)Math.Round(screenPoint.X);
        var cy = (int)Math.Round(screenPoint.Y);

        var left = cx - _pixelColumnsHalf;
        var top = cy - _pixelColumnsHalf;

        if (left < 0) left = 0;
        if (top < 0) top = 0;

        var hBitmap = CaptureRectToHBitmap(left, top, _captureWidth, _captureHeight);
        if (hBitmap == HBITMAP.Null)
        {
            _captureBuffer = null;
            _centerPixelColor = Colors.Transparent;
            return;
        }

        try
        {
            var bsrc = Imaging.CreateBitmapSourceFromHBitmap(
                hBitmap,
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());

            _captureStride = bsrc.PixelWidth * 4;

            var required = _captureStride * bsrc.PixelHeight;
            if (_captureBuffer == null || _captureBuffer.Length < required)
                _captureBuffer = new byte[required];

            bsrc.CopyPixels(_captureBuffer, _captureStride, 0);

            _centerPixelColor = SampleColor(SamplingMode, _captureBuffer, _captureStride);
        }
        finally
        {
            PInvoke.DeleteObject(hBitmap);
            PixelChanged?.Invoke(this, new PixelChangedEventArgs(_centerPixelColor, _lastMousePos));
        }
    }

    //private readonly Rect[] _centerRects = new Rect[2];

    //private void SetCenterRectangles()
    //{
    //    var centerX = (_pixelSizeInternal + Convert.ToInt32(_showGrid)) * _pixelColumnsHalf;
    //    var centerY = (_pixelSizeInternal + Convert.ToInt32(_showGrid)) * _pixelColumnsHalf;
    //
    //    _centerRects[0] = new Rect(
    //        centerX, centerY,
    //        _pixelSizeInternal , _pixelSizeInternal );
    //
    //    if (_samplingMode != PixelSamplingMode.Single)
    //    {
    //        var samplerSize = (int)_samplingMode;
    //
    //        _centerRects[0].Inflate(
    //            _centerRects[0].Width * samplerSize,
    //            _centerRects[0].Height * samplerSize);
    //    }
    //
    //    // The second rectangle should be drawn in a different pen color
    //    // The idea here is creating contrast with the background the two
    //    // rectangles are on so that at least one of the rectangles is
    //    // always visible on screen
    //    _centerRects[1] = _centerRects[0];
    //    _centerRects[1].Inflate(-1, -1);
    //}

    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);
        var dpi = VisualTreeHelper.GetDpi(this);

        if (DesignerProperties.GetIsInDesignMode(this))
        {
            var placeholderText = new FormattedText(
                $"{nameof(PixelMagnifier)} (design-time)",
                System.Globalization.CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight,
                new Typeface("Segoe UI"), 12, Brushes.Gray,
                dpi.PixelsPerDip);

            dc.DrawRectangle(Brushes.Black, null, new Rect(RenderSize));
            dc.DrawText(placeholderText, new Point(4, 4));
            return;
        }

        // If we don't have a capture yet (e.g., StartCapture() isn't called, then
        // capture once at cursor
        if (_captureBuffer == null)
        {
            _lastMousePos = MousePosition;
            CaptureAt(_lastMousePos);
        }

        if (_captureBuffer == null)
            return;

        var pxDev = (int)Math.Round(PixelSize * dpi.DpiScaleX);
        var gridGapDev = ShowGrid ? _gridGap : 0;

        // Expanded (destination) device pixel size including gaps
        var totalDev = pxDev * _captureWidth + gridGapDev * (_captureWidth - 1);
        if (totalDev <= 0)
            return;

        if (_expandedDevSize != totalDev)
        {
            _expandedDevSize = totalDev;
            _expandedBuffer = new byte[_expandedDevSize * _expandedDevSize * 4];

            _expandedBitmap = new WriteableBitmap(
                _expandedDevSize, _expandedDevSize,
                dpi.PixelsPerInchX, dpi.PixelsPerInchY,
                PixelFormats.Bgra32, null);
        }

        var srcStride = _captureStride;
        var destStride = _expandedDevSize * 4;

        Array.Clear(_expandedBuffer!, 0, _expandedBuffer!.Length);

        unsafe
        {
            fixed (byte* srcBase = _captureBuffer)
            fixed (byte* dstBase = _expandedBuffer)
            {
                // Reusable single horizontal line block of one pixel's width
                var lineBytes = pxDev * 4;
                byte* lineBlock = stackalloc byte[lineBytes];

                for (var srcY = 0; srcY < _captureHeight; srcY++)
                {
                    byte* srcRow = srcBase + srcY * srcStride;
                    var destBlockY = srcY * (pxDev + gridGapDev);

                    for (var srcX = 0; srcX < _captureWidth; srcX++)
                    {
                        byte* pSrc = srcRow + srcX * 4;
                        var b = pSrc[0];
                        var g = pSrc[1];
                        var r = pSrc[2];

                        for (var bx = 0; bx < pxDev; bx++)
                        {
                            lineBlock[bx * 4 + 0] = b;
                            lineBlock[bx * 4 + 1] = g;
                            lineBlock[bx * 4 + 2] = r;
                            lineBlock[bx * 4 + 3] = 255;
                        }

                        var destBlockX = srcX * (pxDev + gridGapDev);

                        for (var by = 0; by < pxDev; by++)
                        {
                            byte* pDstRow = dstBase + (destBlockY + by) * destStride + destBlockX * 4;
                            Buffer.MemoryCopy(lineBlock, pDstRow, lineBytes, lineBytes);
                        }
                    }
                }
            }
        }

        _expandedBitmap!.WritePixels(
            new Int32Rect(0, 0, _expandedDevSize, _expandedDevSize),
            _expandedBuffer, destStride, 0);

        // Map device pixels to DIPs once with a scale transform to avoid repeated
        // divisions when DPI scaling is > 96
        dc.PushTransform(new ScaleTransform(
            1.0 / dpi.DpiScaleX, 
            1.0 / dpi.DpiScaleY));

        dc.DrawImage(_expandedBitmap, new Rect(0, 0, _expandedDevSize, _expandedDevSize));

        dc.Pop();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (_refreshTimer != null)
            {
                _refreshTimer.Stop();
                _refreshTimer.Tick -= OnRefreshTimerTick;
            }
        }
    }
}