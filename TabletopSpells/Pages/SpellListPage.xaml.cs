using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;

namespace TabletopSpells.Pages;
[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class SpellListPage : ContentPage
{
    SharedViewModel ViewModel;
    private string characterName;
    public SpellListPage(string characterName)
    {
        InitializeComponent();
        this.characterName = characterName;
        Spells = new ObservableCollection<Spell>(GetAllSpellsFromJson());
        FilteredSpells = new ObservableCollection<Spell>(Spells);
        BindingContext = this;
        this.ViewModel = SharedViewModel.Instance;
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
    private async void OnMenuClicked(object sender, EventArgs e)
    {
        string action = await DisplayActionSheet("Filters", "Cancel", null, "Class", "--");

        switch (action)
        {
            case "Class":
                FilterByClass();
                break;
            case "Delete Character":

                break;
        }
    }

    private void FilterByClass()
    {
        
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
                MessagingCenter.Send(this, "AddSpellToCharacter", selectedSpell);
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
    }
}
