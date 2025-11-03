using System.ComponentModel;
using System.Windows.Media;
using System.Windows;

namespace PixelBox.Drawing;

/// <summary>
/// Renders two contrasting rectangle for use as sampling area visual indicators.
/// </summary>
internal sealed class SamplingAreaIndicator : DrawingVisual
{
    #region Fields

    private static readonly Pen s_penWhite = new(Brushes.White, 1);
    private static readonly Pen s_penBlack = new(Brushes.Black, 1);

    private readonly Rect[] _rects = new Rect[2];

    private DpiScale _dpi;
    private ScaleTransform? _scaleTrans;

    #endregion

    static SamplingAreaIndicator()
    {
        s_penWhite.Freeze();
        s_penBlack.Freeze();
    }

    /// <summary>
    /// Configures the internal <see cref="ScaleTransform"/> so that the visual
    /// is scaled properly regardless of the monitor's current DPI scaling.
    /// </summary>
    private void SetScaleTransform()
    {
        _scaleTrans ??= new ScaleTransform();
        _scaleTrans.ScaleX = 1.0 / _dpi.DpiScaleX;
        _scaleTrans.ScaleY = 1.0 / _dpi.DpiScaleY;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SamplingAreaIndicator"/>
    /// class.
    /// </summary>
    /// 
    /// /// <param name="dpi">
    /// DPI scale information.
    /// </param>
    public SamplingAreaIndicator(DpiScale dpi)
    {
        // Small antialiased rects look super ass and this is actually one of
        // the reasons why this class exists
        VisualEdgeMode = EdgeMode.Aliased;
        _dpi = dpi;

        if (_dpi.DpiScaleX != 1.0 || _dpi.DpiScaleY != 1.0)
            SetScaleTransform();
    }

    protected override void OnDpiChanged(DpiScale oldDpi, DpiScale newDpi)
    {
        base.OnDpiChanged(oldDpi, newDpi);

        _dpi = newDpi;
        SetScaleTransform();
    }

    /// <summary>
    /// Manually sets the DPI scale.
    /// </summary>
    /// 
    /// <param name="newDpi">
    /// The value to set.
    /// </param>
    public void SetDpi(DpiScale newDpi) => OnDpiChanged(_dpi, newDpi);

    /// <summary>
    /// Sets the area of the sampling indicator.
    /// </summary>
    /// 
    /// <param name="rect">
    /// The rectangle representing the sampling area.
    /// </param>
    public void SetArea(Rect rect)
    {
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
        var useScale = _scaleTrans is not null;

        if (useScale)
            dc.PushTransform(_scaleTrans);

        dc.DrawRectangle(null, s_penWhite, _rects[0]);
        dc.DrawRectangle(null, s_penBlack, _rects[1]);

        if (useScale)
            dc.Pop();
    }
}