using System.Diagnostics;
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
        PixelMag.StartCapture();
    }

    private void PixelMag_PixelChanged(object sender, PixelChangedEventArgs e)
    {
        var colorStr = e.Color.ToString();
        Title = $"{colorStr[3..]}";
    }

    private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key == System.Windows.Input.Key.Space)
            PixelMag.ToggleCapture();
        else if (e.Key == System.Windows.Input.Key.Q)
            PixelMag.SamplingMode = PixelSamplingMode.Single;
        else if (e.Key == System.Windows.Input.Key.W)
            PixelMag.SamplingMode = PixelSamplingMode.ThreeByThree;
        else if (e.Key == System.Windows.Input.Key.G)
            PixelMag.ShowGrid = !PixelMag.ShowGrid;
        else if (e.Key == System.Windows.Input.Key.C)
            PixelMag.PixelColumns +=2;
        else if (e.Key == System.Windows.Input.Key.S)
            PixelMag.PixelSize += 1;
    }
}