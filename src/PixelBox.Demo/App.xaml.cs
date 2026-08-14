using System.Windows;

using PixelBox.Demo.Windows;

namespace PixelBox.Demo;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var main = new LoupeWindowDemoWindow();
        main.ShowDialog();
    }
}