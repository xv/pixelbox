using System.Windows.Media;
using System.Windows;

namespace PixelBox;

/// <summary>
/// Provides data for the <see cref="PixelMagnifier.PixelChanged"/> event.
/// </summary>
/// 
/// <param name="color">
/// The color of the new pixel.
/// </param>
/// 
/// <param name="screenPosition">
/// The screen coordinate of the new pixel.
/// </param>
public class PixelChangedEventArgs(Color color, Point screenPosition) : EventArgs
{
    /// <summary>
    /// Gets the color of the pixel at the center of the grid.
    /// </summary>
    public Color Color => color;

    /// <summary>
    /// Gets the coordinates of the pixel at the center of the grid.
    /// </summary>
    public Point ScreenPosition => screenPosition;
}