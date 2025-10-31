
namespace PixelBox;

/// <summary>
/// Represents various configuration parameters for
/// <see cref="PixelMagnifierWindow"/>.
/// </summary>
public class PixelMagnifierWindowConfig
{
    public const int
        PixelColumnsMin = 11,
        PixelColumnsMax = 25;

    public const int
        PixelSizeMin = 10,
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
}