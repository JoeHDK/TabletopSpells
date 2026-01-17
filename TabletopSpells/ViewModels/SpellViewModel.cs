using System.ComponentModel;
using TabletopSpells.Models;

namespace TabletopSpells.ViewModels;

public class SpellViewModel : INotifyPropertyChanged
{
    public Spell Spell { get; }
    private readonly Character character;

    public SpellViewModel(Spell spell, Character character)
    {
        Spell = spell;
        this.character = character;
    }

    public bool IsPrepared
    {
        get => character.GetPreparedSpells().Any(s => s.Id == Spell.Id);
        set
        {
            bool currentlyPrepared = IsPrepared;
            if (value == currentlyPrepared) return;

            // If trying to prepare and at limit, handle via MainThread to show dialog
            if (value && !character.TogglePreparedSpell(Spell))
            {
                // Failed to prepare - at limit. Show replacement dialog via main thread
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    var preparedSpells = character.GetPreparedSpells();
                    var selectedReplacement = await TabletopSpells.Pages.PrepareSpellReplacementDialog.ShowAsync(preparedSpells, Spell.Name);
                    
                    if (selectedReplacement != null)
                    {
                        // Replace the spell
                        character.TogglePreparedSpell(selectedReplacement);
                        character.TogglePreparedSpell(Spell);
                        SharedViewModel.Instance.SavePreparedSpells(character);
                        OnPropertyChanged(nameof(IsPrepared));
                        SharedViewModel.Instance.SpellsChanged?.Invoke();
                    }
                });
                return;
            }

            if (value) // Successfully prepared
            {
                SharedViewModel.Instance.SavePreparedSpells(character);
                OnPropertyChanged(nameof(IsPrepared));
                SharedViewModel.Instance.SpellsChanged?.Invoke();
            }
            else if (character.TogglePreparedSpell(Spell)) // Successfully unprepared
            {
                SharedViewModel.Instance.SavePreparedSpells(character);
                OnPropertyChanged(nameof(IsPrepared));
                SharedViewModel.Instance.SpellsChanged?.Invoke();
            }
        }
    }

    public string PreparedSpellCountText =>
        $"Prepared: {character.GetPreparedSpells().Count} / {character.Level + character.GetRelevantAbilityModifier()}";

    public event PropertyChangedEventHandler PropertyChanged;
    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}