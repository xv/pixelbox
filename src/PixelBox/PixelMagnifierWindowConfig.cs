// Copyright 2025 Jad Altahan <xv.git@aol.com>
// SPDX-License-Identifier: MIT

namespace PixelBox;

/// <summary>
/// Represents various configuration parameters for
/// <see cref="PixelMagnifierWindow"/>.
/// </summary>
public class PixelMagnifierWindowConfig
{
    public const int
        PixelColumnsMin = 15,
        PixelColumnsMax = 25;

    public const int
        PixelSizeMin = 7,
        PixelSizeMax = 15;

    public const int
        RefreshIntervalMin = 10,
        RefreshIntervalMax = 100;

    /// <summary>
    /// <inheritdoc cref="PixelMagnifier.PixelColumns"/>
    /// </summary>
    public int PixelColumns
    { get; set; } = PixelColumnsMin;

    /// <summary>
    /// <inheritdoc cref="PixelMagnifier.PixelSize"/>
    /// </summary>
    public int PixelSize
    { get; set; } = PixelSizeMin;

    /// <summary>
    /// <inheritdoc cref="PixelMagnifier.SamplingMode"/>
    /// </summary>
    public PixelSamplingMode SamplingMode
    { get; set; } = PixelSamplingMode.Single;

    /// <summary>
    /// <inheritdoc cref="PixelMagnifier.ShowGrid"/>
    /// </summary>
    public bool ShowGrid
    { get; set; } = true;

    /// <summary>
    /// <inheritdoc cref="PixelMagnifierWindow.ShowInfoPanel"/>
    /// </summary>
    public bool ShowInfoPanel
    { get; set; } = true;

    /// <summary>
    /// <inheritdoc cref="PixelMagnifier.RefreshInterval"/>
    /// </summary>
    public int RefreshInterval
    { get; set; } = 30;

    /// <summary>
    /// Gets or sets whether the color string should be copied to the clipboard
    /// when the window is closed.
    /// </summary>
    public bool CopyColorToClipboard
    { get; set; } = true;
}