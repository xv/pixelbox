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
        RefreshIntervalMin = 1,
        RefreshIntervalMax = 100;

    /// <summary>
    /// <inheritdoc cref="Loupe.GridSize"/>
    ///
    /// <para>
    /// Default is <see cref="GridSizeMin"/>.
    /// </para>
    /// </summary>
    public int GridSize
    { get; set; } = 17;

    /// <summary>
    /// <inheritdoc cref="Loupe.PixelSize"/>
    ///
    /// <para>
    /// Default is <see cref="PixelSizeMin"/>.
    /// </para>
    /// </summary>
    public int PixelSize
    { get; set; } = 8;

    /// <summary>
    /// <inheritdoc cref="Loupe.SamplingMode"/>
    ///
    /// <para>
    /// Default is <see cref="PixelSamplingMode.Single"/>.
    /// </para>
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
    ///
    /// <para>
    /// Default is <see langword="true"/>.
    /// </para>
    /// </summary>
    public bool ShowInfoPanel
    { get; set; } = true;

    /// <summary>
    /// <inheritdoc cref="Loupe.RefreshInterval"/>
    ///
    /// <para>
    /// Default is 30 milliseconds.
    /// </para>
    /// </summary>
    public int RefreshInterval
    { get; set; } = 30;
}