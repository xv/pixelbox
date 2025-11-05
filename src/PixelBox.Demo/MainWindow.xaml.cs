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

    private void ShowMagnifierWindow()
    {
        var magWindow = new PixelMagnifierWindow(_magWindowCfg)
        {
            ConfirmationSoundEffect = PixelMagnifierWindow.SoundEffect.Pop
        };

        if (magWindow.ShowDialog() != true)
            return;

        var color = magWindow.SelectedPixelColor;
        var coords = magWindow.SelectedPixelPosition;

        ColorText.Text = $"#{color!.Value.R:X2}{color!.Value.G:X2}{color!.Value.B:X2}";
        CoordsText.Text = $"X: {coords!.Value.X} Y: {coords!.Value.Y}";

        ColorPreviewBox.Fill = new SolidColorBrush(color!.Value);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (e.Key == Key.Space)
            ShowMagnifierWindow();
    }
}