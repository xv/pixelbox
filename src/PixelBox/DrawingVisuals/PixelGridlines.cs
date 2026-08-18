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
    private Size _pixelSize;

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
    public void SetGrid(int gridSize, Size pixelSize)
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

        var w = _gridSize * _pixelSize.Width;
        var h = _gridSize * _pixelSize.Height;

        for (var row = 1; row < _gridSize; row++)
        {
            var y = row * _pixelSize.Height;

            dc.DrawLine(s_pen,
                new Point(0, y),
                new Point(w, y));
        }

        for (var col = 1; col < _gridSize; col++)
        {
            var x = col * _pixelSize.Width;

            dc.DrawLine(s_pen,
                new Point(x, 0),
                new Point(x, h));
        }
    }
}