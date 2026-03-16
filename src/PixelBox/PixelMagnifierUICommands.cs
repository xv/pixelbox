// Copyright 2025 Jad Altahan <xv.git@aol.com>
// SPDX-License-Identifier: MIT

using System.Windows.Input;

namespace PixelBox;

/// <summary>
/// Provides UI commands associated with <see cref="PixelMagnifier"/> and
/// <see cref="PixelMagnifierWindow"/>.
/// </summary>
public class PixelMagnifierUICommands
{
    public static readonly RoutedUICommand ToggleGrid =
        new("Toggle Grid",
            nameof(ToggleGrid),
            typeof(PixelMagnifierWindow));

    public static readonly RoutedUICommand IncreaseGridSize =
        new("Increase Grid Size",
            nameof(IncreaseGridSize),
            typeof(PixelMagnifierWindow));

    public static readonly RoutedUICommand DecreaseGridSize =
        new("Decrease Grid Size",
            nameof(DecreaseGridSize),
            typeof(PixelMagnifierWindow));

    public static readonly RoutedUICommand IncreasePixelSize =
        new("Increase Pixel Size",
            nameof(IncreasePixelSize),
            typeof(PixelMagnifierWindow));

    public static readonly RoutedUICommand DecreasePixelSize =
        new("Decrease Pixel Size",
            nameof(DecreasePixelSize),
            typeof(PixelMagnifierWindow));

    public static readonly RoutedUICommand IncreaseColorSamplerSize =
        new("Increase Color Sampler Size",
            nameof(IncreaseColorSamplerSize),
            typeof(PixelMagnifierWindow));

    public static readonly RoutedUICommand DecreaseColorSamplerSize =
        new("Decrease Color Sampler Size",
            nameof(DecreaseColorSamplerSize),
            typeof(PixelMagnifierWindow));

    public static readonly RoutedUICommand Close =
        new("Close",
            nameof(Close),
            typeof(PixelMagnifierWindow));
}