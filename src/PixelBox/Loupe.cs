// Copyright 2025 Jad Altahan <xv.git@aol.com>
// SPDX-License-Identifier: MIT

using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows.Threading;
using System.Windows;

using Windows.Win32;

using PixelBox.DrawingVisuals;

namespace PixelBox;

/// <summary>
/// Represents a pixel magnification control.
/// </summary>
public class Loupe : FrameworkElement, IDisposable
{
    #region Fields

    private DpiScale _dpi;

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

    private bool _recaptureNeeded;

    public event EventHandler<PixelChangedEventArgs>? PixelChanged;

    private readonly SamplingAreaIndicator _samplingAreaIndicator;
    private readonly PixelGridlines _pixelGridlines;

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
    /// Dependency property for the <see cref="GridSize"/> property.
    /// </summary>
    public static readonly DependencyProperty GridSizeProperty =
        DependencyProperty.Register(
            nameof(GridSize),
            typeof(int),
            typeof(Loupe),
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
            typeof(Loupe),
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
            typeof(Loupe),
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
            typeof(Loupe),
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
            typeof(Loupe),
            new FrameworkPropertyMetadata(true,
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
        if (sender is not Loupe mag)
            return;

        mag._recaptureNeeded = true;

        mag.RecalculateGridMetrics(GridMetricUpdateFlags.Dimension);
        mag.RenderSamplingAreaIndicator();
        mag.RenderPixelGridlines();
    }

    private static void OnPixelSizeChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is not Loupe mag)
            return;

        mag._recaptureNeeded = true;

        mag.RecalculateGridMetrics(GridMetricUpdateFlags.CellSize);
        mag.RenderSamplingAreaIndicator();
        mag.RenderPixelGridlines();
    }

    private static void OnSamplingModeChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is not Loupe mag)
            return;

        mag.RenderSamplingAreaIndicator();
        mag.SampleColorAndNotify();
    }

    private static void OnShowGridChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is not Loupe mag)
            return;

        mag._pixelGridlines.Opacity = (bool)e.NewValue ? 1.0 : 0.0;
        mag.RenderSamplingAreaIndicator();
    }

    private static void OnRefreshIntervalChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is not Loupe mag)
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

        if (!CaptureAt(_mousePos))
            return;

        SampleColorAndNotify();
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
        var offset = ShowGrid ? 0 : 1;
        var pxCenter = _pixelSize * _gridSizeHalf;

        var rect = new Rect(
            pxCenter + offset,
            pxCenter + offset,
            _pixelSize - offset,
            _pixelSize - offset);

        if (SamplingMode != PixelSamplingMode.Single)
        {
            var radius = (int)SamplingMode / 2;
            var samplingSize = _pixelSize * radius;

            rect.Inflate(samplingSize, samplingSize);
        }

        // Exaggerate the indicator size slightly for better visibility
        rect.Inflate(2, 2);

        _samplingAreaIndicator.SetArea(rect);
        _samplingAreaIndicator.Render();
    }

    /// <summary>
    /// Renders pixel gridlines.
    /// </summary>
    private void RenderPixelGridlines()
    {
        _pixelGridlines.SetGrid(_gridSize, _pixelSize);
        _pixelGridlines.Render();
    }

    /// <summary>
    /// Captures a new bitmap at the specified screen position.
    /// </summary>
    /// 
    /// <param name="screenPt">
    /// The screen position to capture at.
    /// </param>
    ///
    /// <returns>
    /// <see langword="true"/> if the capture was successful;
    /// <see langword="false"/> otherwise.
    /// </returns>
    private bool CaptureAt(Point screenPt)
    {
        var cx = (int)screenPt.X;
        var cy = (int)screenPt.Y;

        // Possible negative coordinates here when the cursor is at the boundary
        // of the screen are valid! No need to clamp anything
        var left = cx - _gridSizeHalf;
        var top = cy - _gridSizeHalf;

        if (_gridSize != _dib.Width || _gridSize != _dib.Height)
            _dib.Resize(_gridSize, _gridSize);

        return _dib.Capture(left, top);
    }

    /// <summary>
    /// Samples the color of the center pixel, or the average color of
    /// neighboring pixels depending on the specified sampling mode.
    /// </summary>
    /// 
    /// <param name="mode">
    /// The pixel sampling mode to use, which determines the size of the
    /// sampling kernel.
    /// </param>
    /// 
    /// <returns>
    /// The sampled color. If <paramref name="mode"/> is set to
    /// <see cref="PixelSamplingMode.Single"/>, then the exact color of the
    /// center pixel is returned; otherwise, the average color of neighboring
    /// pixels is calculated and returned.
    /// </returns>
    private unsafe Color SampleColor(PixelSamplingMode mode)
    {
        var dibBits = (byte*)_dib.Bits;
        var dibStride = _dib.Stride;

        if (mode == PixelSamplingMode.Single)
        {
            var pixel = dibBits + (_gridSizeHalf * dibStride) + (_gridSizeHalf * 4);

            return Color.FromRgb(
                pixel[2],
                pixel[1],
                pixel[0]);
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
            var pixel = dibBits + ((first + y) * dibStride) + (first * 4);

            for (int x = 0; x < kSize; x++)
            {
                bSum += pixel[0];
                gSum += pixel[1];
                rSum += pixel[2];

                // Advance to the next pixel since 4 bytes per pixel in BGRA
                pixel += 4;
            }
        }

        return Color.FromRgb(
            (byte)(rSum / kTotal),
            (byte)(gSum / kTotal),
            (byte)(bSum / kTotal));
    }

    /// <summary>
    /// Samples the color of the pixel in the captured bitmap and raises the
    /// <see cref="PixelChanged"/> event.
    /// </summary>
    private unsafe void SampleColorAndNotify()
    {
        if (_dib.Bits is null)
            return;

        _sampledColor = SampleColor(SamplingMode);
        PixelChanged?.Invoke(this, new PixelChangedEventArgs(_sampledColor, _mousePos));
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
            $"{nameof(Loupe)}",
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
    /// Ensures that a capture buffer is available. If no previous capture
    /// exists, a new one is performed at the current mouse position.
    /// </summary>
    /// 
    /// <returns>
    /// <see langword="true"/> if a capture buffer is available;
    /// <see langword="false"/> otherwise.
    /// </returns>
    private unsafe bool EnsureValidCapture()
    {
        if (_dib.Bits is not null && !_recaptureNeeded)
            return true;

        _mousePos = MousePosition;

        if (!CaptureAt(_mousePos))
            return false;

        // Even if this is a recapture at the same position, the pixel data may
        // have changed (e.g, previous capture was from a video playing), so
        // color resampling and notification should be done regardless
        SampleColorAndNotify();

        _recaptureNeeded = false;

        return true;
    }

    /// <summary>
    /// Ensures that the bitmap is created and sized correctly for the current
    /// pixel and grid configuration.
    /// </summary>
    /// 
    /// <param name="drawGrid">
    /// Indicates whether grid gaps should be accounted for in the bitmap size.
    /// </param>
    private void EnsureBitmapReady()
    {
        var totalDev = (_pixelSize * _gridSize);

        if (_bitmapDevSize == totalDev)
            return;

        _bitmapDevSize = totalDev;

        _bitmap = new WriteableBitmap(
            _gridSize,
            _gridSize,
            96.0 * _dpi.DpiScaleX,
            96.0 * _dpi.DpiScaleY,
            PixelFormats.Bgra32,
            null);
    }

    /// <summary>
    /// Writes pixel data from the last captured region to the bitmap's back
    /// buffer.
    /// </summary>
    /// 
    /// <param name="drawGrid">
    /// Indicates whether one-pixel gaps should be created between pixel cells.
    /// </param>
    private unsafe void CopyCaptureToBitmap()
    {
        if (_bitmap is null || _dib.Bits is null)
            return;

        // if (_dib.Width != _gridSize || _dib.Height != _gridSize)
        //     return;

        _bitmap.Lock();

        NativeMemory.Copy(
            _dib.Bits,
            (void*)_bitmap.BackBuffer,
            (nuint)(_dib.Stride * _dib.Width));

        _bitmap.AddDirtyRect(new Int32Rect(0, 0, _dib.Width, _dib.Height));

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
        var sizeDip = _bitmapDevSize / _dpi.DpiScaleX;
        dc.DrawImage(_bitmap!, new Rect(0, 0, sizeDip, sizeDip));
    }

    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="Loupe"/> class.
    /// </summary>
    public Loupe()
    {
        RenderOptions.SetBitmapScalingMode(this, BitmapScalingMode.NearestNeighbor);

        SnapsToDevicePixels = true;
        Focusable = false;

        _dpi = VisualTreeHelper.GetDpi(this);

        _dib = new PersistentDibSection();

        _samplingAreaIndicator = new SamplingAreaIndicator(_dpi);
        _pixelGridlines = new PixelGridlines(_dpi);

        _visuals = new VisualCollection(this) { _pixelGridlines, _samplingAreaIndicator };

        _refreshTimer = new DispatcherTimer(DispatcherPriority.Render)
        {
            Interval = TimeSpan.FromMilliseconds(RefreshInterval),
            IsEnabled = false
        };

        RecalculateGridMetrics(GridMetricUpdateFlags.All);
        RenderSamplingAreaIndicator();
        RenderPixelGridlines();

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

        return new Size(
            sizeDev / _dpi.DpiScaleX,
            sizeDev / _dpi.DpiScaleY);
    }

    protected override void OnDpiChanged(DpiScale oldDpi, DpiScale newDpi)
    {
        base.OnDpiChanged(oldDpi, newDpi);

        _dpi = newDpi;

        _samplingAreaIndicator.SetDpi(_dpi);
        _pixelGridlines.SetDpi(_dpi);

        RecalculateGridMetrics(GridMetricUpdateFlags.All);

        RenderSamplingAreaIndicator();
        RenderPixelGridlines();
    }

    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);

        if (RenderDesignTimePlaceholder(dc, _dpi))
            return;

        // If there's no capture yet (e.g., StartCapture() isn't called, then
        // capture once at cursor
        if (!EnsureValidCapture())
            return;

        EnsureBitmapReady();
        CopyCaptureToBitmap();
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