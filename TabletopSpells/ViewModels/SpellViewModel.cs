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

            if (character.TogglePreparedSpell(Spell))
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