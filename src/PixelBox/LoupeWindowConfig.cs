// Copyright 2025 Jad Altahan <xv.git@aol.com>
// SPDX-License-Identifier: MIT

namespace PixelBox;

/// <summary>
/// Represents various configuration parameters for
/// <see cref="LoupeWindow"/>.
/// </summary>
public class LoupeWindowConfig
{
    public const int
        GridSizeMin = 15,
        GridSizeMax = 35;

    public const int
        PixelSizeMin = 7,
        PixelSizeMax = 15;

    public const int
        RefreshIntervalMin = 10,
        RefreshIntervalMax = 100;

    /// <summary>
    /// <inheritdoc cref="Loupe.GridSize"/>
    /// </summary>
    public int GridSize
    { get; set; } = GridSizeMin;

    /// <summary>
    /// <inheritdoc cref="Loupe.PixelSize"/>
    /// </summary>
    public int PixelSize
    { get; set; } = PixelSizeMin;

    /// <summary>
    /// <inheritdoc cref="Loupe.SamplingMode"/>
    /// </summary>
    public PixelSamplingMode SamplingMode
    { get; set; } = PixelSamplingMode.Single;

    /// <summary>
    /// <inheritdoc cref="Loupe.ShowGrid"/>
    /// </summary>
    public bool ShowGrid
    { get; set; } = true;

    /// <summary>
    /// <inheritdoc cref="LoupeWindow.ShowInfoPanel"/>
    /// </summary>
    public bool ShowInfoPanel
    { get; set; } = true;

    /// <summary>
    /// <inheritdoc cref="Loupe.RefreshInterval"/>
    /// </summary>
    public int RefreshInterval
    { get; set; } = 30;
}