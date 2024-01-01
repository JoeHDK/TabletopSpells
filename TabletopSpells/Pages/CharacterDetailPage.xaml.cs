using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using TabletopSpells.Models;

namespace TabletopSpells.Pages
{
    public partial class CharacterDetailPage : ContentPage
    {
        private readonly Character character;

        private string CharacterName
        {
            get; set;
        }
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            MessagingCenter.Unsubscribe<SpellListPage, Spell>(this, "AddSpellToCharacter");
        }

        [Obsolete]
        public CharacterDetailPage(Character character)
        {
            InitializeComponent();
            this.character = character;
            this.Title = $"{character.Name}'s Details";

            //Spells = new ObservableCollection<string>();

            MessagingCenter.Subscribe<SpellListPage, Spell>(this, "AddSpellToCharacter", (sender, spell) =>
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    // Logic to add the spell to the character's list
                    character.AddSpell(spell);
                    //Spells.Add(spell.Name);
                });
            });
        }

        private async void OnDeleteCharacterClicked(object sender, EventArgs e)
        {
            // Display a confirmation dialog
            bool deleteConfirmed = await DisplayAlert(
                "Confirm Delete",
                $"Are you sure you want to delete {CharacterName}?",
                "Yes",
                "No"
            );

            // If the user confirmed, proceed with the deletion
            if (deleteConfirmed)
            {
                DeleteCharacter(CharacterName);

                // Optionally, navigate back to the main character list after deletion
                await Navigation.PopAsync();
            }
        }

        private void DeleteCharacter(string characterName)
        {
            // Retrieve the current list of characters
            string existingCharactersJson = Preferences.Get("characters", "[]");
            var characters = JsonConvert.DeserializeObject<List<string>>(existingCharactersJson);

            // Remove the selected character
            characters.Remove(characterName);

            // Save the updated list
            string updatedCharactersJson = JsonConvert.SerializeObject(characters);
            Preferences.Set("characters", updatedCharactersJson);

            // Assuming you have some way to refresh the character list on the previous page
            // You might need to use MessagingCenter or an event to notify the MainPage to refresh
        }

        [Obsolete]
        private void OnAddSpellClicked(object sender, EventArgs e)
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                await Navigation.PushAsync(new SpellListPage());
            });
        }

        // Other methods for handling spell list interactions
    }
}
