using TabletopSpells.ViewModels;

namespace TabletopSpells.Pages
{
    public partial class ThemeColorPickerPage : ContentPage
    {
        public ThemeColorPickerPage(ThemeColorPickerViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
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
