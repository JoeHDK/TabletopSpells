using System.Diagnostics;
using TabletopSpells.Helpers;
using TabletopSpells.Models;
using TabletopSpells.Services;
using TabletopSpells.Themes;
using TabletopSpells.ViewModels;

namespace TabletopSpells.Pages
{
    public partial class CharacterOptionsPage : ContentPage
    {
        private readonly Character character;
        private readonly SharedViewModel viewModel;

        public CharacterOptionsPage(Character character, SharedViewModel viewModel)
        {
            InitializeComponent();
            this.character = character;
            this.viewModel = viewModel;
            
            Title = $"{character.Name} - Options";
            UpdateThemeToggleText();
        }

        private void OnToggleThemeClicked(object sender, EventArgs e)
        {
            ThemeService.ToggleTheme();
            UpdateThemeToggleText();
            
            // Show confirmation
            var currentTheme = AppColors.Current.Name;
            DisplayAlert("Theme Changed", $"{currentTheme} theme is now active", "OK");
        }

        private void UpdateThemeToggleText()
        {
            var currentTheme = AppColors.Current.Name;
            var nextTheme = currentTheme == "Dark" ? "Light" : "Dark";
            ThemeToggleButton.Text = $"Switch to {nextTheme} Mode";
        }

        private async void OnDeleteCharacterClicked(object sender, EventArgs e)
        {
            bool deleteConfirmed = await DisplayAlert(
                "Confirm Delete",
                $"Are you sure you want to delete {character.Name}? This action cannot be undone.",
                "Yes, Delete",
                "Cancel"
            );

            if (!deleteConfirmed) return;

            // Second confirmation for safety
            bool doubleConfirm = await DisplayAlert(
                "Final Confirmation",
                $"This will permanently delete {character.Name} and all associated data. Are you absolutely sure?",
                "Yes, I'm Sure",
                "No, Keep Character"
            );

            if (!doubleConfirm) return;

            await DeleteCharacter();
        }

        private async Task DeleteCharacter()
        {
            try
            {
                var characters = LocalStorageHelper.LoadCharactersFromFile();
                var characterToRemove = characters.FirstOrDefault(c => c.ID == character.ID);
                
                if (characterToRemove != null)
                {
                    characters.Remove(characterToRemove);
                    LocalStorageHelper.SaveCharactersToFile(characters);

                    viewModel.CharacterSpells.Remove(character.ID);
                    LocalStorageHelper.DeleteCharacterFolder(character.ID.Value);

                    viewModel.OnPropertyChanged(nameof(viewModel.CharacterSpells));
                    
                    await DisplayAlert("Success", $"{character.Name} has been deleted.", "OK");
                    
                    // Navigate back to character select
                    await Navigation.PopToRootAsync();
                }
                else
                {
                    await DisplayAlert("Error", $"Character '{character.Name}' not found.", "OK");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in DeleteCharacter: {ex.Message}");
                await DisplayAlert("Error", "An error occurred while deleting the character.", "OK");
            }
        }

        private async void OnViewAppThemeSettingsClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ThemeSelectionPage());
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            UpdateThemeToggleText();
        }
    }
}

