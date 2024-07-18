using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Diagnostics;
using TabletopSpells.Models;
using TabletopSpells.Models.Enums;

namespace TabletopSpells.Pages
{
    public partial class CharacterOverviewPage : ContentPage
    {
        public Character character;
        public Game gameType;
        public ObservableCollection<string> Spells
        {
            get; private set;
        }

        private SharedViewModel ViewModel;

        protected override void OnAppearing()
        {
            base.OnAppearing();
        }

        public CharacterOverviewPage(Character character, SharedViewModel viewModel, Game gameType)
        {
            InitializeComponent();
            this.character = character;
            this.Title = $"{character.Name}'s home";
            ViewModel = viewModel;  // Use the passed viewModel

            this.BindingContext = ViewModel;
            ViewModel.LoadSpellsForCharacter(character);

            this.gameType = gameType;
        }

        [Obsolete]
        private async void OnCharacterSelected(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new CharacterDetailPage(character, SharedViewModel.Instance, gameType));
        }

        private async void OnSpellPerDaySelected(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new SpellsPerDayPage());
        }

        private async void OnSpellLogSelected(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new SpellLogPage(character));
        }

        [Obsolete]
        private async void OnDeleteCharacterClicked(object sender, EventArgs e)
        {
            bool deleteConfirmed = await DisplayAlert(
                "Confirm Delete",
                $"Are you sure you want to delete {character.Name}?",
                "Yes",
                "No"
            );

            if (deleteConfirmed)
            {
                await DeleteCharacter(character);
                Device.BeginInvokeOnMainThread(async () =>
                {
                    await Navigation.PopAsync();
                });
            }
        }

        private async Task DeleteCharacter(Character character)
        {
            try
            {
                string existingCharactersJson = Preferences.Get("characters", "[]");
                var characters = JsonConvert.DeserializeObject<List<Character>>(existingCharactersJson);

                var characterToRemove = characters.FirstOrDefault(c => c.ID == character.ID);
                if (characterToRemove != null)
                {
                    characters.Remove(characterToRemove);
                    string updatedCharactersJson = JsonConvert.SerializeObject(characters);
                    Preferences.Set("characters", updatedCharactersJson);

                    ViewModel.CharacterSpells.Remove(character.Name);
                    Preferences.Remove($"spells_{character.Name}");
                    
                    ViewModel.OnPropertyChanged(nameof(ViewModel.CharacterSpells));
                    await DisplayAlert("Success", $"{character.Name} has been removed.", "OK");
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
    }
}
