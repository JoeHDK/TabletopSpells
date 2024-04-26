using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using TabletopSpells.Models;

namespace TabletopSpells.Pages
{
    public partial class CharacterOverviewPage : ContentPage
    {
        public ObservableCollection<string> Spells
        {
            get; private set;
        }

        private SharedViewModel ViewModel;
        private string CharacterName;

        protected override void OnAppearing()
        {
            base.OnAppearing();
        }

        public CharacterOverviewPage(string characterName, SharedViewModel viewModel)
        {
            InitializeComponent();
            CharacterName = characterName;
            this.Title = $"{CharacterName}'s spells";
            ViewModel = viewModel;  // Use the passed viewModel

            this.BindingContext = ViewModel;
            ViewModel.LoadSpellsForCharacter(characterName);
        }

        [Obsolete]
        private async void OnCharacterSelected(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new CharacterDetailPage(CharacterName, SharedViewModel.Instance));
        }

        [Obsolete]
        private void OnAddSpellClicked(object sender, EventArgs e)
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                await Navigation.PushAsync(new SpellListPage(CharacterName));
            });
        }

        [Obsolete]
        private async void OnDeleteCharacterClicked(object sender, EventArgs e)
        {
            bool deleteConfirmed = await DisplayAlert(
                "Confirm Delete",
                $"Are you sure you want to delete {CharacterName}?",
                "Yes",
                "No"
            );

            if (deleteConfirmed)
            {
                await DeleteCharacter(CharacterName);
                Device.BeginInvokeOnMainThread(async () =>
                {
                    await Navigation.PopAsync();
                });
            }
        }

        private async Task DeleteCharacter(string characterName)
        {
            try
            {
                string existingCharactersJson = Preferences.Get("characters", "[]");
                var characters = JsonConvert.DeserializeObject<List<Character>>(existingCharactersJson);

                var characterToRemove = characters.FirstOrDefault(c => c.Name == characterName);
                if (characterToRemove != null)
                {
                    characters.Remove(characterToRemove);
                    string updatedCharactersJson = JsonConvert.SerializeObject(characters);
                    Preferences.Set("characters", updatedCharactersJson);

                    ViewModel.CharacterSpells.Remove(characterName);
                    Preferences.Remove($"spells_{characterName}");

                    ViewModel.OnPropertyChanged(nameof(ViewModel.CharacterSpells));
                    await DisplayAlert("Success", $"{characterName} has been removed.", "OK");
                }
                else
                {
                    await DisplayAlert("Error", $"Character '{characterName}' not found.", "OK");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in DeleteCharacter: {ex.Message}");
                await DisplayAlert("Error", "An error occurred while deleting the character.", "OK");
            }
        }
    }
}
