using Newtonsoft.Json;
using System.Collections.ObjectModel;

namespace TabletopSpells.Pages
{
    public partial class CharacterDetailPage : ContentPage
    {
        public ObservableCollection<string> Spells { get; private set; }
        private SharedViewModel ViewModel { get; set; }
        private string CharacterName { get; set; }

        [Obsolete]
        public CharacterDetailPage(string characterName, SharedViewModel viewModel)
        {
            InitializeComponent();
            CharacterName = characterName;
            this.Title = $"{CharacterName}'s Details";
            ViewModel = SharedViewModel.Instance;
            //Spells = new ObservableCollection<string>();
            this.BindingContext = ViewModel;
            LoadSpells();
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

        private void DeleteCharacter(string characterName)
        {
            // Retrieve the current list of characters
            string existingCharactersJson = Preferences.Get("characters", "[]");
            var characters = JsonConvert.DeserializeObject<List<string>>(existingCharactersJson);

            // Remove the selected character
            characters.Remove(characterName);

            // Save the updated list
            string updatedCharactersJson = JsonConvert.SerializeObject(characters);
            Preferences.Set("characters", updatedCharactersJson);

            // Assuming you have some way to refresh the character list on the previous page
            // You might need to use MessagingCenter or an event to notify the MainPage to refresh
        }

        [Obsolete]
        private void OnAddSpellClicked(object sender, EventArgs e)
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                await Navigation.PushAsync(new SpellListPage());
            });
        }

        public void LoadSpells()
        {
            var spellsJson = Preferences.Get("spells", string.Empty);
            if (!string.IsNullOrEmpty(spellsJson))
            {
                var spells = JsonConvert.DeserializeObject<ObservableCollection<Spell>>(spellsJson);
                if (spells != null)
                {
                    SharedViewModel.Instance.Spells.Clear();
                    foreach (var spell in spells)
                    {
                        SharedViewModel.Instance.Spells.Add(spell);
                    }
                }
            }
        }

    }
}
