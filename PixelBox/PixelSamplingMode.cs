namespace PixelBox;

/// <summary>
/// Pixel color sampling mode.
/// </summary>
public enum PixelSamplingMode
{
    /// <summary>
    /// The exact color of a single pixel.
    /// </summary>
    Single,
    /// <summary>
    /// The average color of 9 pixels in a 3x3 kernel.
    /// </summary>
    ThreeByThree,
    /// <summary>
    /// The average color of 25 pixels in a 5x5 kernel.
    /// </summary>
    FiveByFive
}