using System.Collections.ObjectModel;
using TabletopSpells.Models;

namespace TabletopSpells.Pages;

public partial class PrepareSpellReplacementDialog : ContentPage
{
    public ObservableCollection<Spell> PreparedSpells { get; set; } = new();
    public string SelectedSpellName { get; set; } = string.Empty;
    public Spell? SelectedSpell { get; set; }
    public bool IsSelectionValid { get; set; }

    private Spell? selectedReplacement;

    public PrepareSpellReplacementDialog(List<Spell> preparedSpells, string newSpellName)
    {
        InitializeComponent();
        SelectedSpellName = newSpellName;
        
        foreach (var spell in preparedSpells)
        {
            PreparedSpells.Add(spell);
        }

        BindingContext = this;
    }

    private void OnSpellSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is Spell selectedSpell)
        {
            selectedReplacement = selectedSpell;
            IsSelectionValid = true;
            OnPropertyChanged(nameof(IsSelectionValid));
        }
    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private async void OnReplaceClicked(object sender, EventArgs e)
    {
        if (selectedReplacement != null)
        {
            await Navigation.PopAsync();
            Shell.Current.SendBackButtonPressed(); // Signal completion
        }
    }

    public Spell? GetSelectedReplacement() => selectedReplacement;
}
