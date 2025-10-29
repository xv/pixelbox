using System.ComponentModel;
using System.Windows.Media;
using System.Windows;

namespace PixelBox.Drawing;

/// <summary>
/// 
/// </summary>
internal sealed class SamplingAreaIndicator : DrawingVisual
{
    private static readonly Pen s_penWhite = new(Brushes.White, 1);
    private static readonly Pen s_penBlack = new(Brushes.Black, 1);

    private readonly Rect[] _rects = new Rect[2];

    static SamplingAreaIndicator()
    {
        s_penWhite.Freeze();
        s_penBlack.Freeze();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SamplingAreaIndicator"/>
    /// class.
    /// </summary>
    public SamplingAreaIndicator()
    {
        // Small antialiased rects look super ass and this is one of the reasons
        // why this class exists
        VisualEdgeMode = EdgeMode.Aliased;
    }

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

    public void Render()
    {
        if (DesignerProperties.GetIsInDesignMode(this))
            return;

        using var dc = RenderOpen();

        dc.DrawRectangle(null, s_penWhite, _rects[0]);
        dc.DrawRectangle(null, s_penBlack, _rects[1]);
    }
}