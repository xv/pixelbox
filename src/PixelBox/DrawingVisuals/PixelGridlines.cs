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
    private Size _cellSize;

    private bool _isDirty;

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
    /// The dimension of the square grid.
    /// </param>
    /// 
    /// <param name="cellSize">
    /// The size of each pixel cell in the grid.
    /// </param>
    public void SetGrid(int gridSize, Size cellSize)
    {
        if (_gridSize == gridSize &&
            _cellSize == cellSize)
        {
            return;
        }

        _gridSize = gridSize;
        _cellSize = cellSize;

        _isDirty = true;
    }

    /// <summary>
    /// Renders the pixel gridlines.
    /// </summary>
    public void Render()
    {
        if (!_isDirty || DesignerProperties.GetIsInDesignMode(this))
            return;

        using var dc = RenderOpen();

        var w = _gridSize * _cellSize.Width;
        var h = _gridSize * _cellSize.Height;

        for (var row = 1; row < _gridSize; row++)
        {
            var y = row * _cellSize.Height;

            dc.DrawLine(s_pen,
                new Point(0, y),
                new Point(w, y));
        }

        for (var col = 1; col < _gridSize; col++)
        {
            var x = col * _cellSize.Width;

            dc.DrawLine(s_pen,
                new Point(x, 0),
                new Point(x, h));
        }

        _isDirty = false;
    }
}