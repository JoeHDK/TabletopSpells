using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.RegularExpressions;
using TabletopSpells.Models;
using TabletopSpells.Models.Enums;

namespace TabletopSpells.Pages
{
    public partial class CharacterDetailPage : ContentPage
    {
        private Game gameType;
        private string CharacterClass
        {
            get;
        }
        private SharedViewModel? ViewModel
        {
            get; set;
        }
        private Character character;

        protected override void OnAppearing()
        {
            base.OnAppearing();
            CreateList();
        }

        public CharacterDetailPage(Character character, SharedViewModel viewModel, Game gameType)
        {
            InitializeComponent();
            this.gameType = gameType;
            this.character = character;
            this.Title = $"{character.Name}'s spells";
            ViewModel = viewModel;  // Use the passed viewModel
            this.CharacterClass = ViewModel.CurrentCharacter.CharacterClass.ToString();

            this.BindingContext = ViewModel;
            ViewModel.LoadSpellsForCharacter(character);
            
            if (ViewModel.CharacterSpells.ContainsKey(character.ID))
            {
                CreateList();
            }
        }

        [Obsolete]
        private void OnSearchSpellsClicked(object sender, EventArgs e)
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                await Navigation.PushAsync(new SpellListPage(character, gameType));
            });
        }

        private void CreateList()
        {
            // Get the character's class in lowercase for easier comparison
            string characterClassLower = CharacterClass.ToLower();

            // Group spells by their level for the current character, setting the IsNativeSpell property
            var groupedSpells = ViewModel.CharacterSpells[character.ID]
                .Select(spell =>
                {
                    // Determine the level for the character's class
                    int spellLevelForClass = ParseSpellLevel(spell.SpellLevel, CharacterClass);

                    // Check if this spell is native to the character's class
                    spell.IsNativeSpell = spell.SpellLevel?.ToLower().Contains(characterClassLower) ?? false;

                    return new
                    {
                        Level = spellLevelForClass,
                        Spell = spell
                    };
                })
                .Where(spellInfo => spellInfo.Level != -1) // Exclude spells that couldn't be parsed or are irrelevant
                .GroupBy(spellInfo => spellInfo.Level)
                .OrderBy(group => group.Key)
                .Select(group => new
                {
                    Level = group.Key,
                    Spells = group.Select(spellInfo => spellInfo.Spell).OrderBy(spell => spell.Name).ToList()
                })
                .ToList();

            // Create the grouped collection to display in the CollectionView
            var groupedCollection = new ObservableCollection<Grouping<int, Spell>>();

            foreach (var group in groupedSpells)
            {
                groupedCollection.Add(new Grouping<int, Spell>(group.Level, group.Spells));
            }

            // Set the ItemsSource of the CollectionView to the grouped collection
            SpellListView.ItemsSource = groupedCollection;
        }


        private int ParseSpellLevel(string spellLevel, string characterClass)
        {
            string[] parts = spellLevel.Split(',');

            // Initialize the minimum level to a very high number (greater than any possible spell level)
            int minLevel = int.MaxValue;

            foreach (var part in parts)
            {
                var match = Regex.Match(part, @"\d+");
                if (match.Success && int.TryParse(match.Value, out int level))
                {
                    // Check if this part contains the character's class
                    if (part.Trim().ToLower().Contains(characterClass.ToLower()))
                    {
                        return level; // Return the level directly if it's for the character's class
                    }
                    else
                    {
                        // Update the minimum level if this level is lower than the current minLevel
                        minLevel = Math.Min(minLevel, level);
                    }
                }
            }

            // If the character's class wasn't found in the list, return the minimum level found
            return minLevel == int.MaxValue ? -1 : minLevel;
        }

        private async void OnSpellSelected(object sender, SelectionChangedEventArgs e)
        {
            var selectedSpell = e.CurrentSelection.FirstOrDefault() as Spell;
            if (selectedSpell != null)
            {
                int spellLevel = ParseSpellLevel(selectedSpell.SpellLevel, CharacterClass);
                await Navigation.PushAsync(new SpellDetailPage(selectedSpell, character, spellLevel, gameType));

            }
        ((CollectionView)sender).SelectedItem = null;
        }
    }
}
