using Microsoft.Maui.Graphics;
using TabletopSpells.Helpers;
using TabletopSpells.ViewModels;

namespace TabletopSpells.Pages
{
    public partial class ThemeColorPickerPage : ContentPage
    {
        private readonly ColorWheelDrawable _colorWheel = new();

        public ThemeColorPickerPage(ThemeColorPickerViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
            ColorWheelView.Drawable = _colorWheel;
        }

        private void OnColorWheelStart(object sender, TouchEventArgs e)
        {
            UpdateColorFromTouch(e.Touches.FirstOrDefault());
        }

        private void OnColorWheelDrag(object sender, TouchEventArgs e)
        {
            UpdateColorFromTouch(e.Touches.FirstOrDefault());
        }

        private void UpdateColorFromTouch(PointF? touch)
        {
            if (touch == null || BindingContext is not ThemeColorPickerViewModel viewModel)
                return;

            var viewSize = ColorWheelView.Bounds;
            var center = new PointF((float)viewSize.Width / 2f, (float)viewSize.Height / 2f);
            var dx = touch.Value.X - center.X;
            var dy = touch.Value.Y - center.Y;
            var distance = MathF.Sqrt(dx * dx + dy * dy);
            var radius = MathF.Min(center.X, center.Y);

            if (distance > radius)
            {
                var scale = radius / distance;
                dx *= scale;
                dy *= scale;
                distance = radius;
            }

            var hue = MathF.Atan2(dy, dx);
            if (hue < 0)
            {
                hue += MathF.PI * 2f;
            }

            var saturation = MathF.Min(1f, distance / radius);
            var color = Color.FromHsla(hue / (MathF.PI * 2f), saturation, 0.5f);
            viewModel.SelectedColor = color;
        }

        private async void OnCancelClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        private async void OnApplyClicked(object sender, EventArgs e)
        {
            if (BindingContext is ThemeColorPickerViewModel viewModel)
            {
                viewModel.Apply();
            }

            await Navigation.PopAsync();
        }
    }
}
