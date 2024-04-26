using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;

namespace TabletopSpells.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    // SpellListPage.xaml.cs
    public partial class SpellListPage : ContentPage
    {
        private string characterName;
        public SpellListPage(string characterName)
        {
            InitializeComponent();
            this.characterName = characterName;
            Spells = new ObservableCollection<Spell>(GetAllSpellsFromJson());
            FilteredSpells = new ObservableCollection<Spell>(Spells);
            BindingContext = this;
        }

        public ObservableCollection<Spell> Spells
        {
            get; set;
        }
        public ObservableCollection<Spell> FilteredSpells
        {
            get; set;
        }

        [Obsolete]
        private async void OnSearchResultSelected(object sender, SelectionChangedEventArgs e)
        {
            var selectedSpell = e.CurrentSelection.FirstOrDefault() as Spell;
            if (selectedSpell != null)
            {
                // Show confirmation dialog
                bool addToCharacter = await DisplayAlert("Add Spell",
                    $"Do you want to add '{selectedSpell.Name}' to your character?",
                    "Yes", "No");

                if (addToCharacter)
                {
                    // Logic to add the spell to the character's list
                    // You might need to use MessagingCenter or an event
                    MessagingCenter.Send(this, "AddSpellToCharacter", selectedSpell);

                    // Optionally, navigate back after adding
                    await Navigation.PopAsync();
                }

                // Clear selection
                ((CollectionView)sender).SelectedItem = null;
            }
        }

        private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            var searchText = e.NewTextValue.ToLower();
            FilteredSpells.Clear();
            foreach (var spell in Spells.Where(s => s.Name.ToLower().Contains(searchText)))
            {
                FilteredSpells.Add(spell);
            }
        }

        private List<Spell> GetAllSpellsFromJson()
        {
            try
            {
                var assembly = IntrospectionExtensions.GetTypeInfo(typeof(App)).Assembly;
                Stream? stream = assembly.GetManifestResourceStream("TabletopSpells.Spells.Pathfinder1e.json");

                if (stream == null)
                {
                    Debug.WriteLine("Spell data stream not found.");
                    return new List<Spell>();
                }

                using var reader = new StreamReader(stream);
                var jsonContent = reader.ReadToEnd();

                var settings = new JsonSerializerSettings
                {
                    Converters = new List<JsonConverter> { new StringEnumConverter() },
                    NullValueHandling = NullValueHandling.Ignore,
                    Error = (sender, args) =>
                    {
                        Debug.WriteLine("JSON Error: " + args.ErrorContext.Error.Message);
                        args.ErrorContext.Handled = true;
                    }
                };

                var spells = JsonConvert.DeserializeObject<List<Spell>>(jsonContent, settings);

                if (spells == null)
                {
                    Debug.WriteLine("Failed to deserialize spells.");
                    return new List<Spell>();
                }

                return spells;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception occurred while loading spells: {ex}");
                return new List<Spell>();
            }
        }

        private async void OnSpellSelected(object sender, SelectionChangedEventArgs e)
        {
            var selectedSpell = e.CurrentSelection.FirstOrDefault() as Spell;
            if (selectedSpell != null)
            {
                await Navigation.PushAsync(new SpellDetailPage(selectedSpell, characterName));
            }

            ((CollectionView)sender).SelectedItem = null;
            //var selectedSpell = e.CurrentSelection.FirstOrDefault() as Spell;
            //if (selectedSpell != null)
            //{
            //    bool addToCharacter = await DisplayAlert("Add Spell",
            //        $"Do you want to add '{selectedSpell.Name}' to your character?",
            //        "Yes", "No");

            //    if (addToCharacter)
            //    {
            //        SharedViewModel.Instance.AddSpell(characterName, selectedSpell);
            //        await Navigation.PopAsync();
            //    }

            //    ((CollectionView)sender).SelectedItem = null;
            //}
        }
    }
}