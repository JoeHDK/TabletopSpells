using System;
using System.Collections.ObjectModel;
using Xamarin.Forms;

namespace TabletopSpells.Pages
{
    public partial class CharacterDetailPage : ContentPage
    {
        public ObservableCollection<string> Spells
        {
            get; private set;
        }
        public ObservableCollection<string> PreparedSpells
        {
            get; private set;
        }

        private string CharacterName
        {
            get; set;
        }

        public CharacterDetailPage(string characterName)
        {
            InitializeComponent();
            CharacterName = characterName;
            this.Title = $"{CharacterName}'s Details";

            Spells = new ObservableCollection<string>();
            PreparedSpells = new ObservableCollection<string>();

            SpellListView.ItemsSource = Spells;
            PreparedSpellListView.ItemsSource = PreparedSpells;
        }

        private void OnAddSpellClicked(object sender, EventArgs e)
        {
            // Implement the Add Spell functionality
        }

        // Other methods for handling spell list interactions
    }
}
