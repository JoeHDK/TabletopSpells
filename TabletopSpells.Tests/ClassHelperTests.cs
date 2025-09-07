using TabletopSpells.Helpers;
using TabletopSpells.Models.Enums;
using Xunit;
using FluentAssertions;

namespace TabletopSpells.Tests;

public class ClassHelperTests
{
    [Theory]
    [InlineData(Class.Cleric, true)]
    [InlineData(Class.Druid, true)]
    [InlineData(Class.Wizard, false)]
    public void IsDivineCaster_Returns_Expected(Class characterClass, bool expected)
    {
        ClassHelper.IsDivineCaster(characterClass).Should().Be(expected);
    }
}

