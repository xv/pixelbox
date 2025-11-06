About
-----
PixelBox is a fast and customizable BitBlt-based pixel magnification control for WPF. The library contains the the standalone magnifier control itself, and an optional mouse-tracked pixel magnifier and color picker window, similar to what you would find in browser developer tools.

Features
--------
In a nutshell, what the control can provide is:
- Adjustable pixel columns (field of view), pixel size (zoom), and refresh rate.
- Toggleable grid lines.
- Color sampling ranging from single pixel up to a 5×5 region.
- High-DPI support.

Here's a quick demo of the built-in magnifier window:

![demo](https://github.com/xv/PixelBox.WPF/blob/master/media/demo.gif)

Requirements
------------
- **Operating System**: Windows 7 SP1 or later.
- **.NET**: [8.0](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) or later (uses C# 12 features).

API & Shortcuts Overview
------------
### PixelMagnifier (core control)

| **Property**      | **Type**            | **Description**                            |
|:------------------|:--------------------|:-------------------------------------------|
| `PixelColumns`    | `int`               | Number of pixel columns (odd values only). |
| `PixelSize`       | `int`               | Size of each pixel cell (in px).           |
| `ShowGrid`        | `bool`              | Toggles grid visibility.                   |
| `SamplingMode`    | `PixelSamplingMode` | `Single`,`ThreeByThree`, or `FiveByFive`   |
| `RefreshInterval` | `int`               | Image update interval in milliseconds.     |
| `PixelColor`      | `Color`             | Color of the sampled pixel(s).             |
| `PixelPosition`   | `Point`             | Screen coordinates of the center pixel.    |

| **Method**            | **Description**                                                           |
|:----------------------|:--------------------------------------------------------------------------|
| `StartCapture()`      | Begins capturing pixels by starting the internal timer.                   |
| `StopCapture()`       | Stops capturing pixels by stopping the internal  timer.                   |
| `ToggleCapture()`     | Toggles pixel capture. Same as calling  `StartCapture()`/`StopCapture()`. |
| `LockPosition(Point)` | Locks capturing at X and Y of `Point`.                                    |
| `UnlockPosition()`    | Unlocks previously locked coordinates via `LockPosition(Point)`.          |

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
> `PixelMagnifier` uses unmanaged resources and implements `IDisposable`. Be sure to clean up when you are done with the control, either by calling `Dispose()` or wrapping the instance in a `using` statement.

```xml
<Window x:Class="DemoApp.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:pb="clr-namespace:PixelBox;assembly=PixelBox">
    <Grid>
        <pb:PixelMagnifier
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
picker.ShowDialog();

var color = picker.SelectedPixelColor;
var position = picker.SelectedPixelPosition;
```

License
-------
All code in this repository is available under the terms of the [MIT](LICENSE) license.
