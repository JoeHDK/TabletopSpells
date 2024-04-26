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
