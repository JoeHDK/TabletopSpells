# Define the path to the JSON file
$inputPath = "C:\Users\Joe_H\source\repos\JoeHDK\TabletopSpells\TabletopSpells\Spells\Original JSON files\dnd 5e.json"
$outputPath = "C:\Users\Joe_H\source\repos\JoeHDK\TabletopSpells\TabletopSpells\Spells\dnd 5e.json"

# Read the JSON file
$jsonData = Get-Content -Path $inputPath -Raw | ConvertFrom-Json

# Function to extract saving throw from the description
function Extract-SavingThrow($description) {
    $savingThrows = @("Dexterity", "Wisdom", "Constitution", "Charisma", "Strength", "Intelligence")
    foreach ($throw in $savingThrows) {
        if ($description -match $throw) {
            return $throw
        }
    }
    return "none"
}

# Function to transform each spell entry
function Transform-Spell($spell) {
    # Creating spell level string in the desired format
    $spellLevelParts = @()
    foreach ($class in $spell.classes) {
        # Replace 'cantrip' with '0' for spell level
        $level = if ($spell.level -eq 'cantrip') {'0'} else {$spell.level}
        $spellLevelParts += "$class $level"
    }
    $formattedSpellLevel = $spellLevelParts -join ', '

    $description = $spell.description
    if ($spell.PSObject.Properties.Name -contains 'higher_levels') {
        $description += " " + $spell.higher_levels
    }
    if ($spell.ritual) {
        $ritualText = "A ritual spell can be cast following the normal rules for spellcasting, or the spell can be cast as a ritual. The ritual version of a spell takes 10 minutes longer to cast than normal. It also doesn’t expend a spell slot, which means the ritual version of a spell can’t be cast at a higher level."
        $description += " " + $ritualText
    }

    # Extract saving throw from description
    $savingThrow = Extract-SavingThrow $description

    # Creating the new spell object using an ordered dictionary
    $newSpell = [ordered]@{
        duration = $spell.duration
        components = $spell.components.raw
        saving_throw = $savingThrow
        school = $spell.school
        spell_level = $formattedSpellLevel
        name = $spell.name
        range = $spell.range
        description = $description
        source = "Derived" # Placeholder
        targets = "" # Placeholder
        casting_time = $spell.casting_time
        ritual = $spell.ritual
    }
    return $newSpell
}

# Apply the transformation to each spell
$transformedData = $jsonData | ForEach-Object { Transform-Spell $_ }

# Convert the transformed data back to JSON and save it with UTF-8 encoding
$transformedData | ConvertTo-Json -Depth 10 | Out-File -FilePath $outputPath -Encoding utf8

Write-Host "Transformation complete. Output saved to $outputPath"