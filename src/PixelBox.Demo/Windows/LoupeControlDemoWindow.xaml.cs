using System.Windows.Input;
using System.Windows.Media;
using System.Windows;

using Windows.Win32;

namespace PixelBox.Demo.Windows;

/// <summary>
/// Interaction logic for LoupeControlDemoWindow.xaml
/// </summary>
public partial class LoupeControlDemoWindow : Window
{
    #region Fields

    private Point _posOnMouseDown;

    private Color _color;
    private Point _pixelPos;

    private static readonly SolidColorBrush _colorBrush = new();

    #endregion
    #region Methods

    private static Point GetCursorPos()
    {
        PInvoke.GetCursorPos(out System.Drawing.Point p);
        return new Point(p.X, p.Y);
    }

    private static void SetCursorPos(Point pos) =>
        PInvoke.SetCursorPos((int)pos.X, (int)pos.Y);

    #endregion

    public LoupeControlDemoWindow()
    {
        InitializeComponent();

        Magnifier.PixelChanged += OnMagnifierPixelChanged;
        Magnifier.MouseDown += OnMagnifierMouseDown;
        Magnifier.MouseUp += OnMagnifierMouseUp;
    }

    private void OnMagnifierPixelChanged(object? sender, PixelChangedEventArgs e)
    {
        _pixelPos = e.ScreenPosition;
        _color = e.Color;

        _colorBrush.Color = _color;
        ColorPreviewBox.Fill = _colorBrush;

        ColorTextBlock.Text = $"#{_color.R:X2}{_color.G:X2}{_color.B:X2}";
        PositionTextBlock.Text = $"[{_pixelPos.X},{_pixelPos.Y}]";
    }

    private void OnMagnifierMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton != MouseButton.Left)
            return;

        _posOnMouseDown = GetCursorPos();
        SetCursorPos(_pixelPos);

        Magnifier.CaptureMouse();
        Magnifier.StartCapture();
    }

    private void OnMagnifierMouseUp(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton != MouseButton.Left)
            return;

        SetCursorPos(_posOnMouseDown);

        Magnifier.ReleaseMouseCapture();
        Magnifier.StopCapture();
    }

    #region Command Handlers

    #region ToggleCapture

    private void OnToggleCaptureCanExecute(object sender, CanExecuteRoutedEventArgs e) =>
        e.CanExecute = true;

    private void OnToggleCaptureExecuted(object sender, ExecutedRoutedEventArgs e) =>
        Magnifier.ToggleCapture();

    #endregion
    #region ToggleContinuousCapture

    private void OnToggleContinuousCaptureCanExecute(object sender, CanExecuteRoutedEventArgs e) =>
        e.CanExecute = true;

    private void OnToggleContinuousCaptureExecuted(object sender, ExecutedRoutedEventArgs e) =>
        Magnifier.ContinuousCapture = !Magnifier.ContinuousCapture;

    #endregion
    #region ToggleGrid

    private void OnToggleGridCanExecute(object sender, CanExecuteRoutedEventArgs e) =>
        e.CanExecute = true;

    private void OnToggleGridExecuted(object sender, ExecutedRoutedEventArgs e) =>
        Magnifier.ShowGrid = !Magnifier.ShowGrid;

    #endregion
    #region SetSamplerSizeSingle

    private void OnSetSamplerSizeSingleCanExecute(object sender, CanExecuteRoutedEventArgs e) =>
        e.CanExecute = Magnifier.SamplingMode != PixelSamplingMode.Single;

    private void OnSetSamplerSizeSingleExecuted(object sender, ExecutedRoutedEventArgs e) =>
        Magnifier.SamplingMode = PixelSamplingMode.Single;

    #endregion
    #region SetSamplerSize3x3

    private void OnSetSamplerSize3x3CanExecute(object sender, CanExecuteRoutedEventArgs e) =>
        e.CanExecute = Magnifier.SamplingMode != PixelSamplingMode.ThreeByThree;

    private void OnSetSamplerSize3x3Executed(object sender, ExecutedRoutedEventArgs e) =>
        Magnifier.SamplingMode = PixelSamplingMode.ThreeByThree;

    #endregion
    #region SetSamplerSize5x5

    private void OnSetSamplerSize5x5CanExecute(object sender, CanExecuteRoutedEventArgs e) =>
        e.CanExecute = Magnifier.SamplingMode != PixelSamplingMode.FiveByFive;

    private void OnSetSamplerSize5x5Executed(object sender, ExecutedRoutedEventArgs e) =>
        Magnifier.SamplingMode = PixelSamplingMode.FiveByFive;

    #endregion
    #region ToggleLockPosition

    private void OnToggleLockPositionCanExecute(object sender, CanExecuteRoutedEventArgs e) =>
        e.CanExecute = true;

    private void OnToggleLockPositionExecuted(object sender, ExecutedRoutedEventArgs e) =>
        Magnifier.PositionLocked = !Magnifier.PositionLocked;

    #endregion
    #region ToggleLockX

    private void OnToggleLockXCanExecute(object sender, CanExecuteRoutedEventArgs e) =>
        e.CanExecute = true;

    private void OnToggleLockXExecuted(object sender, ExecutedRoutedEventArgs e) =>
        Magnifier.PositionXLocked = !Magnifier.PositionXLocked;

    #endregion
    #region ToggleLockY

    private void OnToggleLockYCanExecute(object sender, CanExecuteRoutedEventArgs e) =>
        e.CanExecute = true;

    private void OnToggleLockYExecuted(object sender, ExecutedRoutedEventArgs e) =>
        Magnifier.PositionYLocked = !Magnifier.PositionYLocked;

    #endregion

    #endregion

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);

        Magnifier.PixelChanged -= OnMagnifierPixelChanged;
        Magnifier.MouseDown -= OnMagnifierMouseDown;
        Magnifier.MouseUp -= OnMagnifierMouseUp;
    }
}