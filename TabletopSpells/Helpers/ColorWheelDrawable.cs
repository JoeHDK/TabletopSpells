using Microsoft.Maui.Graphics;

namespace TabletopSpells.Helpers
{
    public class ColorWheelDrawable : IDrawable
    {
        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            var center = new PointF(dirtyRect.Center.X, dirtyRect.Center.Y);
            var radius = Math.Min(dirtyRect.Width, dirtyRect.Height) / 2f;
            var stepAngle = 4f;
            var stepRadius = 4f;
            var dotSize = stepRadius * 0.75f;

            canvas.Antialias = true;

            for (var r = 0f; r <= radius; r += stepRadius)
            {
                var saturation = radius <= 0 ? 0f : r / radius;
                for (var angle = 0f; angle < 360f; angle += stepAngle)
                {
                    var radians = MathF.PI * angle / 180f;
                    var x = center.X + r * MathF.Cos(radians);
                    var y = center.Y + r * MathF.Sin(radians);
                    var hue = angle / 360f;

                    canvas.FillColor = Color.FromHsla(hue, saturation, 0.5f);
                    canvas.FillCircle(x, y, dotSize);
                }
            }
        }
    }
}
