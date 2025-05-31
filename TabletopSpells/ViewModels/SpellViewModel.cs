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
        get => character.GetPreparedSpells().Any(s => s.Name == Spell.Name);
        set
        {
            if (character.TogglePreparedSpell(Spell))
            {
                OnPropertyChanged(nameof(IsPrepared));
                SharedViewModel.Instance.SpellsChanged?.Invoke(); // Ensure this triggers updates
            }
        }
    }

    public string PreparedSpellCountText =>
        $"Prepared: {character.GetPreparedSpells().Count} / {character.Level + character.GetRelevantAbilityModifier()}";

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(string name) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}