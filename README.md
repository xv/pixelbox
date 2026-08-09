About
-----
PixelBox is a fast and customizable BitBlt-based pixel magnification control for WPF. The library contains the standalone magnifier control itself, and a ready-to-use mouse-tracked pixel magnifier and color picker window, similar to what you would find in browser developer tools.

Features
--------
In a nutshell, what the control can provide is:
- Adjustable grid size, pixel size, and refresh rate.
- Toggleable grid lines.
- Color sampling ranging from single pixel up to a 5×5 region.
- Individual screen axes locking, like in macOS' Digital Color Meter.
- High-DPI support.

Here's a quick demo of the built-in magnifier window:

<!-- Full URL is used here to make it display properly on nuget.org -->
![demo](https://github.com/xv/PixelBox.WPF/blob/master/media/demo.gif)

Requirements
------------
- **Operating System**: Windows 7 SP1 or later.
- **.NET**: [8.0](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) or later (uses C# 12 features).

Installation
------------
You can install PixelBox either by downloading a release from this repository and referencing it manually in your project, or by using NuGet:
```console
dotnet add package PixelBox
```

Quick Start
-----------
### Using the Control Directly

> [!CAUTION]
> `Loupe` holds unmanaged resources for its lifetime and implements `IDisposable`. Call `Dispose()` when the control is no longer needed to properly release these resources.

```xml
<Window x:Class="WpfApp.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:pb="clr-namespace:PixelBox;assembly=PixelBox">
    <Grid>
        <!-- The size of the Loupe control is automatically determined by its
             GridSize and PixelSize properties. Do not set the control size
             manually -->
        <pb:Loupe HorizontalAlignment="Left" VerticalAlignment="Top"
            GridSize="15"
            PixelSize="10"
            ShowGrid="True"
            RefreshInterval="30"
            PixelChanged="OnPixelChanged"/>
    </Grid>
</Window>
```

The `PixelChanged` event is raised whenever the sampled pixel changes. You can use this event to receive the new sampled color and screen position.

```csharp
private Color _color;
private Point _position;

private void OnPixelChanged(object? sender, PixelChangedEventArgs e)
{
    _color = e.Color;
    _position = e.ScreenPosition;
}
```

The `Loupe` control does not automatically capture the screen. You must call `StartCapture()` to begin capturing, and `StopCapture()` to stop.

### Using the Built-In Window

> [!TIP]
> A project demonstrating the use of the built-in window is provided under `PixelBox.Demo`.

```csharp
var picker = new PixelBox.LoupeWindow();

// ShowDialog() will return true if the window was closed via either Enter key or
// mouse left click. You can use the return result to update your UI conditionally
picker.ShowDialog();

var color = picker.PixelColor;
var position = picker.PixelPosition;
```

#### Configuring the Window Keybinds

The magnifier window uses the following keybinds by default:

| **Command**                                     | **Keybind**                          | **Alternative Input**               |
|-------------------------------------------------|--------------------------------------|-------------------------------------|
| `LoupeWindowCommands.ToggleGrid`                | <kbd>G</kbd>                         |                                     |
| `LoupeWindowCommands.IncreaseGridSize`          | <kbd>Shift</kbd> <kbd>OemPlus</kbd>  | <kbd>Shift</kbd> `Mouse Wheel Up`   |
| `LoupeWindowCommands.DecreaseGridSize`          | <kbd>Shift</kbd> <kbd>OemMinus</kbd> | <kbd>Shift</kbd> `Mouse Wheel Down` |
| `LoupeWindowCommands.IncreasePixelSize`         | <kbd>Ctrl</kbd> <kbd>OemPlus</kbd>   | <kbd>Ctrl</kbd> `Mouse Wheel Up`    |
| `LoupeWindowCommands.DecreasePixelSize`         | <kbd>Ctrl</kbd> <kbd>OemMinus</kbd>  | <kbd>Ctrl</kbd> `Mouse Wheel Down`  |
| `LoupeWindowCommands.IncreaseColorSamplerSize`  | <kbd>OemPlus</kbd>                   |                                     |
| `LoupeWindowCommands.DecreaseColorSamplerSize`  | <kbd>OemMinus</kbd>                  |                                     |
| `LoupeWindowCommands.Close`                     | <kbd>Enter</kbd>                     |                                     |

> [!NOTE]
> <kbd>OemPlus</kbd> and <kbd>OemMinus</kbd> are the <kbd>+</kbd> and <kbd>-</kbd> keys to the left of <kbd>Backspace</kbd>. However, they may vary on non-US keyboard layouts.

You can remap the keybinds for any of the listed commands before instantiating the window:
```csharp
LoupeWindow.ConfigureKeyBindings(bindings =>
{
    bindings.Close = new KeyBinding
    {
        Command = LoupeWindowCommands.Close,
        Key = Key.Space,
        Modifiers = ModifierKeys.None
    };
});

var picker = new LoupeWindow();
```

#### Customizing the Info Panel
You can customize how the color and screen position values are formatted in the info panel (visible when `ShowInfoPanel = true`) by providing your own `IValueConverter` implementations to the `PixelColorConverter` and `PixelPositionConverter` properties.

The example below formats the color value using CSS RGB syntax instead of the default HTML hex format:
```csharp
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

internal class ColorToCssStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not Color c)
            return string.Empty;

        return $"rgb({c.R}, {c.G}, {c.B})";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
         => throw new NotImplementedException();
}
```
```csharp
// Set the converter after instantiating the window
var picker = new LoupeWindow
{
    PixelColorConverter = new ColorToCssStringConverter(),
    InfoPanelMinWidth = 175
};
```

> [!TIP]
> The `InfoPanelMinWidth` property can be used to set the minimum width of the info panel. This is useful when the panel's content might exceed the default minimum width, which would otherwise cause the magnifier position to shift horizontally due to its centering.

License
-------
All code in this repository is available under the terms of the [MIT](LICENSE) license.
