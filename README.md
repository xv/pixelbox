About
-----
PixelBox is a fast and customizable BitBlt-based pixel magnification control for WPF. The library contains the standalone magnifier control itself, and an optional mouse-tracked pixel magnifier and color picker window, similar to what you would find in browser developer tools.

Features
--------
In a nutshell, what the control can provide is:
- Adjustable pixel columns (field of view), pixel size (zoom), and refresh rate.
- Toggleable grid lines.
- Color sampling ranging from single pixel up to a 5×5 region.
- Individual screen axes locking, like in macOS' Digital Color Meter.
- High-DPI support.

Here's a quick demo of the built-in magnifier window:

![demo](https://github.com/xv/PixelBox.WPF/blob/master/media/demo.gif)

Requirements
------------
- **Operating System**: Windows 7 SP1 or later.
- **.NET**: [8.0](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) or later (uses C# 12 features).

Installation
------------
You can either manually download a release from this repository, or via NuGet:
```console
dotnet add package pixelbox
```

Quick Start
-----------
### Using the Control Directly

> [!CAUTION]
> `PixelMagnifier` uses unmanaged resources and implements `IDisposable`. Call `Dispose()` when the control is no longer needed to properly release resources it holds.

```xml
<Window x:Class="DemoApp.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:pb="clr-namespace:PixelBox;assembly=PixelBox">
    <Grid>
        <!-- The control size of PixelMagnifier is automatically determiend via its
             PixelColumns, PixelSize, and ShowGrid properties. The size should not
             be set manually. -->
        <pb:PixelMagnifier HorizontalAlignment="Left" VerticalAlignment="Top"
            PixelColumns="15"
            PixelSize="10"
            ShowGrid="True"
            RefreshInterval="30"/>
    </Grid>
</Window>
```

### Using the Built-In Window

> [!TIP]
> A project demonstrating the use of the built-in window is provided under `PixelBox.Demo`.

```c#
using PixelBox;

var picker = new PixelMagnifierWindow();

// ShowDialog() will return true if the window was closed via either Enter key or
// mouse left click. You can use the return result to update your UI conditionally
picker.ShowDialog();

var color = picker.SelectedPixelColor;
var position = picker.SelectedPixelPosition;
```

License
-------
All code in this repository is available under the terms of the [MIT](LICENSE) license.
