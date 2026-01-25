using TabletopSpells.Models;
using TabletopSpells.ViewModels;

namespace TabletopSpells.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PrepareSpellsPage
    {
        private readonly PrepareSpellsViewModel _viewModel;
        private readonly List<Spell> _originalSpells;
        private readonly Character _character;

        public PrepareSpellsPage(List<Spell> spells, Func<int> getMaxPrepared, Character character)
        {
            InitializeComponent();

            _originalSpells = spells ?? throw new ArgumentNullException(nameof(spells));
            _character = character ?? throw new ArgumentNullException(nameof(character));

            _viewModel = new PrepareSpellsViewModel(spells, getMaxPrepared);
            _viewModel.RequestClose += OnRequestClose;
            BindingContext = _viewModel;
        }

        private async void OnRequestClose(object? sender, PrepareSpellsViewModel.CloseEventArgs e)
        {
            if (e.ApplyChanges)
            {

                foreach (var item in _viewModel.AddedSpells)
                {
                    var original = FindOriginalSpell(item.Id);
                    if (original == null) continue;
                    
                    original.IsPrepared = item.IsPrepared;
                        
                    if (item.IsPrepared)
                    {
                        _character.AddSpell(original);
                    }

                    _character.TogglePreparedSpell(original);
                }

                foreach (var spell in _character.GetPreparedSpells())
                {
                    SharedViewModel.Instance.SaveSpellForCharacter(_character, spell);
                }

                SharedViewModel.Instance.SavePreparedSpells(_character);

                _character.ClearManualllyPreparedSpells();
                SharedViewModel.Instance.LoadPreparedSpells(_character);
            }

            await Navigation.PopModalAsync();
        }

        private Spell? FindOriginalSpell(Guid id)
        {
            foreach (var spell in _originalSpells)
            {
                if (spell.Id == id)
                    return spell;
            }
            return null;
        }
    }
}

