// Copyright 2025 Jad Altahan <xv.git@aol.com>
// SPDX-License-Identifier: MIT

using System.Windows.Input;

namespace PixelBox;

/// <summary>
/// Provides UI commands associated with <see cref="LoupeWindow"/>.
/// </summary>
public static class LoupeWindowCommands
{
    public static readonly RoutedCommand ToggleGrid =
        new(nameof(ToggleGrid),
            typeof(LoupeWindow));

    public static readonly RoutedCommand IncreaseGridSize =
        new(nameof(IncreaseGridSize),
            typeof(LoupeWindow));

    public static readonly RoutedCommand DecreaseGridSize =
        new(nameof(DecreaseGridSize),
            typeof(LoupeWindow));

    public static readonly RoutedCommand IncreasePixelSize =
        new(nameof(IncreasePixelSize),
            typeof(LoupeWindow));

    public static readonly RoutedCommand DecreasePixelSize =
        new(nameof(DecreasePixelSize),
            typeof(LoupeWindow));

    public static readonly RoutedCommand IncreaseColorSamplerSize =
        new(nameof(IncreaseColorSamplerSize),
            typeof(LoupeWindow));

    public static readonly RoutedCommand DecreaseColorSamplerSize =
        new(nameof(DecreaseColorSamplerSize),
            typeof(LoupeWindow));

    public static readonly RoutedCommand Close =
        new(nameof(Close),
            typeof(LoupeWindow));
}