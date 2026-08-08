// Copyright 2025 Jad Altahan <xv.git@aol.com>
// SPDX-License-Identifier: MIT

using System.Windows.Media;
using System.Windows;

namespace PixelBox;

/// <summary>
/// Provides data for the <see cref="Loupe.PixelChanged"/> event.
/// </summary>
/// 
/// <param name="color">
/// The color of the sampled pixel.
/// </param>
/// 
/// <param name="screenPosition">
/// The screen coordinates of the sampled pixel. 
/// </param>
public class PixelChangedEventArgs(Color color, Point screenPosition) : EventArgs
{
    /// <summary>
    /// Gets the color of the pixel at the center of the sampling region.
    ///
    /// <para>
    /// If <see cref="Loupe.SamplingMode"/> is not
    /// <see cref="PixelSamplingMode.Single"/>, this returns the average color
    /// of the pixels in the sampling region.
    /// </para>
    /// </summary>
    public Color Color => color;

    /// <summary>
    /// Gets the screen coordinates of the pixel at the center of the sampling
    /// region.
    /// </summary>
    public Point ScreenPosition => screenPosition;
}