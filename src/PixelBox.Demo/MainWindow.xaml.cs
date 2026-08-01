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
            // A lower value will result in a smoother experience, but will also result
            // in higher CPU usage due to the more frequent screen captures. An ideal
            // value is between 15 throuh 30
            RefreshInterval = 15,

            ShowInfoPanel = true
        };

        // You can configure various keybindings through this static method prior to
        // instantiating LoupeWindow
        //
        // Besides the configurable keybindings, you can use CTRL or SHIFT + MOUSE WHEEL
        // to set the GridSize and PixelSize properties of the Loupe control
        //
        // You can also use the arrow keys to move the cursor pixel by pixel in
        // 8-directional manner. Holding down the SHIFT key while using arrow keys will
        // accelerate the movement
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
        // window will be created with default configuration. Check LoupeWindowConfig
        // to see what the default config is
        var loupe = new LoupeWindow(_loupeWindowCfg);

        // ShowDialog() will return true if the window was closed via either ENTER key
        // (by defalt) or MOUSE LEFT CLICK. You can use the return result to update your
        // UI conditionally
        //
        // ESC key acts as a cancel button. It will close the window but ShowDialog()
        // will return false
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

    private async void CopyColorTextButton_Click(object sender, RoutedEventArgs e)
    {
        Clipboard.SetText(ColorTextBlock.Text.Split(' ')[0]);

        CopyIcon.Data = IconGeometries.Check;
        CopyColorTextButton.IsEnabled = false;

        await Task.Delay(1000);

        CopyIcon.Data = IconGeometries.Copy;
        CopyColorTextButton.IsEnabled = true;
    }
}