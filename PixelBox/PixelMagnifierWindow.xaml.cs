using System.IO;
using System.Media;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;

using Windows.Win32.Graphics.Gdi;
using Windows.Win32.UI.WindowsAndMessaging;
using Windows.Win32;

using PixelBox.Resources;

namespace PixelBox;

/// <summary>
/// Represents a mouse-tracked pixel magnifier window.
/// </summary>
public partial class PixelMagnifierWindow : Window
{
    #region Fields

    private readonly PixelMagnifierWindowConfig _config;

    private const double WindowOffset = 1;

    private readonly DpiScale _dpi;
    private readonly Rect _screen = SystemParameters.WorkArea;

    private readonly SolidColorBrush _colorPreviewBrush = new(Colors.Transparent);

    private static readonly SoundPlayer s_soundPlayer = new();
    private static readonly Dictionary<SoundEffect, MemoryStream> s_preloadedSoundStreams = [];
    private static readonly object s_soundLock = new();

    private readonly Cursor _cursor;

    private readonly PixelSamplingMode[] _samplingModes =
    {
        PixelSamplingMode.Single,
        PixelSamplingMode.ThreeByThree,
        PixelSamplingMode.FiveByFive,
        PixelSamplingMode.SevenBySeven
    };

    #endregion
    #region Enums

    /// <summary>
    /// Represents predefined sound effects.
    /// </summary>
    public enum SoundEffect
    {
        None,
        Pop,
        Click,
        Notify
    }

    #endregion
    #region Dependency Properties

    private static readonly DependencyProperty ColorPreviewBrushProperty =
        DependencyProperty.Register(
            nameof(ColorPreviewBrush),
            typeof(Brush),
            typeof(PixelMagnifierWindow));

    /// <summary>
    /// Gets or sets the brush used to render the color preview element.
    /// </summary>
    internal Brush ColorPreviewBrush
    {
        get => (Brush)GetValue(ColorPreviewBrushProperty);
        set => SetValue(ColorPreviewBrushProperty, value);
    }

    private static readonly DependencyProperty PixelColorStringProperty =
        DependencyProperty.Register(
            nameof(PixelColorString),
            typeof(string),
            typeof(PixelMagnifierWindow),
            new PropertyMetadata("?"));

    /// <summary>
    /// Gets or sets the current pixel color as a string representation.
    /// </summary>
    internal string PixelColorString
    {
        get => (string)GetValue(PixelColorStringProperty);
        set => SetValue(PixelColorStringProperty, value);
    }

    private static readonly DependencyProperty PixelPositionStringProperty =
        DependencyProperty.Register(
            nameof(PixelPositionString),
            typeof(string),
            typeof(PixelMagnifierWindow),
            new PropertyMetadata("?"));

    /// <summary>
    /// Gets or sets the current pixel position as a string representation.
    /// </summary>
    internal string PixelPositionString
    {
        get => (string)GetValue(PixelPositionStringProperty);
        set => SetValue(PixelPositionStringProperty, value);
    }

    private static readonly DependencyProperty MagnifierXProperty =
        DependencyProperty.Register(
            nameof(MagnifierX),
            typeof(double),
            typeof(PixelMagnifierWindow));

    /// <summary>
    /// Gets or sets the X coordinate of the magnifier window.
    /// </summary>
    internal double MagnifierX
    {
        get => (double)GetValue(MagnifierXProperty);
        set => SetValue(MagnifierXProperty, value);
    }

    private static readonly DependencyProperty MagnifierYProperty =
        DependencyProperty.Register(
            nameof(MagnifierY),
            typeof(double),
            typeof(PixelMagnifierWindow));

    /// <summary>
    /// Gets or sets the Y coordinate of the magnifier window.
    /// </summary>
    internal double MagnifierY
    {
        get => (double)GetValue(MagnifierYProperty);
        set => SetValue(MagnifierYProperty, value);
    }

    private static readonly DependencyProperty OverlayImageProperty =
    DependencyProperty.Register(
        nameof(OverlayImage),
        typeof(ImageSource),
        typeof(PixelMagnifierWindow));

    /// <summary>
    /// Gets or sets the overlay image used as the background for the magnifier.
    /// </summary>
    /// 
    /// <remarks>
    /// This image should snapshot of the desktop screen. The idea here is that
    /// the overlay would prevent the mouse cursor from interacting with the
    /// desktop's UI elements underneath.
    /// </remarks>
    internal ImageSource OverlayImage
    {
        get => (ImageSource)GetValue(OverlayImageProperty);
        set => SetValue(OverlayImageProperty, value);
    }

    #endregion
    #region Properties

    /// <summary>
    /// Gets or sets the sound effect that plays upon a confirmation action.
    /// </summary>
    public SoundEffect ConfirmationSoundEffect
    { get; set; } = SoundEffect.Pop;

    /// <summary>
    /// Gets or sets whether to show the information panel below the magnifier.
    /// </summary>
    public bool ShowInfoPanel
    {
        get => InfoPanelHost.Visibility == Visibility.Visible;
        set => InfoPanelHost.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
    }

    /// <summary>
    /// Gets the color of the currently selected pixel.
    /// </summary>
    public Color? SelectedPixelColor
    { get; private set; }

    /// <summary>
    /// Gets the screen coordinates of the currently selected pixel.
    /// </summary>
    public Point? SelectedPixelPosition 
    { get; private set; }

    #endregion
    #region Methods

    /// <summary>
    /// Moves the mouse cursor in the direction(s) of the arrow keys currently
    /// pressed. Supports 8-directional movement.
    /// </summary>
    private static void HandlePixelNavigation()
    {
        if ((Keyboard.Modifiers & (ModifierKeys.Control | ModifierKeys.Alt)) != 0)
            return;

        var step =
            Keyboard.IsKeyDown(Key.LeftShift) ||
            Keyboard.IsKeyDown(Key.RightShift) ? 5 : 1;

        var left = Keyboard.IsKeyDown(Key.Left);
        var right = Keyboard.IsKeyDown(Key.Right);
        var up = Keyboard.IsKeyDown(Key.Up);
        var down = Keyboard.IsKeyDown(Key.Down);

        if (!(left || right || up || down))
            return;

        int dx = 0, dy = 0;

        if (left) dx -= step;
        if (right) dx += step;
        if (up) dy -= step;
        if (down) dy += step;

        PInvoke.GetCursorPos(out System.Drawing.Point p);
        PInvoke.SetCursorPos(p.X + dx, p.Y + dy);
    }

    /// <summary>
    /// Loads a cursor from a pack URI resource stream.
    /// </summary>
    /// 
    /// <param name="packUri">
    /// URI of the cursor resource.
    /// </param>
    /// 
    /// <param name="scaleWithDpi">
    /// Specifies whether the cursor should scale with the system DPI settings.
    /// Set to <see langword="true"/> if the cursor file defines multiple bitmap
    /// sizes.
    /// </param>
    /// 
    /// <returns>
    /// A <see cref="Cursor"/> object representing the cursor.
    /// </returns>
    public static Cursor LoadCursorFromResource(Uri packUri, bool scaleWithDpi)
    {
        using var stream = Application.GetResourceStream(packUri).Stream;
        return new Cursor(stream, scaleWithDpi);
    }

    /// <summary>
    /// Preloads sound streams into memory for quick access during application
    /// runtime.
    /// </summary>
    private static void LoadSoundStreams()
    {
        static MemoryStream Load(Uri uri)
        {
            using var rs = Application.GetResourceStream(uri).Stream;
            var ms = new MemoryStream();
            rs.CopyTo(ms);
            ms.Position = 0;
            return ms;
        }

        s_preloadedSoundStreams[SoundEffect.Pop] = Load(ResourceUris.Sounds.Pop);
        s_preloadedSoundStreams[SoundEffect.Click] = Load(ResourceUris.Sounds.Click);
        s_preloadedSoundStreams[SoundEffect.Notify] = Load(ResourceUris.Sounds.Notify);
    }

    /// <summary>
    /// Plays the specified sound effect.
    /// </summary>
    /// 
    /// <param name="soundEffect">
    /// The sound effect to play.
    /// </param>
    private static void PlaySoundEffect(SoundEffect soundEffect)
    {
        if (soundEffect == SoundEffect.None ||
            !s_preloadedSoundStreams.TryGetValue(soundEffect, out var ms))
            return;

        lock (s_soundLock)
        {
            ms.Position = 0;
            s_soundPlayer.Stream = ms;
            s_soundPlayer.Play();
        }
    }

    /// <summary>
    /// Retrieves the bounds of the virtual screen.
    /// </summary>
    /// 
    /// <returns>
    /// An <see cref="Int32Rect"/> representing the virtual screen's bounds.
    /// </returns>
    private static Int32Rect GetScreenBounds() =>
        new(PInvoke.GetSystemMetrics(SYSTEM_METRICS_INDEX.SM_XVIRTUALSCREEN),
            PInvoke.GetSystemMetrics(SYSTEM_METRICS_INDEX.SM_YVIRTUALSCREEN),
            PInvoke.GetSystemMetrics(SYSTEM_METRICS_INDEX.SM_CXVIRTUALSCREEN),
            PInvoke.GetSystemMetrics(SYSTEM_METRICS_INDEX.SM_CYVIRTUALSCREEN));


    /// <summary>
    /// Retrieves the current mouse cursor screen position.
    /// </summary>
    /// 
    /// <returns>
    /// A <see cref="Point"/> representing the current cursor position,
    /// converted from screen coordinates to the coordinate system of this
    /// window.
    /// </returns>
    /// 
    /// <remarks>
    /// The window (<see cref="PixelMagnifierWindow"/>) must be loaded into the
    /// visual tree before this method is called.
    /// </remarks>
    private Point GetMousePosition()
    {
        PInvoke.GetCursorPos(out System.Drawing.Point p);
        return PointFromScreen(new Point(p.X, p.Y));
    }

    /// <summary>
    /// Captures the current desktop screen.
    /// </summary>
    /// 
    /// <returns>
    /// A <see cref="BitmapSource"/> representing the captured desktop screen,
    /// or <see langword="null"/> if the capture
    /// fails.
    /// </returns>
    private static BitmapSource? CaptureDesktopScreen()
    {
        var hBitmap = BitmapInterop.CaptureRectToHBitmap(GetScreenBounds());
        if (hBitmap == HBITMAP.Null)
            return null;

        try { return hBitmap.ToBitmapSource(true); }
        finally { PInvoke.DeleteObject(hBitmap); }
    }

    /// <summary>
    /// Updates the position of the magnifier on the screen based on the
    /// specified point.
    /// </summary>
    /// 
    /// <param name="pos">
    /// The position, in screen coordinates, where the magnifier should be placed.
    /// </param>
    private void UpdateMagnifierPosition(Point pos)
    {
        // Initial magnifier position
        var left = pos.X + WindowOffset;
        var top = pos.Y + WindowOffset;

        var width = MagnifierHost.ActualWidth;
        var height = MagnifierHost.ActualHeight;

        // If at the right border of the screen, move the magnifier leftward
        if ((left + width) > _screen.Right)
            left = pos.X - width - WindowOffset;

        // If the bottom border of the screen, move the magnifier upward
        if ((top + height) > _screen.Bottom)
            top = pos.Y - height - WindowOffset;

        var sx = _dpi.DpiScaleX;
        var sy = _dpi.DpiScaleY;

        // Applying sx/sy prevents the magnifier grid lines from pixel-shifting
        // on high-DPI screens
        //
        // Flooring (or even ceiling) prevents rounding artifacts noticeable when
        // the mouse move by 1px which sometimes causes the magnifier window to
        // remain still until the mouse moves again.
        MagnifierX = Math.Floor(left * sx) / sx;
        MagnifierY = Math.Floor(top * sy) / sy;
    }

    /// <summary>
    /// Updates the color preview element's brush and string representation of
    /// the current pixel color.
    /// </summary>
    /// 
    /// <param name="newColor">
    /// The new pixel color.
    /// </param>
    private void UpdatePixelColorInfo(Color newColor)
    {
        if (newColor == _colorPreviewBrush.Color)
            return;

        _colorPreviewBrush.Color = newColor;

        ColorPreviewBrush = _colorPreviewBrush;
        PixelColorString = $"#{newColor.R:X2}{newColor.G:X2}{newColor.B:X2}";
    }

    /// <summary>
    /// Updates the string representation of the current pixel position on screen.
    /// </summary>
    /// 
    /// <param name="newPos">
    /// The new pixel position.
    /// </param>
    private void UpdatePixelPositionString(Point newPos) =>
        PixelPositionString = $"X:{newPos.X} Y:{newPos.Y}";

    #endregion

    static PixelMagnifierWindow()
    {
        LoadSoundStreams();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PixelMagnifierWindow"/>
    /// class.
    /// </summary>
    public PixelMagnifierWindow() : this(new PixelMagnifierWindowConfig()) { }

    /// <summary>
    /// <inheritdoc cref="PixelMagnifierWindow()"/>
    /// </summary>
    /// 
    /// <param name="config">
    /// Configuration parameters of this window.
    /// </param>
    public PixelMagnifierWindow(PixelMagnifierWindowConfig config)
    {
        InitializeComponent();

        DataContext = this;

        _config = config ?? throw new ArgumentNullException(nameof(config));
        _cursor = LoadCursorFromResource(ResourceUris.Cursors.Cross, true);
        _dpi = VisualTreeHelper.GetDpi(this);

        OverlayImage = CaptureDesktopScreen()!;

        Cursor = _cursor;

        Magnifier.PixelChanged += OnPixelChanged;

        Loaded += (_, _) =>
        {
            ApplyConfig();

            // Set the initial magnifier position at the current mouse position
            UpdateMagnifierPosition(GetMousePosition());

            Magnifier.StartCapture();
        };

        Closing += (_, _) =>
        {
            Magnifier.StopCapture();

            SelectedPixelColor = Magnifier.PixelColor;
            SelectedPixelPosition = Magnifier.PixelPosition;

            if (Cursor == _cursor)
            {
                Cursor = null;
                _cursor?.Dispose();
            }

            ExtractConfig();
        };

        Unloaded += (_, _) =>
        {
            Magnifier.PixelChanged -= OnPixelChanged;
        };
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        UpdateMagnifierPosition(e.GetPosition(this));
    }

    protected override void OnMouseWheel(MouseWheelEventArgs e)
    {
        base.OnMouseWheel(e);

        if (Keyboard.Modifiers == ModifierKeys.Control)
        {
            if (e.Delta > 0)
                PixelMagnifierUICommands.ZoomIn.Execute(null, this);
            else if (e.Delta < 0)
                PixelMagnifierUICommands.ZoomOut.Execute(null, this);

            e.Handled = true;
        }
        else if (Keyboard.Modifiers == ModifierKeys.Shift)
        {
            if (e.Delta > 0)
                PixelMagnifierUICommands.ExpandView.Execute(null, this);
            else if (e.Delta < 0)
                PixelMagnifierUICommands.ShrinkView.Execute(null, this);

            e.Handled = true;
        }
    }

    protected override void OnPreviewKeyDown(KeyEventArgs e)
    {
        base.OnPreviewKeyDown(e);

        switch (e.Key)
        {
            case Key.Left:
            case Key.Right:
            case Key.Up:
            case Key.Down:
                HandlePixelNavigation();
                e.Handled = true;
                break;
        }
    }

    private void OnPixelChanged(object? sender, PixelChangedEventArgs e)
    {
        UpdatePixelColorInfo(e.Color);
        UpdatePixelPositionString(e.ScreenPosition);
    }

    private void OnToggleGridExecuted(object sender, ExecutedRoutedEventArgs e) =>
        Magnifier.ShowGrid = !Magnifier.ShowGrid;

    private void OnExpandViewExecuted(object sender, ExecutedRoutedEventArgs e) =>
        Magnifier.PixelColumns = Math.Min(Magnifier.PixelColumns + 2, PixelMagnifierWindowConfig.PixelColumnsMax);

    private void OnShrinkViewExecuted(object sender, ExecutedRoutedEventArgs e) =>
        Magnifier.PixelColumns = Math.Max(Magnifier.PixelColumns - 2, PixelMagnifierWindowConfig.PixelColumnsMin);

    private void OnZoomInExecuted(object sender, ExecutedRoutedEventArgs e) =>
        Magnifier.PixelSize = Math.Min(Magnifier.PixelSize + 1, PixelMagnifierWindowConfig.PixelSizeMax);

    private void OnZoomOutExecuted(object sender, ExecutedRoutedEventArgs e) =>
        Magnifier.PixelSize = Math.Max(Magnifier.PixelSize - 1, PixelMagnifierWindowConfig.PixelSizeMin);

    private void OnIncreaseColorSamplingAreaExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        var idx = Array.IndexOf(_samplingModes, Magnifier.SamplingMode);
        if (idx < _samplingModes.Length - 1)
            Magnifier.SamplingMode = _samplingModes[idx + 1];
    }

    private void OnDecreaseColorSamplingAreaExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        var idx = Array.IndexOf(_samplingModes, Magnifier.SamplingMode);
        if (idx > 0)
            Magnifier.SamplingMode = _samplingModes[idx - 1];
    }

    private void OnCloseExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        if (e.Command == PixelMagnifierUICommands.Close)
        {
            DialogResult = true;

            Clipboard.SetText(PixelColorString);
            PlaySoundEffect(ConfirmationSoundEffect);
        }

        Close();
    }

    /// <summary>
    /// Disposes all preloaded sound streams.
    /// </summary>
    public static void DisposePreloadedStreams()
    {
        foreach (var stream in s_preloadedSoundStreams.Values)
            stream?.Dispose();

        s_preloadedSoundStreams.Clear();
    }

    private void ApplyConfig()
    {
        Magnifier.PixelColumns = Math.Clamp(
            _config.PixelColumns,
            PixelMagnifierWindowConfig.PixelColumnsMin,
            PixelMagnifierWindowConfig.PixelColumnsMax);

        Magnifier.PixelSize = Math.Clamp(
            _config.PixelSize,
            PixelMagnifierWindowConfig.PixelSizeMin,
            PixelMagnifierWindowConfig.PixelSizeMax);

        Magnifier.RefreshInterval = Math.Clamp(
            _config.RefreshInterval,
            PixelMagnifierWindowConfig.RefreshIntervalMin,
            PixelMagnifierWindowConfig.RefreshIntervalMax);

        Magnifier.SamplingMode = _config.SamplingMode;
        Magnifier.ShowGrid = _config.ShowGrid;
    }

    private void ExtractConfig()
    {
        _config.PixelSize = Magnifier.PixelSize;
        _config.PixelColumns = Magnifier.PixelColumns;
        _config.RefreshInterval = Magnifier.RefreshInterval;
        _config.SamplingMode = Magnifier.SamplingMode;
        _config.ShowGrid = Magnifier.ShowGrid;
    }
}