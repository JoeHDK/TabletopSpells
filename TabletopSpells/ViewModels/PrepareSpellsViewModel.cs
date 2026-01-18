using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using TabletopSpells.Models;

namespace TabletopSpells.ViewModels
{
    public class PrepareSpellsViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public event EventHandler<CloseEventArgs>? RequestClose;

        public ObservableCollection<SpellPrepareItemViewModel> AddedSpells { get; }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        private readonly Func<int> _getMaxPrepared;

        public int MaxPrepared => _getMaxPrepared();

        private string _prepareSummary = string.Empty;
        public string PrepareSummary
        {
            get => _prepareSummary;
            set
            {
                if (_prepareSummary != value)
                {
                    _prepareSummary = value;
                    OnPropertyChanged(nameof(PrepareSummary));
                }
            }
        }

        public PrepareSpellsViewModel(List<Spell> allSpells, Func<int> getMaxPrepared)
        {
            _getMaxPrepared = getMaxPrepared ?? throw new ArgumentNullException(nameof(getMaxPrepared));

            // Filter only added spells (those that are in KnownSpells or manually added)
            var addedSpells = allSpells
                .Where(s => s != null)
                .ToList();

            AddedSpells = new ObservableCollection<SpellPrepareItemViewModel>(
                addedSpells.Select(s => new SpellPrepareItemViewModel(s, OnSpellPreparedToggled))
            );

            SaveCommand = new Command(() => RequestClose?.Invoke(this, new CloseEventArgs(true)));
            CancelCommand = new Command(() => RequestClose?.Invoke(this, new CloseEventArgs(false)));

            UpdatePrepareSummary();
        }

        private void OnSpellPreparedToggled(SpellPrepareItemViewModel item, bool newValue)
        {
            if (newValue)
            {
                var currentPreparedCount = AddedSpells.Count(x => x.IsPrepared);

                if (currentPreparedCount >= MaxPrepared)
                {
                    // Disallow and reset the checkbox
                    item.IsPrepared = false;
                    return;
                }
            }

            UpdatePrepareSummary();
        }

        private void UpdatePrepareSummary()
        {
            var prepared = AddedSpells.Count(x => x.IsPrepared);
            PrepareSummary = $"Prepared: {prepared} / {MaxPrepared}";
        }

        protected void OnPropertyChanged(string name)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public class CloseEventArgs : EventArgs
        {
            public bool ApplyChanges { get; }
            public CloseEventArgs(bool applyChanges) => ApplyChanges = applyChanges;
        }
    }

    public class SpellPrepareItemViewModel : INotifyPropertyChanged
    {
        private bool _isPrepared;
        private readonly Action<SpellPrepareItemViewModel, bool> _onToggle;

        public event PropertyChangedEventHandler? PropertyChanged;

        public Guid Id { get; }
        public string Name { get; }

        public bool IsPrepared
        {
            get => _isPrepared;
            set
            {
                if (_isPrepared != value)
                {
                    _isPrepared = value;
                    OnPropertyChanged(nameof(IsPrepared));
                    OnPropertyChanged(nameof(IsCheckboxEnabled));
                    _onToggle?.Invoke(this, value);
                }
            }
        }

        public bool IsCheckboxEnabled { get; set; } = true;

        public SpellPrepareItemViewModel(Spell model, Action<SpellPrepareItemViewModel, bool> onToggle)
        {
            Id = model.Id;
            Name = model.Name ?? "Unknown Spell";
            _isPrepared = model.IsPrepared;
            _onToggle = onToggle;
        }

        protected void OnPropertyChanged(string name)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}

