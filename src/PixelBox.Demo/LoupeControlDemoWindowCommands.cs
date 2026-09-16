using System.Windows.Input;

namespace PixelBox.Demo;

public static class LoupeControlDemoWindowCommands
{
    public static readonly RoutedCommand ToggleGrid =
        new(nameof(ToggleGrid),
            typeof(LoupeControlDemoWindowCommands));

    public static readonly RoutedCommand ToggleCapture =
        new(nameof(ToggleCapture),
            typeof(LoupeControlDemoWindowCommands));

    public static readonly RoutedCommand ToggleContinuousCapture =
        new(nameof(ToggleContinuousCapture),
            typeof(LoupeControlDemoWindowCommands));

    public static readonly RoutedCommand ToggleLockPosition =
        new(nameof(ToggleLockPosition),
            typeof(LoupeControlDemoWindowCommands));

    public static readonly RoutedCommand ToggleLockX =
        new(nameof(ToggleLockX),
            typeof(LoupeControlDemoWindowCommands));

    public static readonly RoutedCommand ToggleLockY =
        new(nameof(ToggleLockY),
            typeof(LoupeControlDemoWindowCommands));

    public static readonly RoutedCommand SetSamplerSizeSingle =
        new(nameof(SetSamplerSizeSingle),
            typeof(LoupeControlDemoWindowCommands));

    public static readonly RoutedCommand SetSamplerSize3x3 =
        new(nameof(SetSamplerSize3x3),
            typeof(LoupeControlDemoWindowCommands));

    public static readonly RoutedCommand SetSamplerSize5x5 =
        new(nameof(SetSamplerSize5x5),
            typeof(LoupeControlDemoWindowCommands));

    static LoupeControlDemoWindowCommands()
    {
        ToggleCapture.InputGestures.Add(new KeyGesture(Key.Space, ModifierKeys.None));
        ToggleGrid.InputGestures.Add(new KeyGesture(Key.G, ModifierKeys.Control));
        SetSamplerSizeSingle.InputGestures.Add(new KeyGesture(Key.D1, ModifierKeys.Control));
        SetSamplerSize3x3.InputGestures.Add(new KeyGesture(Key.D3, ModifierKeys.Control));
        SetSamplerSize5x5.InputGestures.Add(new KeyGesture(Key.D5, ModifierKeys.Control));
        ToggleLockPosition.InputGestures.Add(new KeyGesture(Key.L, ModifierKeys.Control));
        ToggleLockX.InputGestures.Add(new KeyGesture(Key.X, ModifierKeys.Control));
        ToggleLockY.InputGestures.Add(new KeyGesture(Key.Y, ModifierKeys.Control));
    }
}