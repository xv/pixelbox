// Copyright 2025 Jad Altahan <xv.git@aol.com>
// SPDX-License-Identifier: MIT

using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;

using Windows.Win32.Graphics.Gdi;
using Windows.Win32;

using PixelBox.BindingConverters;
using PixelBox.Resources;

namespace PixelBox;

/// <summary>
/// Represents a mouse-tracked pixel magnifier window.
/// </summary>
public partial class LoupeWindow : Window
{
    #region Fields

    private DpiScale _dpi;

    private const double WindowOffset = 2;

    private readonly LoupeWindowConfig _config;

    private readonly SolidColorBrush _colorPreviewBrush =
        new(Colors.Transparent);

    private readonly Cursor _cursor;

    private readonly PixelSamplingMode[] _samplingModes =
        Enum.GetValues<PixelSamplingMode>();

    private static readonly LoupeWindowKeyBindings _keyBindings =
        LoupeWindowKeyBindings.CreateDefault();

    #endregion
    #region Dependency Properties

    /// <summary>
    /// Dependency property for the <see cref="ColorPreviewBrush"/> property.
    /// </summary>
    private static readonly DependencyProperty ColorPreviewBrushProperty =
        DependencyProperty.Register(
            nameof(ColorPreviewBrush),
            typeof(Brush),
            typeof(LoupeWindow));

    /// <summary>
    /// Gets or sets the brush used to render the color preview element.
    /// </summary>
    internal Brush ColorPreviewBrush
    {
        get => (Brush)GetValue(ColorPreviewBrushProperty);
        set => SetValue(ColorPreviewBrushProperty, value);
    }

    /// <summary>
    /// Dependency property for the <see cref="InfoPanelMinWidth"/> property.
    /// </summary>
    public static readonly DependencyProperty InfoPanelMinWidthProperty =
        DependencyProperty.Register(
            nameof(InfoPanelMinWidth),
            typeof(double),
            typeof(LoupeWindow),
            new PropertyMetadata(150d));

    /// <summary>
    /// Gets or sets the the minimum width of the info panel containing the
    /// current pixel's color and screen position.
    /// </summary>
    public double InfoPanelMinWidth
    {
        get => (double)GetValue(InfoPanelMinWidthProperty);
        set => SetValue(InfoPanelMinWidthProperty, value);
    }

    /// <summary>
    /// Dependency property for the <see cref="OverlayImage"/> property.
    /// </summary>
    private static readonly DependencyProperty OverlayImageProperty =
        DependencyProperty.Register(
            nameof(OverlayImage),
            typeof(ImageSource),
            typeof(LoupeWindow));

    /// <summary>
    /// Gets or sets the overlay image used as the background for the magnifier.
    /// </summary>
    /// 
    /// <remarks>
    /// This image should snapshot of the desktop screen. The idea here is that
    /// the overlay would prevent the mouse cursor from interacting with the
    /// desktop's UI elements underneath.
    /// </remarks>
    internal ImageSource? OverlayImage
    {
        get => (ImageSource)GetValue(OverlayImageProperty);
        set => SetValue(OverlayImageProperty, value);
    }

    /// <inheritdoc cref="Loupe.PixelColor"/>
    public Color PixelColor
    {
        get => (Color)GetValue(PixelColorProperty);
        private set => SetValue(PixelColorProperty, value);
    }

    /// <summary>
    /// Dependency property for the <see cref="PixelColor"/> property.
    /// </summary>
    public static readonly DependencyProperty PixelColorProperty =
        DependencyProperty.Register(
            nameof(PixelColor),
            typeof(Color),
            typeof(LoupeWindow),
            new PropertyMetadata(default(Color)));

    /// <summary>
    /// Gets or sets the value converter used to transform pixel color values
    /// for data binding operations and user interface representation.
    /// </summary>
    public IValueConverter PixelColorConverter
    {
        get => (IValueConverter)GetValue(PixelColorConverterProperty);
        set => SetValue(PixelColorConverterProperty, value);
    }

    /// <summary>
    /// Dependency property for the <see cref="PixelColorConverter"/> property.
    /// </summary>
    public static readonly DependencyProperty PixelColorConverterProperty =
        DependencyProperty.Register(
            nameof(PixelColorConverter),
            typeof(IValueConverter),
            typeof(LoupeWindow),
            new PropertyMetadata(new ColorToStringConverter()));

    /// <inheritdoc cref="Loupe.PixelPosition"/>
    public Point PixelPosition
    {
        get => (Point)GetValue(PixelPositionProperty);
        private set => SetValue(PixelPositionProperty, value);
    }

    /// <summary>
    /// Dependency property for the <see cref="PixelPosition"/> property.
    /// </summary>
    public static readonly DependencyProperty PixelPositionProperty =
        DependencyProperty.Register(
            nameof(PixelPosition),
            typeof(Point),
            typeof(LoupeWindow),
            new PropertyMetadata(default(Point)));

    /// <summary>
    /// Gets or sets the value converter used to transform pixel screen position
    /// values for data binding operations and user interface representation.
    /// </summary>
    public IValueConverter PixelPositionConverter
    {
        get => (IValueConverter)GetValue(PixelPositionConverterProperty);
        set => SetValue(PixelPositionConverterProperty, value);
    }

    /// <summary>
    /// Dependency property for the <see cref="PixelPositionConverter"/> property.
    /// </summary>
    public static readonly DependencyProperty PixelPositionConverterProperty =
        DependencyProperty.Register(
            nameof(PixelPositionConverter),
            typeof(IValueConverter),
            typeof(LoupeWindow),
            new PropertyMetadata(new PointToStringConverter()));

    #endregion
    #region Properties

    /// <summary>
    /// Gets the key bindings for the window.
    /// </summary>
    public static LoupeWindowKeyBindings KeyBindings => _keyBindings;

    /// <summary>
    /// Gets or sets whether the info panel containing the current pixel's color
    /// and screen position is displayed.
    /// </summary>
    private bool ShowInfoPanel
    {
        get => InfoPanelHost.Visibility == Visibility.Visible;
        set => InfoPanelHost.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
    }

    #endregion
    #region Methods

    /// <summary>
    /// Applies input bindings for various commands to the window.
    /// </summary>
    private void ApplyInputBindings()
    {
        InputBindings.Clear();

        InputBindings.Add(_keyBindings.ToggleGrid);
        InputBindings.Add(_keyBindings.IncreaseGridSize);
        InputBindings.Add(_keyBindings.DecreaseGridSize);
        InputBindings.Add(_keyBindings.IncreasePixelSize);
        InputBindings.Add(_keyBindings.DecreasePixelSize);
        InputBindings.Add(_keyBindings.IncreaseColorSamplerSize);
        InputBindings.Add(_keyBindings.DecreaseColorSamplerSize);
        InputBindings.Add(_keyBindings.Close);

        InputBindings.Add(new KeyBinding
        {
            Command = ApplicationCommands.Close,
            Key = Key.Escape
        });

        InputBindings.Add(new MouseBinding
        {
            Command = LoupeWindowCommands.Close,
            MouseAction = MouseAction.LeftClick
        });
    }

    /// <summary>
    /// Applies user-set configuration.
    /// </summary>
    private void ApplyConfig()
    {
        Magnifier.GridSize = Math.Clamp(
            _config.GridSize,
            LoupeWindowConfig.GridSizeMin,
            LoupeWindowConfig.GridSizeMax);

        Magnifier.PixelSize = Math.Clamp(
            _config.PixelSize,
            LoupeWindowConfig.PixelSizeMin,
            LoupeWindowConfig.PixelSizeMax);

        Magnifier.RefreshInterval = Math.Clamp(
            _config.RefreshInterval,
            LoupeWindowConfig.RefreshIntervalMin,
            LoupeWindowConfig.RefreshIntervalMax);

        Magnifier.SamplingMode = _config.SamplingMode;
        Magnifier.ShowGrid = _config.ShowGrid;
        Magnifier.ShowGrid = _config.ShowGrid;
        ShowInfoPanel = _config.ShowInfoPanel;
    }

    /// <summary>
    /// Configures the key bindings for the window.
    /// </summary>
    /// 
    /// <param name="config">
    /// Receives a <see cref="LoupeWindowKeyBindings"/> instance to
    /// modify the key bindings.
    /// </param>
    ///
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="config"/> is <see langword="null"/>.
    /// </exception>
    public static void ConfigureKeyBindings(Action<LoupeWindowKeyBindings> config)
    {
        ArgumentNullException.ThrowIfNull(config);
        config(_keyBindings);
    }

    /// <summary>
    /// Extracts user-set configuration.
    /// </summary>
    private void ExtractConfig()
    {
        _config.PixelSize = Magnifier.PixelSize;
        _config.GridSize = Magnifier.GridSize;
        _config.RefreshInterval = Magnifier.RefreshInterval;
        _config.SamplingMode = Magnifier.SamplingMode;
        _config.ShowGrid = Magnifier.ShowGrid;
        _config.ShowInfoPanel = ShowInfoPanel;
    }

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
    /// Retrieves the current mouse cursor screen position.
    /// </summary>
    /// 
    /// <returns>
    /// A <see cref="Point"/> representing the current cursor position,
    /// converted from screen coordinates to the coordinate system of this
    /// window.
    /// </returns>
    private Point GetMousePosition()
    {
        PInvoke.GetCursorPos(out System.Drawing.Point p);
        return new Point(p.X / _dpi.DpiScaleX, p.Y / _dpi.DpiScaleY);
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
        var hBitmap = BitmapInterop.CaptureRectToHBitmap(VirtualScreenInfo.Bounds);
        if (hBitmap == HBITMAP.Null)
            return null;

        try { return BitmapInterop.BitmapSourceFromHBitmap(hBitmap, true); }
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

        var sx = _dpi.DpiScaleX;
        var sy = _dpi.DpiScaleY;

        // If at the right border of the screen, move the magnifier leftward
        if ((left + width) > VirtualScreenInfo.Right / sx)
            left = pos.X - width - WindowOffset;

        // If the bottom border of the screen, move the magnifier upward
        if ((top + height) > VirtualScreenInfo.Bottom / sy)
            top = pos.Y - height - WindowOffset;

        // Applying sx/sy prevents the magnifier grid lines from pixel-shifting
        // on high-DPI screens
        //
        // Flooring (or even ceiling) prevents rounding artifacts noticeable when
        // the mouse move by 1px which sometimes causes the magnifier window to
        // remain still until the mouse moves again.
        MagnifierTranslate.X = Math.Floor(left * sx) / sx;
        MagnifierTranslate.Y = Math.Floor(top * sy) / sy;
    }

    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="LoupeWindow"/>
    /// class.
    /// </summary>
    public LoupeWindow() : this(new LoupeWindowConfig()) { }

    /// <summary>
    /// <inheritdoc cref="LoupeWindow()"/>
    /// </summary>
    /// 
    /// <param name="config">
    /// Configuration parameters of this window.
    /// </param>
    public LoupeWindow(LoupeWindowConfig config)
    {
        InitializeComponent();

        DataContext = this;

        _config = config ?? throw new ArgumentNullException(nameof(config));
        _cursor = LoadCursorFromResource(ResourceUris.Cursors.Cross, true);
        _dpi = VisualTreeHelper.GetDpi(this);

        Cursor = _cursor;

        ApplyInputBindings();
        ApplyConfig();
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);

        // It appears that setting the overlay here as opposed to the Loaded event
        // is more reliable in that if the mouse was over an interactive element
        // (e.g., button with hover effects), the overlay properly reflects the
        // original UI state
        OverlayImage = CaptureDesktopScreen();
        OverlayImage?.Freeze();

        UpdateMagnifierPosition(GetMousePosition());
    }

    protected override void OnContentRendered(EventArgs e)
    {
        base.OnContentRendered(e);

        Magnifier.StartCapture();
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        base.OnClosing(e);

        PixelColor = Magnifier.PixelColor;
        PixelPosition = Magnifier.PixelPosition;

        ExtractConfig();
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);

        Magnifier.StopCapture();
        Magnifier.PixelChanged -= OnPixelChanged;
        Magnifier.Dispose();

        if (Cursor == _cursor)
        {
            Cursor = null;
            _cursor?.Dispose();
        }
    }

    protected override void OnDpiChanged(DpiScale oldDpi, DpiScale newDpi)
    {
        base.OnDpiChanged(oldDpi, newDpi);
        _dpi = newDpi;
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
                LoupeWindowCommands.IncreasePixelSize.Execute(null, this);
            else if (e.Delta < 0)
                LoupeWindowCommands.DecreasePixelSize.Execute(null, this);

            e.Handled = true;
        }
        else if (Keyboard.Modifiers == ModifierKeys.Shift)
        {
            if (e.Delta > 0)
                LoupeWindowCommands.IncreaseGridSize.Execute(null, this);
            else if (e.Delta < 0)
                LoupeWindowCommands.DecreaseGridSize.Execute(null, this);

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
            case Key.System:
                // Prevent Alt key from (probably) processing WM_ENTERMENULOOP
                // which causes the window to stop following the mouse until
                // the key is pressed again
                e.Handled = true;
                break;
        }
    }

    private void OnPixelChanged(object? sender, PixelChangedEventArgs e)
    {
        PixelColor = e.Color;
        PixelPosition = e.ScreenPosition;

        _colorPreviewBrush.Color = e.Color;
        ColorPreviewBrush = _colorPreviewBrush;
    }

    private void OnToggleGridExecuted(object sender, ExecutedRoutedEventArgs e) =>
        Magnifier.ShowGrid = !Magnifier.ShowGrid;

    private void OnIncreaseGridSizeExecuted(object sender, ExecutedRoutedEventArgs e) =>
        Magnifier.GridSize = Math.Min(Magnifier.GridSize + 2, LoupeWindowConfig.GridSizeMax);

    private void OnDecreaseGridSizeExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        // Sampling kernel cannot be larger than the number of pixels available
        if (Magnifier.GridSize <= (int)Magnifier.SamplingMode)
            return;

        Magnifier.GridSize = Math.Max(Magnifier.GridSize - 2, LoupeWindowConfig.GridSizeMin);
    }

    private void OnIncreasePixelSizeExecuted(object sender, ExecutedRoutedEventArgs e) =>
        Magnifier.PixelSize = Math.Min(Magnifier.PixelSize + 1, LoupeWindowConfig.PixelSizeMax);

    private void OnDecreasePixelSizeExecuted(object sender, ExecutedRoutedEventArgs e) =>
        Magnifier.PixelSize = Math.Max(Magnifier.PixelSize - 1, LoupeWindowConfig.PixelSizeMin);

    private void OnIncreaseColorSamplerSizeExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        if ((int)Magnifier.SamplingMode >= Magnifier.GridSize)
            return;

        var idx = Array.IndexOf(_samplingModes, Magnifier.SamplingMode);
        if (idx < _samplingModes.Length - 1)
            Magnifier.SamplingMode = _samplingModes[idx + 1];
    }

    private void OnDecreaseColorSamplerSizeExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        var idx = Array.IndexOf(_samplingModes, Magnifier.SamplingMode);
        if (idx > 0)
            Magnifier.SamplingMode = _samplingModes[idx - 1];
    }

    private void OnCloseExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        if (e.Command == LoupeWindowCommands.Close)
            DialogResult = true;

        Close();
    }
}