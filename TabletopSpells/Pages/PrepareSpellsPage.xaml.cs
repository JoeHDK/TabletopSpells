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
                System.Diagnostics.Debug.WriteLine("=== PrepareSpellsPage.OnRequestClose START ===");
                System.Diagnostics.Debug.WriteLine($"Character: {_character.Name}, ID: {_character.ID}");
                
                var currentlyPreparedIds = _character.GetPreparedSpells()
                    .Select(s => s.Id)
                    .ToHashSet();
                System.Diagnostics.Debug.WriteLine($"Currently prepared (before): {currentlyPreparedIds.Count}");

                foreach (var item in _viewModel.AddedSpells)
                {
                    var original = FindOriginalSpell(item.Id);
                    if (original == null)
                    {
                        System.Diagnostics.Debug.WriteLine($"  ✗ Spell {item.Id} not found in original spells");
                        continue;
                    }

                    var wasAlreadyPrepared = currentlyPreparedIds.Contains(item.Id);
                    var shouldNowBePrepared = item.IsPrepared;

                    System.Diagnostics.Debug.WriteLine($"  Spell: {original.Name}, ID: {original.Id}, Was: {wasAlreadyPrepared}, Now: {shouldNowBePrepared}");

                    if (wasAlreadyPrepared == shouldNowBePrepared) continue;
                    var result = _character.TogglePreparedSpell(original);
                    System.Diagnostics.Debug.WriteLine($"    Toggle result: {result}");
                }

                var afterToggle = _character.GetPreparedSpells();
                System.Diagnostics.Debug.WriteLine($"Prepared spells after toggle: {afterToggle.Count}");
                foreach (var spell in afterToggle)
                {
                    System.Diagnostics.Debug.WriteLine($"  - {spell.Name} (ID: {spell.Id})");
                }

                SharedViewModel.Instance.SavePreparedSpells(_character);
                System.Diagnostics.Debug.WriteLine("=== PrepareSpellsPage.OnRequestClose END ===");
            }

            await Navigation.PopModalAsync();
        }

        private Spell? FindOriginalSpell(Guid id)
        {
            return _originalSpells.FirstOrDefault(spell => spell.Id == id);
        }
    }
}

