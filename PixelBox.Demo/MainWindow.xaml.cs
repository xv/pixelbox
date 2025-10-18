using System.Windows.Input;
using System.Windows.Media;
using System.Windows;

namespace PixelBox.Demo;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (e.Key == Key.Space)
        {
            // var cfg = new PixelMagnifierWindowConfig { PixelSize = 10, ShowGrid = false };

            var magWindow = new PixelMagnifierWindow()
            {
                ShowInfoPanel = true,
                ConfirmationSoundEffect = PixelMagnifierWindow.SoundEffect.Pop
            };

            if (magWindow.ShowDialog() == true)
            {
                var color = magWindow.SelectedPixelColor;
                var coord = magWindow.SelectedPixelPosition;

                Color.Text = $"#{color!.Value.R:X2}{color!.Value.G:X2}{color!.Value.B:X2}";
                Coord.Text = $"X: {coord!.Value.X} Y: {coord!.Value.Y}";

                ColorBox.Fill = new SolidColorBrush(color!.Value);
            }
        }
    }
}