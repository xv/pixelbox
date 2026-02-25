using System.Windows.Input;
using System.Windows.Media;
using System.Windows;

namespace PixelBox.Demo;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly PixelMagnifierWindowConfig _magWindowCfg;
    private readonly SolidColorBrush _colorbrush;

    public MainWindow()
    {
        InitializeComponent();

        _colorbrush = new SolidColorBrush();

        _magWindowCfg = new PixelMagnifierWindowConfig 
        {
            RefreshInterval = 30,
            ShowInfoPanel = true
        };
    }

    private static string RgbStringFromColor(Color color) =>
        $"{color.R}, {color.G}, {color.B}";

    private static string HexStringFromColor(Color color) =>
        $"#{color.R:X2}{color.G:X2}{color.B:X2}";

    private void ShowMagnifierWindow()
    {
        var magWindow = new PixelMagnifierWindow(_magWindowCfg)
        {
            ConfirmationSoundEffect = PixelMagnifierWindow.SoundEffect.Pop
        };

        // ShowDialog() will return true if the window was closed via either Enter key or
        // mouse left click. You can use the return result to update your UI conditionally
        if (magWindow.ShowDialog() != true)
            return;

        var color = magWindow.PixelColor;
        var position = magWindow.PixelPosition;

        _colorbrush.Color = color;

        ColorTextBlock.Text = $"{HexStringFromColor(color)} ({RgbStringFromColor(color)})";
        PositionTextBlock.Text = $"X: {position.X} Y: {position.Y}";

        ColorPreviewBox.Fill = _colorbrush;
        PixelInfoPanel.Visibility = Visibility.Visible;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (e.Key == Key.Space)
            ShowMagnifierWindow();
    }

    private void Kbd_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        ShowMagnifierWindow();
    }
}