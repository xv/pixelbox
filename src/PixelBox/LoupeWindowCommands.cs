// Copyright 2025 Jad Altahan <xv.git@aol.com>
// SPDX-License-Identifier: MIT

using System.Windows.Input;

namespace PixelBox;

/// <summary>
/// Provides UI commands associated with <see cref="LoupeWindow"/>.
/// </summary>
public static class LoupeWindowCommands
{
    /// <summary>
    /// Command to toggle the visibility of the grid overlay in
    /// <see cref="LoupeWindow"/>.
    /// </summary>
    public static readonly RoutedCommand ToggleGrid =
        new(nameof(ToggleGrid),
            typeof(LoupeWindow));

    /// <summary>
    /// Command to increase the grid size in <see cref="LoupeWindow"/>.
    /// </summary>
    public static readonly RoutedCommand IncreaseGridSize =
        new(nameof(IncreaseGridSize),
            typeof(LoupeWindow));

    /// <summary>
    /// Command to decrease the grid size in <see cref="LoupeWindow"/>.
    /// </summary>
    public static readonly RoutedCommand DecreaseGridSize =
        new(nameof(DecreaseGridSize),
            typeof(LoupeWindow));

    /// <summary>
    /// Command to increase the pixel size in <see cref="LoupeWindow"/>.
    /// </summary>
    public static readonly RoutedCommand IncreasePixelSize =
        new(nameof(IncreasePixelSize),
            typeof(LoupeWindow));

    /// <summary>
    /// Command to decrease the pixel size in <see cref="LoupeWindow"/>.
    /// </summary>
    public static readonly RoutedCommand DecreasePixelSize =
        new(nameof(DecreasePixelSize),
            typeof(LoupeWindow));

    /// <summary>
    /// Command to increase the sampler size in <see cref="LoupeWindow"/>.
    /// </summary>
    public static readonly RoutedCommand IncreaseColorSamplerSize =
        new(nameof(IncreaseColorSamplerSize),
            typeof(LoupeWindow));

    /// <summary>
    /// Command to decrease the sampler size in <see cref="LoupeWindow"/>.
    /// </summary>
    public static readonly RoutedCommand DecreaseColorSamplerSize =
        new(nameof(DecreaseColorSamplerSize),
            typeof(LoupeWindow));

    /// <summary>
    /// Command to confirm the pixel selection at the current position and close
    /// the <see cref="LoupeWindow"/>.
    /// </summary>
    ///
    /// <remarks>
    /// The <c>DialogResult</c> property of <see cref="LoupeWindow"/> should be
    /// set to <see langword="true"/> when this command is executed.
    /// </remarks>
    public static readonly RoutedCommand Confirm =
        new(nameof(Confirm),
            typeof(LoupeWindow));
}