using TabletopSpells.Models;
using TabletopSpells.Models.Enums;
using TabletopSpells.ViewModels;

namespace TabletopSpells.Pages;
public partial class SpellDetailPage
{
    private readonly Spell spell;
    private readonly Character character;
    private readonly int spellLevel;
    private bool spellIsKnown;
    private bool spellIsPrepared;
    private bool castAsRitual;

    public SpellDetailPage(Spell spell, Character character, int spellLevel, Game gameType)
    {
        InitializeComponent();
        this.spell = spell;
        //this.gameType = gameType;
        this.character = character;
        BindingContext = spell;
        this.spellLevel = spellLevel;
        CheckIfSpellIsKnown();
        CheckIfSpellIsPrepared();
        UpdateButtons();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        
        // Refresh the prepared state in case it changed while navigating
        CheckIfSpellIsKnown();
        CheckIfSpellIsPrepared();
        UpdateButtons();
    }

    private void CheckIfSpellIsKnown()
    {
        var viewModel = SharedViewModel.Instance;
        spellIsKnown = viewModel.CharacterSpells.ContainsKey(character.ID) &&
                       viewModel.CharacterSpells[character.ID].Any(s => s.Name == spell.Name);
    }

    private void CheckIfSpellIsPrepared()
    {
        // Check only manually prepared spells, NOT domain spells
        spellIsPrepared = character.GetManuallyPreparedSpells().Any(s => s.Id == spell.Id);
    }

    private void UpdateButtons()
    {
        // Clear toolbar items
        ToolbarItems.Clear();
        
        // Determine what toolbar items to show based on state
        if (!spellIsKnown)
        {
            // Spell not in list - show simple "Add Spell" button
            var addButton = new ToolbarItem
            {
                Text = "Add Spell",
                Priority = 0,
                Order = ToolbarItemOrder.Primary
            };
            addButton.Clicked += async (s, e) => await OnAddSpellClickedAsync();
            ToolbarItems.Add(addButton);
        }
        else
        {
            // Spell is in list - show burger menu with multiple options
            var menuButton = new ToolbarItem
            {
                Text = "☰",
                Priority = 0,
                Order = ToolbarItemOrder.Primary
            };
            menuButton.Clicked += OnMenuButtonClicked;
            ToolbarItems.Add(menuButton);
        }
        
        // ShowCastSpellButton handles the cast button visibility
        ShowCastSpellButton();
    }

    private async void OnMenuButtonClicked(object? sender, EventArgs e)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("=== OnMenuButtonClicked START ===");
            System.Diagnostics.Debug.WriteLine($"Character: {character.Name}, IsDivineCaster: {character.IsDivineCaster}");
            System.Diagnostics.Debug.WriteLine($"SpellIsKnown: {spellIsKnown}");
            
            // Build the menu options based on current state
            var options = new List<string>();

            // Add/Remove spell option (always available)
            if (spellIsKnown)
            {
                options.Add("Remove Spell");
                System.Diagnostics.Debug.WriteLine("Added option: Remove Spell");
            }
            else
            {
                options.Add("Add Spell");
                System.Diagnostics.Debug.WriteLine("Added option: Add Spell");
            }

            // Mark/Unmark Domain option (only for divine casters with known spells)
            if (character.IsDivineCaster && spellIsKnown)
            {
                bool isAlwaysPrepared = spell.IsAlwaysPrepared || 
                                       character.AlwaysPreparedSpells.Contains(spell.Name ?? string.Empty);
                if (isAlwaysPrepared)
                {
                    options.Add("Unmark Domain");
                    System.Diagnostics.Debug.WriteLine("Added option: Unmark Domain");
                }
                else
                {
                    options.Add("Mark Domain");
                    System.Diagnostics.Debug.WriteLine("Added option: Mark Domain");
                }
            }

            System.Diagnostics.Debug.WriteLine($"Total options: {options.Count}");
            
            // Show the action sheet
            var action = await DisplayActionSheet("Spell Options", "Cancel", null, options.ToArray());
            System.Diagnostics.Debug.WriteLine($"User selected: {action}");

            if (action == "Cancel" || string.IsNullOrEmpty(action))
            {
                System.Diagnostics.Debug.WriteLine("=== OnMenuButtonClicked CANCELLED ===");
                return;
            }

            // Handle the selected action
            if (action == "Add Spell")
            {
                System.Diagnostics.Debug.WriteLine("Executing: Add Spell");
                await OnAddSpellClickedAsync();
            }
            else if (action == "Remove Spell")
            {
                System.Diagnostics.Debug.WriteLine("Executing: Remove Spell");
                await OnRemoveSpellClickedAsync();
            }
            else if (action == "Mark Domain")
            {
                System.Diagnostics.Debug.WriteLine("Executing: Mark Domain");
                await OnToggleAlwaysPreparedAsync(markAsDomain: true);
            }
            else if (action == "Unmark Domain")
            {
                System.Diagnostics.Debug.WriteLine("Executing: Unmark Domain");
                await OnToggleAlwaysPreparedAsync(markAsDomain: false);
            }
            
            System.Diagnostics.Debug.WriteLine("=== OnMenuButtonClicked END ===");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ERROR in OnMenuButtonClicked: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
        }
    }

    private void ShowCastSpellButton()
    {
        // Initially hide and disable the cast spell button
        CastSpellButton.IsVisible = false;
        CastSpellButton.IsEnabled = false;
        CastSpellButton.Clicked -= OnCastSpellClicked; // Clear any existing event subscriptions

        // Check if spell is available:
        // - For divine casters: spell must be in added spells AND (marked as prepared OR marked as domain)
        // - For others: spell must be in added spells
        bool spellAvailable;
        if (character.IsDivineCaster)
        {
            // Divine casters: need spell in list AND (manually prepared OR domain spell)
            bool isAlwaysPrepared = spell.IsAlwaysPrepared || 
                                   character.AlwaysPreparedSpells.Contains(spell.Name ?? string.Empty);
            spellAvailable = spellIsKnown && (spellIsPrepared || isAlwaysPrepared);
        }
        else
        {
            // Traditional casters just need the spell to be known/added
            spellAvailable = spellIsKnown;
        }

        // If the spell isn't available to the character, no further action is needed
        if (!spellAvailable)
        {
            return;
        }

        // Determine if there are any available spell slots for the spell's level or higher
        var hasAvailableSpellSlots = false;
        for (var level = spellLevel; level <= 9; level++) // Assuming spell levels range from 0 to 9
        {
            if (!character.MaxSpellsPerDay.TryGetValue(level, out var maxSpells) ||
                maxSpells <= character.SpellsUsedToday.GetValueOrDefault(level, 0)) continue;
            hasAvailableSpellSlots = true;
            break;
        }

        // Show the cast spell button since the spell is available
        CastSpellButton.IsVisible = true;

        if (spell.Ritual || hasAvailableSpellSlots || spellLevel == 0)
        {
            CastSpellButton.IsEnabled = true;
            CastSpellButton.Clicked += OnCastSpellClicked;
        }
        else
        {
            CastSpellButton.Opacity = 0.5;
        }
    }


    private async void OnCastSpellClicked(object? sender, EventArgs eventArgs)
    {
        try
        {
            var currentCharacter = SharedViewModel.Instance.CurrentCharacter;

            if (currentCharacter == null)
            {
                await DisplayAlert("Error", "No character selected.", "OK");
                return;
            }

            if (spell.Ritual)
            {
                var castAsRitualConfirmation = await DisplayAlert("Cast as Ritual",
                    $"Do you want to cast '{spell.Name}' as a ritual?",
                    "Yes",
                    "No");

                if (castAsRitualConfirmation)
                {
                    castAsRitual = true;
                    // await DisplayAlert("Ritual Cast", $"{spell.Name} has been cast as a ritual.", "OK");
                    if (spell.Name != null)
                        SharedViewModel.Instance.LogSpellCast(currentCharacter, spell.Name, spellLevel, castAsRitual);
                    ReloadUI();
                    return;
                }
            }

            if (spellLevel == 0)
            {
                if (spell.Name != null)
                    SharedViewModel.Instance.LogSpellCast(currentCharacter, spell.Name, spellLevel, false);
                await DisplayAlert("Cast as Cantrip", "Success", "OK");
                ReloadUI();
                return;
            }

            var spellSlots = new List<string>();
            foreach (var (level, maxSpells) in currentCharacter.MaxSpellsPerDay)
            {
                if (level < spellLevel) continue;
        
                var usedSpells = currentCharacter.SpellsUsedToday.GetValueOrDefault(level, 0);
                var remainingSpells = maxSpells - usedSpells;

                if (maxSpells <= 0) continue;
                var slotDisplay = $"Level {level} ({remainingSpells} remaining)";
                spellSlots.Add(slotDisplay);
            }

            if (spellSlots.Count == 0)
            {
                await DisplayAlert("No Spell Slots", $"No spell slots available for {spell.Name}.", "OK");
                return;
            }

            var selectedSlot = await DisplayActionSheet($"{spell.Name} (lvl: {spellLevel})", "Cancel", null, spellSlots.ToArray());

            if (selectedSlot == "Cancel" || string.IsNullOrEmpty(selectedSlot))
            {
                return;
            }

            var selectedLevel = int.Parse(selectedSlot.Split(' ')[1]);

            var success = currentCharacter.CastSpell(selectedLevel);

            if (success)
            {
                if (spell.Name != null)
                {
                    SharedViewModel.Instance.LogSpellCast(currentCharacter, spell.Name, selectedLevel, castAsRitual);

                    SharedViewModel.Instance.SaveSpellsPerDayDetails(currentCharacter, currentCharacter.MaxSpellsPerDay,
                        currentCharacter.SpellsUsedToday);
                }
            }
            else
            {
                if (spell.Name != null)
                {
                    SharedViewModel.Instance.LogFailedSpellCast(currentCharacter, spell.Name, selectedLevel,
                        "Unable to cast spell.");
                    await DisplayAlert("Failed", $"Failed to cast {spell.Name} at level {selectedLevel}.", "OK");
                }
            }

            ReloadUI();
        }
        catch (Exception e)
        {
            await DisplayAlert("Error", $"An error occurred while casting the spell: {e.Message}", "OK");
        }
    }


    private void ReloadUI()
    {
        CheckIfSpellIsKnown();
        UpdateButtons();      
        OnPropertyChanged(nameof(character.SpellsUsedToday));
        OnPropertyChanged(nameof(character.MaxSpellsPerDay));
    }

    private async void OnAddSpellClicked(object? sender, EventArgs eventArgs)
    {
        try
        {
            var viewModel = SharedViewModel.Instance;
            // Ensure character has an ID
            if (character.ID == null) character.ID = Guid.NewGuid();

            viewModel.AddSpell(character, spell);
            viewModel.SaveSpellForCharacter(character, spell);
            CheckIfSpellIsKnown();
            UpdateButtons();

            // Notify other UI to reload lists
            SharedViewModel.Instance.SpellsChanged?.Invoke();

            await Navigation.PopAsync();
        }
        catch (Exception e)
        {
            await DisplayAlert("Error", $"An error occurred while adding the spell: {e.Message}", "OK");
        }
    }

    private async void OnRemoveSpellClicked(object? sender, EventArgs eventArgs)
    {
        try
        {
            var viewModel = SharedViewModel.Instance;
            // Ensure the character has an ID and the in-memory spell collection is loaded
            character.ID ??= Guid.NewGuid();
 
            // This will initialize CharacterSpells[character.ID] if missing
            var spellsForChar = viewModel.SpellsForCharacter(character);
 
            // Remove by matching Id or Name to handle different object instances
            var existing = spellsForChar.FirstOrDefault(s => s.Id == spell.Id) ?? spellsForChar.FirstOrDefault(s => s.Name == spell.Name);
            if (existing != null)
            {
                spellsForChar.Remove(existing);
            }
 
            // Ensure persistent removal and any divine-caster cleanup happens
            viewModel.RemoveSpellForCharacter(character, spell);
             CheckIfSpellIsKnown();
             UpdateButtons();

             // Notify other UI to reload lists
             SharedViewModel.Instance.SpellsChanged?.Invoke();

             await Navigation.PopAsync();
        }
        catch (Exception e)
        {
            await DisplayAlert("Error", $"An error occurred while removing the spell: {e.Message}", "OK");
        }
    }

    private async void OnPrepareSpellClicked(object? sender, EventArgs eventArgs)
    {
        try
        {
            var viewModel = SharedViewModel.Instance;

            // Prompt user whether this should be a regular prepare or a domain (always-prepared) save
            var choice = await DisplayActionSheet("Prepare Options", "Cancel", null, "Prepare Normally", "Mark as Domain (Always Prepared)");
            if (choice == "Cancel" || string.IsNullOrEmpty(choice)) return;

            // First, ensure the spell is in the character's known spells (so they can cast it)
            if (!viewModel.CharacterSpells.ContainsKey(character.ID))
            {
                viewModel.CharacterSpells[character.ID] = [];
            }
            if (viewModel.CharacterSpells[character.ID].All(s => s.Name != spell.Name))
            {
                viewModel.AddSpell(character, spell);
                viewModel.SaveSpellForCharacter(character, spell);
            }

            if (choice == "Mark as Domain (Always Prepared)")
            {
                // Mark as domain (always prepared) and persist immediately
                spell.IsAlwaysPrepared = true;
                if (!character.AlwaysPreparedSpells.Contains(spell.Name ?? string.Empty))
                    character.AlwaysPreparedSpells.Add(spell.Name ?? string.Empty);
                viewModel.SavePreparedSpells(character); // includes always-prepared save now
                await DisplayAlert("Success", $"'{spell.Name}' has been saved as a domain (always prepared) spell.", "OK");
            }
            
            CheckIfSpellIsPrepared();
            UpdateButtons();

            // Refresh lists
            SharedViewModel.Instance.SpellsChanged?.Invoke();

            await Navigation.PopAsync();
        }
        catch (Exception e)
        {
            await DisplayAlert("Error", $"An error occurred while preparing the spell: {e.Message}", "OK");
        }
    }

    private async void OnUnprepareSpellClicked(object? sender, EventArgs eventArgs)
    {
        try
        {
            var viewModel = SharedViewModel.Instance;

            // If the spell is marked as always-prepared (domain), ask whether to unmark domain or unprepare manually
            if (spell.IsAlwaysPrepared)
            {
                var choice = await DisplayActionSheet("This spell is marked as domain (always prepared).", "Cancel", null, "Unmark Domain");
                if (choice == "Cancel" || string.IsNullOrEmpty(choice)) return;
                if (choice == "Unmark Domain")
                {
                    spell.IsAlwaysPrepared = false;
                    if (character.AlwaysPreparedSpells.Contains(spell.Name ?? string.Empty))
                        character.AlwaysPreparedSpells.Remove(spell.Name ?? string.Empty);
                    viewModel.SavePreparedSpells(character);
                }
            }
            else
            {
                character.TogglePreparedSpell(spell);
                viewModel.SavePreparedSpells(character);
            }

            CheckIfSpellIsPrepared();
            UpdateButtons();

            // Refresh lists
            SharedViewModel.Instance.SpellsChanged?.Invoke();

            await Navigation.PopAsync();
        }
        catch (Exception e)
        {
            await DisplayAlert("Error", $"An error occurred while unpreparing the spell: {e.Message}", "OK");
        }
    }

    private async Task<Spell?> ShowReplacementDialog(List<Spell> preparedSpells)
    {
        var spellNames = preparedSpells.Select(s => s.Name).ToArray();
        var selectedSpellName = await DisplayActionSheet(
            "Prepared Spell Limit Reached",
            "Cancel",
            null,
            spellNames);

        if (selectedSpellName == "Cancel" || string.IsNullOrEmpty(selectedSpellName))
        {
            return null;
        }

        return preparedSpells.FirstOrDefault(s => s.Name == selectedSpellName);
    }

    private async void OnToggleAlwaysPreparedClicked(object? sender, EventArgs eventArgs)
    {
        try
        {
            var viewModel = SharedViewModel.Instance;
            
            // Check if spell is already marked as always prepared
            bool isCurrentlyAlwaysPrepared = spell.IsAlwaysPrepared || 
                                            character.AlwaysPreparedSpells.Contains(spell.Name ?? string.Empty);

            if (isCurrentlyAlwaysPrepared)
            {
                // Unmark as domain spell
                spell.IsAlwaysPrepared = false;
                if (character.AlwaysPreparedSpells.Contains(spell.Name ?? string.Empty))
                {
                    character.AlwaysPreparedSpells.Remove(spell.Name ?? string.Empty);
                }
                
                // IMPORTANT: Save the spell file with IsAlwaysPrepared flag cleared
                viewModel.SaveSpellForCharacter(character, spell);
                
                // Save the updated state
                viewModel.SavePreparedSpells(character);
                
                await DisplayAlert("Success", $"'{spell.Name}' is no longer marked as a domain spell.", "OK");
            }
            else
            {
                // Mark as domain spell (always prepared)
                spell.IsAlwaysPrepared = true;
                if (!character.AlwaysPreparedSpells.Contains(spell.Name ?? string.Empty))
                {
                    character.AlwaysPreparedSpells.Add(spell.Name ?? string.Empty);
                }
                
                // Ensure the spell is in the character's spell list
                if (!viewModel.CharacterSpells.ContainsKey(character.ID))
                {
                    viewModel.CharacterSpells[character.ID] = [];
                }
                if (viewModel.CharacterSpells[character.ID].All(s => s.Name != spell.Name))
                {
                    viewModel.AddSpell(character, spell);
                }
                
                // IMPORTANT: Save the spell file with IsAlwaysPrepared flag
                viewModel.SaveSpellForCharacter(character, spell);
                
                // Save the updated state
                viewModel.SavePreparedSpells(character);
                
                await DisplayAlert("Success", $"'{spell.Name}' has been marked as a domain spell (always prepared).", "OK");
            }
            
            // Refresh UI
            CheckIfSpellIsPrepared();
            CheckIfSpellIsKnown();
            UpdateButtons();
            
            // Notify other UI to reload lists
            SharedViewModel.Instance.SpellsChanged?.Invoke();
        }
        catch (Exception e)
        {
            await DisplayAlert("Error", $"An error occurred while toggling always prepared status: {e.Message}", "OK");
        }
    }

    // Async wrapper methods for the context menu (don't navigate away)
    private async Task OnAddSpellClickedAsync()
    {
        try
        {
            var viewModel = SharedViewModel.Instance;
            // Ensure character has an ID
            if (character.ID == null) character.ID = Guid.NewGuid();

            viewModel.AddSpell(character, spell);
            viewModel.SaveSpellForCharacter(character, spell);
            CheckIfSpellIsKnown();
            UpdateButtons();

            // Notify other UI to reload lists
            SharedViewModel.Instance.SpellsChanged?.Invoke();

            await DisplayAlert("Success", $"'{spell.Name}' has been added to your spells.", "OK");
        }
        catch (Exception e)
        {
            await DisplayAlert("Error", $"An error occurred while adding the spell: {e.Message}", "OK");
        }
    }

    private async Task OnRemoveSpellClickedAsync()
    {
        try
        {
            var viewModel = SharedViewModel.Instance;
            // Ensure the character has an ID and the in-memory spell collection is loaded
            character.ID ??= Guid.NewGuid();
 
            // This will initialize CharacterSpells[character.ID] if missing
            var spellsForChar = viewModel.SpellsForCharacter(character);
 
            // Remove by matching Id or Name to handle different object instances
            var existing = spellsForChar.FirstOrDefault(s => s.Id == spell.Id) ?? spellsForChar.FirstOrDefault(s => s.Name == spell.Name);
            if (existing != null)
            {
                spellsForChar.Remove(existing);
            }
 
            // Ensure persistent removal and any divine-caster cleanup happens
            viewModel.RemoveSpellForCharacter(character, spell);
            CheckIfSpellIsKnown();
            UpdateButtons();

            // Notify other UI to reload lists
            SharedViewModel.Instance.SpellsChanged?.Invoke();

            await DisplayAlert("Success", $"'{spell.Name}' has been removed from your spells.", "OK");
        }
        catch (Exception e)
        {
            await DisplayAlert("Error", $"An error occurred while removing the spell: {e.Message}", "OK");
        }
    }

    private async Task OnToggleAlwaysPreparedAsync(bool markAsDomain)
    {
        try
        {
            var viewModel = SharedViewModel.Instance;

            if (markAsDomain)
            {
                // Mark as domain spell (always prepared)
                spell.IsAlwaysPrepared = true;
                if (!character.AlwaysPreparedSpells.Contains(spell.Name ?? string.Empty))
                {
                    character.AlwaysPreparedSpells.Add(spell.Name ?? string.Empty);
                }
                
                // Ensure the spell is in the character's spell list
                if (!viewModel.CharacterSpells.ContainsKey(character.ID))
                {
                    viewModel.CharacterSpells[character.ID] = [];
                }
                if (viewModel.CharacterSpells[character.ID].All(s => s.Name != spell.Name))
                {
                    viewModel.AddSpell(character, spell);
                }
                
                // IMPORTANT: Save the spell file with IsAlwaysPrepared flag
                viewModel.SaveSpellForCharacter(character, spell);
                
                // Save the updated state
                viewModel.SavePreparedSpells(character);
                
                await DisplayAlert("Success", $"'{spell.Name}' has been marked as a domain spell (always prepared).", "OK");
            }
            else
            {
                // Unmark as domain spell
                spell.IsAlwaysPrepared = false;
                if (character.AlwaysPreparedSpells.Contains(spell.Name ?? string.Empty))
                {
                    character.AlwaysPreparedSpells.Remove(spell.Name ?? string.Empty);
                }
                
                // IMPORTANT: Save the spell file with IsAlwaysPrepared flag cleared
                viewModel.SaveSpellForCharacter(character, spell);
                
                // Save the updated state
                viewModel.SavePreparedSpells(character);
                
                await DisplayAlert("Success", $"'{spell.Name}' is no longer marked as a domain spell.", "OK");
            }
            
            // Refresh UI
            CheckIfSpellIsPrepared();
            CheckIfSpellIsKnown();
            UpdateButtons();
            
            // Notify other UI to reload lists
            SharedViewModel.Instance.SpellsChanged?.Invoke();
        }
        catch (Exception e)
        {
            await DisplayAlert("Error", $"An error occurred while toggling always prepared status: {e.Message}", "OK");
        }
    }
}
