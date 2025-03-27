using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using TabletopSpells.Models;
using TabletopSpells.Models.Enums;
using TabletopSpells.ViewModels;

namespace TabletopSpells.Pages
{
    public partial class CharacterDetailPage : ContentPage
    {
        private readonly Game gameType;
        private string CharacterClass
        {
            get;
        }
        private SharedViewModel? ViewModel
        {
            get; set;
        }
        private readonly Character character;

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
            Title = $"{character.Name}'s spells";
            ViewModel = viewModel;  // Use the passed viewModel
            CharacterClass = ViewModel.CurrentCharacter.CharacterClass.ToString();

            BindingContext = ViewModel;
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
            var characterClassLower = CharacterClass.ToLower();
            var spellsToShow = new List<Spell>();

            if (character.IsDivineCaster)
            {
                // Use prepared spells + auto-prepared
                spellsToShow = character.GetPreparedSpells().ToList();

                foreach (var spellName in character.AlwaysPreparedSpells)
                {
                    var alwaysPrepared = ViewModel.SpellsForCharacter(character)
                        .FirstOrDefault(s => s.Name.Equals(spellName, StringComparison.OrdinalIgnoreCase));
                    if (alwaysPrepared != null && !spellsToShow.Contains(alwaysPrepared))
                    {
                        spellsToShow.Add(alwaysPrepared);
                    }
                }
            }
            else
            {
                spellsToShow = ViewModel.CharacterSpells[character.ID].ToList();
            }

            var groupedSpells = spellsToShow
                .Select(spell =>
                {
                    var spellLevelForClass = ParseSpellLevel(spell.SpellLevel, CharacterClass);
                    spell.IsNativeSpell = spell.SpellLevel?.ToLower().Contains(characterClassLower) ?? false;
                    return new
                    {
                        Level = spellLevelForClass,
                        Spell = spell
                    };
                })
                .Where(spellInfo => spellInfo.Level != -1)
                .GroupBy(spellInfo => spellInfo.Level)
                .OrderBy(group => group.Key)
                .Select(group => new Grouping<int, Spell>(
                    group.Key,
                    group.Select(spellInfo => spellInfo.Spell).OrderBy(s => s.Name).ToList()))
                .ToList();

            var groupedCollection = new ObservableCollection<Grouping<int, Spell>>(groupedSpells);
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
