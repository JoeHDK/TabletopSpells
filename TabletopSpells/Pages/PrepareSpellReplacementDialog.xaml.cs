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
    private TaskCompletionSource<Spell?>? _tcs;

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

    public static async Task<Spell?> ShowAsync(List<Spell> preparedSpells, string newSpellName)
    {
        var dialog = new PrepareSpellReplacementDialog(preparedSpells, newSpellName);
        dialog._tcs = new TaskCompletionSource<Spell?>();
        await Shell.Current.Navigation.PushModalAsync(dialog);
        var result = await dialog._tcs.Task;
        return result;
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
        // Return null result
        if (_tcs != null && !_tcs.Task.IsCompleted) _tcs.SetResult(null);
        await Shell.Current.Navigation.PopModalAsync();
    }

    private async void OnReplaceClicked(object sender, EventArgs e)
    {
        if (selectedReplacement != null)
        {
            if (_tcs != null && !_tcs.Task.IsCompleted) _tcs.SetResult(selectedReplacement);
            await Shell.Current.Navigation.PopModalAsync();
        }
    }

    public Spell? GetSelectedReplacement() => selectedReplacement;
}
