using System;
using System.Windows;
using System.Windows.Media;

public class Wave
{
    private double offsetIncreaseBy;
    public double amplitude;
    public double sc;
    private SolidColorBrush color;

    private double phase;
    private double offset;

    public Wave(double offsetIncreaseBy, double amplitude, double sc, SolidColorBrush color)
    {
        this.offsetIncreaseBy = offsetIncreaseBy;
        this.amplitude = amplitude;
        this.sc = sc;
        this.color = color;
    }

    public void Update()
    {
        phase += 0.05f;
        offset += offsetIncreaseBy;
    }

    public void Disable()
    {
        color = new SolidColorBrush(Color.FromArgb(20, color.Color.R, color.Color.G, color.Color.B));
    }

    public void Enable()
    {
        color = new SolidColorBrush(Color.FromArgb(255, color.Color.R, color.Color.G, color.Color.B));

    }
    
    public void DrawWave(DrawingContext drawingContext, double thickness, Size renderSize)
    {
            
        Pen graphPen = new Pen(color, thickness);
        double width = renderSize.Width;
        double height = renderSize.Height;
        double scale = 40;

        StreamGeometry geometry = new StreamGeometry();
        using (StreamGeometryContext ctx = geometry.Open())
        {
            bool started = false;
            for (double x = -10; x <= 10; x += 0.01)
            {
                double y = Sinc(x, amplitude, sc, offset);

                // Transform to screen coordinates
                double screenX = width / 2 + x * scale;
                double screenY = height / 2 - y * scale;

                if (!started)
                {
                    ctx.BeginFigure(new Point(screenX, screenY), false, false);
                    started = true;
                }
                else
                {
                    ctx.LineTo(new Point(screenX, screenY), true, false);
                }
            }
        }

        drawingContext.DrawGeometry(null, graphPen, geometry);
    }
    private double Sinc(double x, double amplitude, double scale, double offset)
    {
        if (x == 0) return 1;
        return Math.Sin(x * amplitude) / (x*scale) + (Math.Sin(x + offset)/20);
    }
}