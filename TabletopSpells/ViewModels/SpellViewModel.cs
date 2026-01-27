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

    // Notifying wrapper for the Spell's IsFavoriteSpell so UI can bind and get updates
    public bool IsFavorite
    {
        get => Spell.IsFavoriteSpell;
        set
        {
            if (Spell.IsFavoriteSpell == value) return;
            Spell.IsFavoriteSpell = value;
            OnPropertyChanged(nameof(IsFavorite));
            // Also notify that Spell changed if other bindings use it
            OnPropertyChanged(nameof(Spell));
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