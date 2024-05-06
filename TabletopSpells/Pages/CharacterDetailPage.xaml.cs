using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace TabletopSpells.Pages
{
    public partial class CharacterDetailPage : ContentPage
    {
        private string CharacterClass
        {
            get;
        }
        private SharedViewModel? ViewModel
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
            this.CharacterClass = ViewModel.CurrentCharacter.CharacterClass.ToString();

            this.BindingContext = ViewModel;
            ViewModel.LoadSpellsForCharacter(characterName);

            if (ViewModel.CharacterSpells.ContainsKey(CharacterName))
            {
                CreateList();
            }
        }

        [Obsolete]
        private void OnSearchSpellsClicked(object sender, EventArgs e)
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                await Navigation.PushAsync(new SpellListPage(CharacterName, CharacterClass));
            });
        }

        private void CreateList()
        {
            var groupedSpells = ViewModel.CharacterSpells[CharacterName]
                .GroupBy(spell => ParseSpellLevel(spell.SpellLevel, CharacterClass))
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

            return -1; 
        }

        private async void OnSpellSelected(object sender, SelectionChangedEventArgs e)
        {
            var selectedSpell = e.CurrentSelection.FirstOrDefault() as Spell;
            if (selectedSpell != null)
            {
                int spellLevel = ParseSpellLevel(selectedSpell.SpellLevel, CharacterClass);
                if (spellLevel >= 0) 
                {
                    await Navigation.PushAsync(new SpellDetailPage(selectedSpell, CharacterName, spellLevel));
                }
                else
                {
                    await DisplayAlert("Error", "Failed to determine spell level for the selected spell.", "OK");
                }
            }

        ((CollectionView)sender).SelectedItem = null;
        }
    }
}
