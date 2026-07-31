using System.Windows.Input;
using System.Windows.Media;
using System.Windows;

namespace PixelBox.Demo;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly LoupeWindowConfig _loupeWindowCfg;

    public MainWindow()
    {
        InitializeComponent();

        _loupeWindowCfg = new LoupeWindowConfig
        {
            RefreshInterval = 15,
            ShowInfoPanel = true
        };

        // You can configure various keybindings through this static method prior to
        // instantiating LoupeWindow
        //
        // Besides the configurable keybindings, you can use CTRL or SHIFT + Mouse Wheel
        // to set the GridSize and PixelSize properties of the Loupe control
        LoupeWindow.ConfigureKeyBindings(bindings =>
        {
            bindings.Close = new KeyBinding
            {
                Command = LoupeWindowCommands.Close,
                Key = Key.Enter, // Just for demo; Enter is already the default
                Modifiers = ModifierKeys.None
            };
        });
    }

    private static string RgbStringFromColor(Color color) =>
        $"{color.R}, {color.G}, {color.B}";

    private static string HexStringFromColor(Color color) =>
        $"#{color.R:X2}{color.G:X2}{color.B:X2}";

    private void ShowLoupeWindow()
    {
        // Passing LoupeWindowConfig instance is optional. If not provided, the
        // window will be created with default configuration
        var loupe = new LoupeWindow(_loupeWindowCfg);

        // ShowDialog() will return true if the window was closed via either Enter key or
        // mouse left click. You can use the return result to update your UI conditionally
        if (loupe.ShowDialog() != true)
            return;

        var color = loupe.PixelColor;
        var position = loupe.PixelPosition;

        ColorTextBlock.Text = $"{HexStringFromColor(color)} ({RgbStringFromColor(color)})";
        PositionTextBlock.Text = $"X: {position.X} Y: {position.Y}";

        ColorPreviewBox.Fill = new SolidColorBrush(color);
        PixelInfoPanel.Visibility = Visibility.Visible;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (e.Key == Key.Space)
            ShowLoupeWindow();
    }

    private void Kbd_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        ShowLoupeWindow();
    }
}