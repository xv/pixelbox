// Copyright 2025 Jad Altahan <xv.git@aol.com>
// SPDX-License-Identifier: MIT

using System.Windows.Input;

namespace PixelBox;

/// <summary>
/// Provides UI commands associated with <see cref="LoupeWindow"/>.
/// </summary>
public static class LoupeWindowCommands
{
    public static readonly RoutedUICommand ToggleGrid =
        new("Toggle Grid",
            nameof(ToggleGrid),
            typeof(LoupeWindow));

    public static readonly RoutedUICommand IncreaseGridSize =
        new("Increase Grid Size",
            nameof(IncreaseGridSize),
            typeof(LoupeWindow));

    public static readonly RoutedUICommand DecreaseGridSize =
        new("Decrease Grid Size",
            nameof(DecreaseGridSize),
            typeof(LoupeWindow));

    public static readonly RoutedUICommand IncreasePixelSize =
        new("Increase Pixel Size",
            nameof(IncreasePixelSize),
            typeof(LoupeWindow));

    public static readonly RoutedUICommand DecreasePixelSize =
        new("Decrease Pixel Size",
            nameof(DecreasePixelSize),
            typeof(LoupeWindow));

    public static readonly RoutedUICommand IncreaseColorSamplerSize =
        new("Increase Color Sampler Size",
            nameof(IncreaseColorSamplerSize),
            typeof(LoupeWindow));

    public static readonly RoutedUICommand DecreaseColorSamplerSize =
        new("Decrease Color Sampler Size",
            nameof(DecreaseColorSamplerSize),
            typeof(LoupeWindow));

    public static readonly RoutedUICommand Close =
        new("Close",
            nameof(Close),
            typeof(LoupeWindow));
}