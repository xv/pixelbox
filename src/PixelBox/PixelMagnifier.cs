// Copyright 2025 Jad Altahan <xv.git@aol.com>
// SPDX-License-Identifier: MIT

using System.ComponentModel;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows.Threading;
using System.Windows;

using Windows.Win32;
using PixelBox.Drawing;

namespace PixelBox;

/// <summary>
/// Represents a pixel magnification control.
/// </summary>
public class PixelMagnifier : FrameworkElement, IDisposable
{
    #region Fields

    private DpiScale _dpi;
    private ScaleTransform? _scaleTrans;

    private readonly DispatcherTimer _refreshTimer;

    private int _pixelSize;
    private int _gridSize;
    private int _gridSizeHalf;

    private Color _sampledColor;
    private Point _mousePos;

    private Point? _lockedPos;
    private bool _lockX, _lockY;

    private readonly PersistentDibSection _dib;
    private WriteableBitmap? _bitmap;
    private int _bitmapDevSize = 0;

    public event EventHandler<PixelChangedEventArgs>? PixelChanged;

    private readonly SamplingAreaIndicator _samplingAreaIndicator;

    private readonly VisualCollection _visuals;

    private bool _disposed;

    #endregion
    #region Enums

    /// <summary>
    /// Specifies which grid metric values should be recalculated.
    /// </summary>
    [Flags]
    private enum GridMetricUpdateFlags
    {
        /// <summary>
        /// No grid metrics are updated.
        /// </summary>
        None = 0,
        /// <summary>
        /// Indicates that the grid's dimension should be recalculated. This
        /// represents the number of cells along one side of the square grid.
        /// </summary>
        Dimension = 1 << 0,
        /// <summary>
        /// Indicates that the grid's cell size metric should be recalculated.
        /// </summary>
        CellSize = 1 << 1,
        /// <summary>
        /// Indicates that all grid metrics should be recalculated.
        /// </summary>
        All = Dimension | CellSize
    }

    #endregion
    #region Dependency Properties

    /// <summary>
    /// Dependency property for the <see cref="Background"/> property.
    /// </summary>
    public static readonly DependencyProperty BackgroundProperty =
        DependencyProperty.Register(
            nameof(Background),
            typeof(Brush),
            typeof(PixelMagnifier),
            new FrameworkPropertyMetadata(Brushes.Black,
                FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>
    /// Gets or sets the brush used to fill the control's background.
    /// </summary>
    /// 
    /// <remarks>
    /// When <see cref="ShowGrid"/> is <see langword="true"/>, the control
    /// leaves gaps between pixels rather than drawing actual grid lines. The
    /// background brush fills the area behind these gaps, creating the
    /// appearance of grid lines in the brush's color.
    /// </remarks>
    [Category("Appearance")]
    [Description("Sets the background of the control.")]
    public Brush Background
    {
        get => (Brush)GetValue(BackgroundProperty);
        set => SetValue(BackgroundProperty, value);
    }

    /// <summary>
    /// Dependency property for the <see cref="GridSize"/> property.
    /// </summary>
    public static readonly DependencyProperty GridSizeProperty =
        DependencyProperty.Register(
            nameof(GridSize),
            typeof(int),
            typeof(PixelMagnifier),
            new FrameworkPropertyMetadata(11,
                FrameworkPropertyMetadataOptions.AffectsMeasure |
                FrameworkPropertyMetadataOptions.AffectsRender,
                OnGridSizeChanged, CoerceGridSize),
            v => (v is int i) && (i > 0) && (i < 100));

    private static object CoerceGridSize(DependencyObject d, object value)
    {
        var cols = (int)value;

        if (cols % 2 != 1)
            cols++;

        return cols;
    }

    /// <summary>
    /// Gets or sets the size of the pixel grid. The value must be greater than
    /// zero and less than 100.
    /// </summary>
    /// 
    /// <remarks>
    /// Only odd values are accepted, so that the position of center pixel is
    /// symmetric horizontally and vertically.
    /// </remarks>
    [Category("Appearance")]
    [Description("Sets the size of the pixel grid. Uses odd values only. Valid range is [1,99].")]
    public int GridSize
    {
        get => (int)GetValue(GridSizeProperty);
        set => SetValue(GridSizeProperty, value);
    }

    /// <summary>
    /// Dependency property for the <see cref="PixelSize"/> property.
    /// </summary>
    public static readonly DependencyProperty PixelSizeProperty =
        DependencyProperty.Register(
            nameof(PixelSize),
            typeof(int),
            typeof(PixelMagnifier),
            new FrameworkPropertyMetadata(9,
                FrameworkPropertyMetadataOptions.AffectsMeasure |
                FrameworkPropertyMetadataOptions.AffectsRender,
                OnPixelSizeChanged),
            v => (v is int i) && (i > 0) && (i <= 100));

    /// <summary>
    /// Gets or sets the size of the pixel cells. The value must be greater than
    /// zero and less than or equal to 100.
    /// </summary>
    [Category("Appearance")]
    [Description("Sets the size (in px) of individual pixel cells in the grid. Valid range is [1,100].")]
    public int PixelSize
    {
        get => (int)GetValue(PixelSizeProperty);
        set => SetValue(PixelSizeProperty, value);
    }

    /// <summary>
    /// Dependency property for the <see cref="SamplingMode"/> property.
    /// </summary>
    public static readonly DependencyProperty SamplingModeProperty =
        DependencyProperty.Register(
            nameof(SamplingMode),
            typeof(PixelSamplingMode),
            typeof(PixelMagnifier),
            new FrameworkPropertyMetadata(PixelSamplingMode.Single,
                FrameworkPropertyMetadataOptions.AffectsRender,
                OnSamplingModeChanged));

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
    /// Dependency property for the <see cref="RefreshInterval"/> property.
    /// </summary>
    public static readonly DependencyProperty RefreshIntervalProperty =
        DependencyProperty.Register(
            nameof(RefreshInterval),
            typeof(int),
            typeof(PixelMagnifier),
            new FrameworkPropertyMetadata(30, OnRefreshIntervalChanged),
            v => (v is int i) && (i > 0) && (i <= 1000));

    /// <summary>
    /// Gets or sets the redraw rate of the control in milliseconds. The value
    /// must be greater than zero and less than or equal to 1000.
    /// </summary>
    [Category("Behavior")]
    [Description("Sets refresh rate (in ms) of the control. Valid range is [1,1000].")]
    public int RefreshInterval
    {
        get => (int)GetValue(RefreshIntervalProperty);
        set => SetValue(RefreshIntervalProperty, value);
    }

    /// <summary>
    /// Dependency property for the <see cref="ShowGrid"/> property.
    /// </summary>
    public static readonly DependencyProperty ShowGridProperty =
        DependencyProperty.Register(
            nameof(ShowGrid),
            typeof(bool),
            typeof(PixelMagnifier),
            new FrameworkPropertyMetadata(true,
                FrameworkPropertyMetadataOptions.AffectsMeasure |
                FrameworkPropertyMetadataOptions.AffectsRender,
                OnShowGridChanged));

    /// <summary>
    /// Gets or sets whether the lines of rows and columns of pixels should be
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
    #region Properties

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
    /// Gets whether the control captures pixels on mouse movement.
    /// </summary>
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [EditorBrowsable(EditorBrowsableState.Always)]
    public bool IsCapturing => _refreshTimer.IsEnabled;

    /// <summary>
    /// Gets the color corresponding to the pixel located at
    /// the center of the grid.
    ///
    /// If <see cref="SamplingMode"/> is not <see cref="PixelSamplingMode.Single"/>,
    /// then the average color of pixels in the sampling region is retrieved.
    /// </summary>
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [EditorBrowsable(EditorBrowsableState.Always)]
    public Color PixelColor => _sampledColor;

    /// <summary>
    /// Gets the screen coordinates corresponding to the pixel located at
    /// the center of the grid.
    /// </summary>
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [EditorBrowsable(EditorBrowsableState.Always)]
    public Point PixelPosition => _mousePos;

    /// <summary>
    /// Gets or sets whether both X and Y screen coordinates are locked.
    /// </summary>
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [EditorBrowsable(EditorBrowsableState.Always)]
    public bool PositionLocked
    {
        get => _lockedPos.HasValue;
        set
        {
            if (value)
            {
                _lockedPos = _mousePos;
                _lockX = _lockY = true;
            }
            else
            {
                _lockedPos = null;
                _lockX = _lockY = false;
            }
        }
    }

    /// <summary>
    /// Gets or sets whether the X screen coordinate is locked.
    /// </summary>
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [EditorBrowsable(EditorBrowsableState.Always)]
    public bool PositionXLocked
    {
        get => _lockX;
        set
        {
            if (value && !_lockX)
            {
                var pos = _lockedPos ?? _mousePos;
                pos.X = _mousePos.X >= 0 ? _mousePos.X : MousePosition.X;

                _lockedPos = pos;
            }

            _lockX = value;

            if (!_lockX && !_lockY)
                _lockedPos = null;
        }
    }

    /// <summary>
    /// Gets or sets whether the Y screen coordinate is locked.
    /// </summary>
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [EditorBrowsable(EditorBrowsableState.Always)]
    public bool PositionYLocked
    {
        get => _lockY;
        set
        {
            if (value && !_lockY)
            {
                var pos = _lockedPos ?? _mousePos;
                pos.Y = _mousePos.Y >= 0 ? _mousePos.Y : MousePosition.Y;

                _lockedPos = pos;
            }

            _lockY = value;

            if (!_lockX && !_lockY)
                _lockedPos = null;
        }
    }

    #endregion
    #region Event Handling

    private static void OnGridSizeChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is not PixelMagnifier mag)
            return;

        mag.RecalculateGridMetrics(GridMetricUpdateFlags.Dimension);
        mag.RenderSamplingAreaIndicator();
    }

    private static void OnPixelSizeChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is not PixelMagnifier mag)
            return;

        mag.RecalculateGridMetrics(GridMetricUpdateFlags.CellSize);
        mag.RenderSamplingAreaIndicator();
    }

    private static void OnSamplingModeChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is not PixelMagnifier mag)
            return;

        mag.RenderSamplingAreaIndicator();
        mag.SampleColorFromLastCapture();
    }

    private static void OnShowGridChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is not PixelMagnifier mag)
            return;

        mag.RenderSamplingAreaIndicator();
    }

    private static void OnRefreshIntervalChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is not PixelMagnifier mag)
            return;

        mag._refreshTimer.Interval = TimeSpan.FromMilliseconds((int)e.NewValue);
    }

    private void OnRefreshTimerTick(object? sender, EventArgs e)
    {
        var cursorPos = MousePosition;
        Point currentPos;

        if (!_lockedPos.HasValue)
            currentPos = cursorPos;
        else
        {
            var locked = _lockedPos.Value;

            currentPos = new Point(
                _lockX ? locked.X : cursorPos.X,
                _lockY ? locked.Y : cursorPos.Y);
        }

        if (currentPos == _mousePos)
            return;

        _mousePos = currentPos;

        CaptureAt(_mousePos);
        InvalidateVisual();
    }

    #endregion
    #region Methods

    /// <summary>
    /// Begins capturing pixels.
    /// </summary>
    public void StartCapture() => _refreshTimer.Start();

    /// <summary>
    /// Stops capturing pixels.
    /// </summary>
    public void StopCapture() => _refreshTimer.Stop();

    /// <summary>
    /// Toggles pixel capture.
    /// </summary>
    public void ToggleCapture() => _refreshTimer.IsEnabled = !_refreshTimer.IsEnabled;

    /// <summary>
    /// Locks the specified screen coordinates on both X and Y axes.
    /// </summary>
    /// 
    /// <param name="pos">
    /// The screen coordinates.
    /// </param>
    public void LockPosition(Point pos)
    {
        _lockedPos = pos;
        _lockX = _lockY = true;
    }

    /// <summary>
    /// Locks the specified screen coordinates, optionally locking the X and Y
    /// axes individually.
    /// </summary>
    ///
    /// <param name="pos">
    /// The screen coordinates.
    /// </param>
    ///
    /// <param name="lockX">
    /// Specifies whether to lock the X coordinate.
    /// </param>
    /// 
    /// <param name="lockY">
    /// Specifies whether to lock the Y coordinate.
    /// </param>
    public void LockPosition(Point pos, bool lockX, bool lockY)
    {
        _lockedPos = pos;
        _lockX = lockX;
        _lockY = lockY;

        if (!lockX && !lockY)
            _lockedPos = null;
    }

    /// <summary>
    /// Locks only the X coordinate of the current screen position.
    /// </summary>
    public void LockPositionX() => PositionXLocked = true;

    /// <summary>
    /// Locks only the Y coordinate of the current screen position.
    /// </summary>
    public void LockPositionY() => PositionYLocked = true;

    /// <summary>
    /// Unlocks the X coordinate, allowing horizontal mouse tracking to update
    /// the position.
    /// </summary>
    public void UnlockPositionX() => PositionXLocked = false;

    /// <summary>
    /// Unlocks the Y coordinate, allowing vertical mouse tracking to update the
    /// position.
    /// </summary>
    public void UnlockPositionY() => PositionYLocked = false;

    /// <summary>
    /// Unlocks both X and Y coordinates, allowing the mouse to update the
    /// position freely.
    /// </summary>
    public void UnlockPosition() => PositionLocked = false;

    /// <summary>
    /// Configures the internal <see cref="ScaleTransform"/> so that the visual
    /// is scaled properly regardless of the monitor's current DPI scaling.
    /// </summary>
    private void SetScaleTransform()
    {
        _scaleTrans ??= new ScaleTransform();
        _scaleTrans.ScaleX = 1.0 / _dpi.DpiScaleX;
        _scaleTrans.ScaleY = 1.0 / _dpi.DpiScaleY;
    }

    /// <summary>
    /// Recalculates grid metric values based on the specified update flags.
    /// </summary>
    /// 
    /// <param name="flags">
    /// Specifies which grid metrics to recalculate.
    /// </param>
    private void RecalculateGridMetrics(GridMetricUpdateFlags flags)
    {
        if ((flags & GridMetricUpdateFlags.CellSize) != 0)
            _pixelSize = (int)Math.Round(PixelSize * _dpi.DpiScaleX);

        if ((flags & GridMetricUpdateFlags.Dimension) != 0)
        {
            _gridSize = GridSize;
            _gridSizeHalf = _gridSize / 2;
        }
    }

    /// <summary>
    /// Renders the visual indicator surrounding the current sampling area.
    /// </summary>
    private void RenderSamplingAreaIndicator()
    {
        var pxCenter = (_pixelSize + (ShowGrid ? 1 : 0)) * _gridSizeHalf;

        var rect = new Rect(
            pxCenter, pxCenter,
            _pixelSize + 1, _pixelSize + 1);

        var samplerSize = (int)SamplingMode / 2;

        if (SamplingMode != PixelSamplingMode.Single)
            rect.Inflate(rect.Width * samplerSize, rect.Height * samplerSize);

        if (!ShowGrid)
            rect.Inflate(-(samplerSize + 1), -(samplerSize + 1));

        _samplingAreaIndicator.SetArea(rect);
        _samplingAreaIndicator.Render();
    }

    /// <summary>
    /// Captures a new bitmap at the specified screen position.
    /// </summary>
    /// 
    /// <param name="screenPt">
    /// The screen position to capture at.
    /// </param>
    private unsafe void CaptureAt(Point screenPt)
    {
        var cx = (int)screenPt.X;
        var cy = (int)screenPt.Y;

        // Possible negative coordinates here when the cursor is at the boundary
        // of the screen are valid! No need to clamp anything
        var left = cx - _gridSizeHalf;
        var top = cy - _gridSizeHalf;

        if (_gridSize != _dib.Width || _gridSize != _dib.Height)
            _dib.Resize(_gridSize, _gridSize);

        if (!_dib.Capture(left, top))
            return;

        _sampledColor = SampleColor(SamplingMode, (byte*)_dib.Bits, _dib.Stride);
        PixelChanged?.Invoke(this, new PixelChangedEventArgs(_sampledColor, _mousePos));
    }

    /// <summary>
    /// Samples the color of thr pixel.
    /// </summary>
    /// 
    /// <param name="mode">
    /// The pixel sampling mode to use, which determines the size of the
    /// sampling kernel.
    /// </param>
    /// 
    /// <param name="pBits">
    /// Pointer to the pixel data.
    /// </param>
    /// 
    /// <param name="stride">
    /// The stride of a single row of pixels in the buffer.
    /// </param>
    /// 
    /// <returns>
    /// The sampled color. If <paramref name="mode"/> is set to
    /// <see cref="PixelSamplingMode.Single"/>, then the exact color of the
    /// pixel is returned; otherwise, the average color of neighboring pixels
    /// is calculated and returned.
    /// </returns>
    private unsafe Color SampleColor(PixelSamplingMode mode, byte* pBits, int stride)
    {
        if (mode == PixelSamplingMode.Single)
        {
            var idx = (_gridSizeHalf * stride) + (_gridSizeHalf * 4);
            return Color.FromRgb(
                *(pBits + idx + 2 /* R */),
                *(pBits + idx + 1 /* G */),
                *(pBits + idx + 0 /* B */));
        }

        var kSize = (int)mode;
        var kTotal = kSize * kSize;

        // Index of the first cell (top-left corner) of the kernel
        var first = _gridSizeHalf - (kSize / 2);

        int rSum = 0,
            gSum = 0,
            bSum = 0;

        for (int y = 0; y < kSize; y++)
        {
            for (int x = 0; x < kSize; x++)
            {
                var idx = (first + y) * stride + (first + x) * 4;
                bSum += *(pBits + idx + 0);
                gSum += *(pBits + idx + 1);
                rSum += *(pBits + idx + 2);
            }
        }

        return Color.FromRgb(
            (byte)(rSum / kTotal),
            (byte)(gSum / kTotal),
            (byte)(bSum / kTotal));
    }

    /// <summary>
    /// Samples color using the last capture buffer. However, if the said buffer
    /// is <see langword="null"/>, a new capture will be created.
    /// </summary>
    private unsafe void SampleColorFromLastCapture()
    {
        if (_dib.Bits is null)
        {
            CaptureAt(_mousePos);
            return;
        }

        _sampledColor = SampleColor(SamplingMode,
            (byte*)_dib.Bits, _dib.Stride);

        PixelChanged?.Invoke(this, new PixelChangedEventArgs(
            _sampledColor, _mousePos));
    }

    /// <summary>
    /// Renders a placeholder string containing the control name when in
    /// design mode.
    /// </summary>
    /// 
    /// <param name="dc">
    /// The drawing context.
    /// </param>
    /// 
    /// <param name="dpi">
    /// DPI scale information.
    /// </param>
    /// 
    /// <returns>
    /// <see langword="true"/> if in design mode and the placeholder was
    /// rendered; false otherwise
    /// </returns>
    private bool RenderDesignTimePlaceholder(DrawingContext dc, DpiScale dpi)
    {
        if (!DesignerProperties.GetIsInDesignMode(this))
            return false;

        var rect = new Rect(RenderSize);

        var text = new FormattedText(
            $"{nameof(PixelMagnifier)}",
            System.Globalization.CultureInfo.InvariantCulture,
            FlowDirection.LeftToRight,
            new Typeface("Segoe UI"), 12, Brushes.Gray, dpi.PixelsPerDip)
        {
            MaxTextWidth = rect.Width - 10,
            MaxTextHeight = rect.Height - 10
        };

        dc.DrawRectangle(Brushes.Black, null, rect);

        var x = rect.Left + (rect.Width - text.Width) / 2;
        var y = rect.Top + (rect.Height - text.Height) / 2;

        dc.DrawText(text, new Point(x, y));

        return true;
    }

    /// <summary>
    /// Draws a simulated grid to visualize individual pixel cell boundaries.
    /// </summary>
    /// 
    /// <param name="dc">
    /// The drawing context to use.
    /// </param>
    private void DrawGrid(DrawingContext dc)
    {
        dc.DrawRectangle(Background, null,
            new Rect(0, 0, ActualWidth, ActualHeight));
    }

    /// <summary>
    /// Ensures that a capture buffer is available. If no previous capture
    /// exists, a new one is performed at the current mouse position.
    /// </summary>
    /// 
    /// <returns>
    /// <see langword="true"/> if a capture buffer is available;
    /// <see langword="false"/> otherwise.
    /// </returns>
    private unsafe bool EnsureCapture()
    {
        if (_dib.Bits is null)
        {
            _mousePos = MousePosition;
            CaptureAt(_mousePos);
        }

        return _dib.Bits is not null;
    }

    /// <summary>
    /// Ensures that the bitmap is created and sized correctly for the current
    /// pixel and grid configuration.
    /// </summary>
    /// 
    /// <param name="drawGrid">
    /// Indicates whether grid gaps should be accounted for in the bitmap size.
    /// </param>
    private void EnsureBitmapReady(bool drawGrid)
    {
        var gridGapDev = drawGrid ? 1 : 0;
        var totalDev = (_pixelSize * _gridSize) + (gridGapDev * (_gridSize - 1));

        if (_bitmapDevSize == totalDev)
            return;

        _bitmapDevSize = totalDev;

        _bitmap = new WriteableBitmap(
            _bitmapDevSize, _bitmapDevSize,
            _dpi.PixelsPerInchX, _dpi.PixelsPerInchY,
            PixelFormats.Bgra32, null);

        // Recapture since the size has changed
        CaptureAt(_mousePos);
    }

    /// <summary>
    /// Writes pixel data from the last captured region to the bitmap's back
    /// buffer.
    /// </summary>
    /// 
    /// <param name="drawGrid">
    /// Indicates whether one-pixel gaps should be created between pixel cells.
    /// </param>
    private unsafe void CopyCaptureToBitmap(bool drawGrid)
    {
        if (_bitmap is null || _dib.Bits is null)
            return;

        var gridGapDev = drawGrid ? 1 : 0;
        var dstStride = _bitmap.BackBufferStride;

        _bitmap.Lock();

        byte* dstBase = (byte*)_bitmap.BackBuffer;
        byte* srcBase = (byte*)_dib.Bits;

        var lineBytes = _pixelSize * 4;
        byte* lineBlock = stackalloc byte[lineBytes];

        for (var srcY = 0; srcY < _gridSize; srcY++)
        {
            byte* pSrcRow = srcBase + (srcY * _dib.Stride);
            var dstBlockY = srcY * (_pixelSize + gridGapDev);

            for (var srcX = 0; srcX < _gridSize; srcX++)
            {
                byte* pSrc = pSrcRow + srcX * 4;
                var b = pSrc[0];
                var g = pSrc[1];
                var r = pSrc[2];

                for (var bx = 0; bx < _pixelSize; bx++)
                {
                    lineBlock[bx * 4 + 0] = b;
                    lineBlock[bx * 4 + 1] = g;
                    lineBlock[bx * 4 + 2] = r;
                    lineBlock[bx * 4 + 3] = 255;
                }

                var dstBlockX = srcX * (_pixelSize + gridGapDev);

                for (var by = 0; by < _pixelSize; by++)
                {
                    byte* pDstRow = dstBase + ((dstBlockY + by) * dstStride) + (dstBlockX * 4);
                    Buffer.MemoryCopy(lineBlock, pDstRow, lineBytes, lineBytes);
                }
            }
        }

        _bitmap.AddDirtyRect(new Int32Rect(0, 0, _bitmapDevSize, _bitmapDevSize));
        _bitmap.Unlock();
    }

    /// <summary>
    /// Draws the bitmap onto the specified <see cref="DrawingContext"/>,
    /// applying DPI scaling if required.
    /// </summary>
    /// 
    /// <param name="dc">
    /// The drawing context to use.
    /// </param>
    private void DrawBitmap(DrawingContext dc)
    {
        var useScale = _scaleTrans is not null;

        if (useScale)
            dc.PushTransform(_scaleTrans);

        dc.DrawImage(_bitmap!, new Rect(0, 0, _bitmapDevSize, _bitmapDevSize));

        if (useScale)
            dc.Pop();
    }

    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="PixelMagnifier"/> class.
    /// </summary>
    public PixelMagnifier()
    {
        RenderOptions.SetBitmapScalingMode(this, BitmapScalingMode.NearestNeighbor);

        SnapsToDevicePixels = true;
        Focusable = false;

        _dpi = VisualTreeHelper.GetDpi(this);

        _dib = new PersistentDibSection();

        _samplingAreaIndicator = new SamplingAreaIndicator(_dpi);
        _visuals = new VisualCollection(this) { _samplingAreaIndicator };

        _refreshTimer = new DispatcherTimer(DispatcherPriority.Render)
        {
            Interval = TimeSpan.FromMilliseconds(RefreshInterval),
            IsEnabled = false
        };

        RecalculateGridMetrics(GridMetricUpdateFlags.All);
        RenderSamplingAreaIndicator();

        if (_dpi.DpiScaleX != 1.0 || _dpi.DpiScaleY != 1.0)
            SetScaleTransform();

        Loaded += (_, _) => _refreshTimer.Tick += OnRefreshTimerTick;

        Unloaded += (_, _) =>
        {
            _refreshTimer.Stop();
            _refreshTimer.Tick -= OnRefreshTimerTick;
        };
    }

    protected override Visual GetVisualChild(int index) => _visuals[index];

    protected override int VisualChildrenCount => _visuals.Count;

    protected override Size MeasureOverride(Size availableSize)
    {
        var sizeDev = _gridSize * _pixelSize;

        if (ShowGrid)
            sizeDev += _gridSize - 1;

        return new Size(
            sizeDev / _dpi.DpiScaleX,
            sizeDev / _dpi.DpiScaleY);
    }

    protected override void OnDpiChanged(DpiScale oldDpi, DpiScale newDpi)
    {
        base.OnDpiChanged(oldDpi, newDpi);

        _dpi = newDpi;
        _samplingAreaIndicator.SetDpi(_dpi);

        SetScaleTransform();

        RecalculateGridMetrics(GridMetricUpdateFlags.All);
        RenderSamplingAreaIndicator();
    }

    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);

        if (RenderDesignTimePlaceholder(dc, _dpi))
            return;

        // Since the "grid" is really nothing more than gaps between pixels,
        // simply draw a background behind to simulate grid lines
        var drawGrid = ShowGrid;
        if (drawGrid)
            DrawGrid(dc);

        // If there's no capture yet (e.g., StartCapture() isn't called, then
        // capture once at cursor
        if (!EnsureCapture())
            return;

        EnsureBitmapReady(drawGrid);
        CopyCaptureToBitmap(drawGrid);
        DrawBitmap(dc);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
            _dib?.Dispose();

        _disposed = true;
    }
}