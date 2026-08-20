// Copyright 2025 Jad Altahan <xv.git@aol.com>
// SPDX-License-Identifier: MIT

using System.ComponentModel;
using System.Windows.Media;
using System.Windows;

namespace PixelBox.DrawingVisuals;

/// <summary>
/// Renders two contrasting rectangles for use as sampling area visual indicators.
/// </summary>
internal sealed class SamplingAreaIndicator : InverseScaledDrawingVisual
{
    #region Fields

    private static readonly Pen s_penWhite = new(Brushes.White, 1);
    private static readonly Pen s_penBlack = new(Brushes.Black, 1);

    private readonly Rect[] _rects = new Rect[2];

    #endregion

    static SamplingAreaIndicator()
    {
        s_penWhite.Freeze();
        s_penBlack.Freeze();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SamplingAreaIndicator"/>
    /// class.
    /// </summary>
    /// 
    /// <param name="dpi">
    /// DPI scale information.
    /// </param>
    public SamplingAreaIndicator(DpiScale dpi) : base(dpi)
    {
        // Small antialiased rects look super ass and this is actually one of
        // the reasons why this class exists
        VisualEdgeMode = EdgeMode.Aliased;
    }

    /// <summary>
    /// Sets the sampling indicator area.
    /// </summary>
    /// 
    /// <param name="gridSize">
    /// The size of the square grid.
    /// </param>
    /// 
    /// <param name="cellSize">
    /// The size of each pixel cell in the grid.
    /// </param>
    /// 
    /// <param name="kernelSize">
    /// The size of the square sampling kernel.
    /// </param>
    /// 
    /// <param name="gridDrawn"> 
    /// Indicates whether gridlines are currently drawn.
    /// </param>
    public void SetArea(int gridSize, Size cellSize, int kernelSize, bool gridDrawn)
    {
        var offset = gridDrawn ? 0 : 1;

        var center = new Point(
            cellSize.Width * (gridSize / 2),
            cellSize.Height * (gridSize / 2));

        var rect = new Rect(
            center.X + offset,
            center.Y + offset,
            cellSize.Width - offset,
            cellSize.Height - offset);

        if (kernelSize > 1)
        {
            var radius = kernelSize / 2;

            rect.Inflate(
                cellSize.Width * radius,
                cellSize.Height * radius);
        }

        // Exaggerate the indicator size slightly for better visibility
        rect.Inflate(2, 2);

        _rects[0] = rect;

        // The second rectangle should be drawn in a different pen color
        //
        // The idea here is creating contrast with the background the two
        // rectangles are on so that at least one of the rectangles is
        // always visible on screen
        _rects[1] = rect;
        _rects[1].Inflate(-1, -1);
    }

    /// <summary>
    /// Renders the sampling area indicator.
    /// </summary>
    public void Render()
    {
        if (DesignerProperties.GetIsInDesignMode(this))
            return;

        using var dc = RenderOpen();

        dc.DrawRectangle(null, s_penBlack, _rects[0]);
        dc.DrawRectangle(null, s_penWhite, _rects[1]);
    }
}