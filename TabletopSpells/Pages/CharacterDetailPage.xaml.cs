using Newtonsoft.Json;
using System.Collections.ObjectModel;

namespace TabletopSpells.Pages
{
    public partial class CharacterDetailPage : ContentPage
    {
        public ObservableCollection<string> Spells
        {
            get; private set;
        }
        private SharedViewModel ViewModel
        {
            get; set;
        }
        private string CharacterName
        {
            get; set;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            CreateList();
        }

        public CharacterDetailPage(string characterName, SharedViewModel viewModel)
        {
            InitializeComponent();
            CharacterName = characterName;
            this.Title = $"{CharacterName}'s spells";
            ViewModel = SharedViewModel.Instance;

            this.BindingContext = ViewModel;
            ViewModel.LoadSpellsForCharacter(characterName);

            if (ViewModel.CharacterSpells.ContainsKey(CharacterName))
            {
                CreateList();
            }
        }

        private void CreateList()
        {

            
            SpellListView.ItemsSource = ViewModel.CharacterSpells[CharacterName]
                    .OrderBy(spell => spell.SpellLevel)
                    .ThenBy(spell => spell.Name)
                    .ToList();
        }

        private async void OnDeleteCharacterClicked(object sender, EventArgs e)
        {
            // Display a confirmation dialog
            bool deleteConfirmed = await DisplayAlert(
                "Confirm Delete",
                $"Are you sure you want to delete {CharacterName}?",
                "Yes",
                "No"
            );

            // If the user confirmed, proceed with the deletion
            if (deleteConfirmed)
            {
                DeleteCharacter(CharacterName);

                // Optionally, navigate back to the main character list after deletion
                await Navigation.PopAsync();
            }
        }

        private async void DeleteCharacter(string characterName)
        {
            // Retrieve the current list of characters
            string existingCharactersJson = Preferences.Get("characters", "[]");
            var characters = JsonConvert.DeserializeObject<List<string>>(existingCharactersJson);

            // Remove the selected character
            if (characters.Remove(characterName))
            {
                // Save the updated list
                string updatedCharactersJson = JsonConvert.SerializeObject(characters);
                Preferences.Set("characters", updatedCharactersJson);

                // Remove the associated spell list for the character
                SharedViewModel.Instance.CharacterSpells.Remove(characterName);
                // Optionally, you can also remove the character's spells from Preferences
                Preferences.Remove($"spells_{characterName}");

                // Notify any subscribers about the change
                SharedViewModel.Instance.OnPropertyChanged(nameof(SharedViewModel.Instance.CharacterSpells));
                await DisplayAlert("Success", $"{characterName} has been removed.", "OK");
            }
            else
            {
                await DisplayAlert("Error", $"Character '{characterName}' not found.", "OK");
            }
        }


        [Obsolete]
        private void OnAddSpellClicked(object sender, EventArgs e)
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                await Navigation.PushAsync(new SpellListPage(CharacterName));
            });
        }

        private async void OnSpellSelected(object sender, SelectionChangedEventArgs e)
        {
            var selectedSpell = e.CurrentSelection.FirstOrDefault() as Spell;
            if (selectedSpell != null)
            {
                await Navigation.PushAsync(new SpellDetailPage(selectedSpell, CharacterName));
            }

            // Optionally, clear the selection
            ((CollectionView)sender).SelectedItem = null;
        }
    }
}
