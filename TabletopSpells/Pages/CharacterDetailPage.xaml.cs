using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using TabletopSpells.Models;

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
            ViewModel = viewModel;  // Use the passed viewModel

            this.BindingContext = ViewModel;
            ViewModel.LoadSpellsForCharacter(characterName);

            if (ViewModel.CharacterSpells.ContainsKey(CharacterName))
            {
                CreateList();
            }
        }

        [Obsolete]
        private async void OnMenuClicked(object sender, EventArgs e)
        {
            string action = await DisplayActionSheet("Menu", "Cancel", null, "Add Spell", "Delete Character");

            switch (action)
            {
                case "Add Spell":
                    OnAddSpellClicked(this, null);
                    break;
                case "Delete Character":
                    OnDeleteCharacterClicked(this, null);
                    break;
            }
        }

        private void CreateList()
        {
            string characterClass = ViewModel.CurrentCharacter.CharacterClass.ToString();

            var groupedSpells = ViewModel.CharacterSpells[CharacterName]
                .GroupBy(spell => ParseSpellLevel(spell.SpellLevel, characterClass))
                .OrderBy(group => group.Key)
                .Select(group => new { Level = group.Key, Spells = group.OrderBy(spell => spell.Name) }) // Add sorting by name within each level
                .ToList();

            var groupedCollection = new ObservableCollection<Grouping<int, Spell>>();

            foreach (var group in groupedSpells)
            {
                var spellsSortedAlphabetically = group.Spells.OrderBy(spell => spell.Name).ToList(); // Ensure spells are sorted alphabetically
                groupedCollection.Add(new Grouping<int, Spell>(group.Level, spellsSortedAlphabetically));
            }

            SpellListView.ItemsSource = groupedCollection;
        }


        private int ParseSpellLevel(string spellLevel, string characterClass)
        {
            string classLowerCase = characterClass.ToLower();
            string[] parts = spellLevel.Split(',');

            foreach (var part in parts)
            {
                if (part.Trim().ToLower().Contains(classLowerCase))
                {
                    var match = Regex.Match(part, @"\d+");
                    if (match.Success)
                    {
                        return int.Parse(match.Value);
                    }
                }
            }

            return -1; // Return an invalid level if not found
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

            ((CollectionView)sender).SelectedItem = null;
        }
    }
}
