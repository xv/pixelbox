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

    public MainWindow()
    {
        InitializeComponent();

        _magWindowCfg = new PixelMagnifierWindowConfig 
        {
            RefreshInterval = 30
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

        // ShowDialog() returns true if the magnifier was closed via
        // mouse left click, Space or Enter keys
        if (magWindow.ShowDialog() != true)
            return;

        var color = magWindow.SelectedPixelColor!;
        var coords = magWindow.SelectedPixelPosition!;

        ColorText.Text = $"{HexStringFromColor(color.Value)} ({RgbStringFromColor(color.Value)})";
        CoordsText.Text = $"X: {coords.Value.X} Y: {coords.Value.Y}";

        ColorPreviewBox.Fill = new SolidColorBrush(color.Value);
        ColorInfoPanel.Visibility = Visibility.Visible;
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