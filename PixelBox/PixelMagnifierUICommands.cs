using System.Windows.Input;

namespace PixelBox;

internal class PixelMagnifierUICommands
{
    public static readonly RoutedUICommand ToggleGrid =
        new("Toggle Grid",
            nameof(ToggleGrid),
            typeof(PixelMagnifierWindow));

    public static readonly RoutedUICommand ExpandView =
        new("Expand View",
            nameof(ExpandView),
            typeof(PixelMagnifierWindow));

    public static readonly RoutedUICommand ShrinkView =
        new("Shrink View",
            nameof(ShrinkView),
            typeof(PixelMagnifierWindow));

    public static readonly RoutedUICommand ZoomIn =
        new("Zoom In",
            nameof(ZoomIn),
            typeof(PixelMagnifierWindow));

    public static readonly RoutedUICommand ZoomOut =
        new("Zoom Out",
            nameof(ZoomOut),
            typeof(PixelMagnifierWindow));

    public static readonly RoutedUICommand IncreaseColorSamplingArea =
        new("Increase Color Sampling Area",
            nameof(IncreaseColorSamplingArea),
            typeof(PixelMagnifierWindow));

    public static readonly RoutedUICommand DecreaseColorSamplingArea =
        new("Decrease Color Sampling Area",
            nameof(DecreaseColorSamplingArea),
            typeof(PixelMagnifierWindow));

    public static readonly RoutedUICommand Close =
        new("Close",
            nameof(Close),
            typeof(PixelMagnifierWindow));
}