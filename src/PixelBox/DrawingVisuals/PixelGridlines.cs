// Copyright 2026 Jad Altahan <xv.git@aol.com>
// SPDX-License-Identifier: MIT

using System.ComponentModel;
using System.Windows.Media;
using System.Windows;

namespace PixelBox.DrawingVisuals;

/// <summary>
/// Renders gridlines for use as a visual indicator of pixel boundaries.
/// </summary>
internal sealed class PixelGridlines : InverseScaledDrawingVisual
{
    #region Fields

    private static readonly Pen s_pen = new(Brushes.Black, 1);

    private int _gridSize;
    private int _pixelSize;

    #endregion

    static PixelGridlines()
    {
        s_pen.Freeze();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PixelGridlines"/> class.
    /// </summary>
    /// 
    /// <param name="dpi">
    /// DPI scale information.
    /// </param>
    public PixelGridlines(DpiScale dpi) : base(dpi)
    {
        VisualEdgeMode = EdgeMode.Aliased;
    }

    /// <summary>
    /// Sets the grid parameters.
    /// </summary>
    /// 
    /// <param name="gridSize">
    /// The number of pixels in the grid.
    /// </param>
    /// 
    /// <param name="pixelSize">
    /// The size of each pixel in the grid.
    /// </param>
    public void SetGrid(int gridSize, int pixelSize)
    {
        if (_gridSize == gridSize &&
            _pixelSize == pixelSize)
        {
            return;
        }

        _gridSize = gridSize;
        _pixelSize = pixelSize;
    }

    /// <summary>
    /// Renders the pixel gridlines.
    /// </summary>
    public void Render()
    {
        if (DesignerProperties.GetIsInDesignMode(this))
            return;

        using var dc = RenderOpen();

        double size = _gridSize * _pixelSize;

        for (var x = 1; x < _gridSize; x++)
        {
            double px = x * _pixelSize;

            dc.DrawLine(
                s_pen,
                new Point(0, px),
                new Point(size, px));
        }

        for (var y = 1; y < _gridSize; y++)
        {
            double py = y * _pixelSize;

            dc.DrawLine(
                s_pen,
                new Point(py, 0),
                new Point(py, size));
        }
    }
}