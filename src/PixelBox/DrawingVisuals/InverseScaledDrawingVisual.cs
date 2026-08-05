// Copyright 2026 Jad Altahan <xv.git@aol.com>
// SPDX-License-Identifier: MIT

using System.Windows.Media;
using System.Windows;

namespace PixelBox.DrawingVisuals;

/// <summary>
/// Provides a <see cref="DrawingVisual"/> that applies an inverse
/// <see cref="ScaleTransform"/> to counteract monitor DPI scaling for
/// pixel-accurate rendering.
/// </summary>
internal abstract class InverseScaledDrawingVisual : DrawingVisual
{
    private DpiScale _dpi;
    private ScaleTransform? _scaleTransform;

    protected InverseScaledDrawingVisual(DpiScale dpi)
    {
        _dpi = dpi;

        if (_dpi.DpiScaleX != 1.0 || _dpi.DpiScaleY != 1.0)
            UpdateScaleTransform();
    }

    /// <summary>
    /// Gets the current DPI scale information.
    /// </summary>
    protected DpiScale Dpi => _dpi;

    /// <summary>
    /// Updates the scaling transform applied to this visual.
    /// </summary>
    private void UpdateScaleTransform()
    {
        _scaleTransform ??= new ScaleTransform();

        _scaleTransform.ScaleX = 1.0 / _dpi.DpiScaleX;
        _scaleTransform.ScaleY = 1.0 / _dpi.DpiScaleY;

        VisualTransform = _scaleTransform;
    }

    protected override void OnDpiChanged(DpiScale oldDpi, DpiScale newDpi)
    {
        base.OnDpiChanged(oldDpi, newDpi);

        _dpi = newDpi;
        UpdateScaleTransform();
    }

    /// <summary>
    /// Manually sets the DPI scale.
    /// </summary>
    /// 
    /// <param name="dpi">
    /// The value to set.
    /// </param>
    public void SetDpi(DpiScale dpi)
    {
        if (_dpi.Equals(dpi))
            return;

        OnDpiChanged(_dpi, dpi);
    }
}