using System.Windows.Input;
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
    private Point _pixelPos;

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

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (e.Key == Key.G)
            Magnifier.ShowGrid = !Magnifier.ShowGrid;

        if (e.Key == Key.Space)
            Magnifier.ToggleCapture();
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);

        Magnifier.PixelChanged -= OnMagnifierPixelChanged;
        Magnifier.MouseDown -= OnMagnifierMouseDown;
        Magnifier.MouseUp -= OnMagnifierMouseUp;
    }
}