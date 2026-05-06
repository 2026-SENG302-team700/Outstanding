using SENG302.Api.Models.Entities;
using SENG302.Api.Resources.Helpers;
using Shouldly;

namespace SENG302.Api.Tests.Unit.Services;

public class ProfanityToolsTests
{
    private ProfanityTools _toolUnderTest = new();

    [Theory]
    [InlineData("Crap")]
    [InlineData("crap")]
    [InlineData("I am shitting")]
    [InlineData("You are an arse")] //don't take it personally
    public void ContainsProfanity_ProfaneText_FilterOn_ReturnTrue(string text)
    {
        User user = new User
        {
            Email = "bob@bob.bob",
            Country = "Mars",
            DisplayName = "Bob",
            ProfanityFiltering = true, //filter on
        };
        var result = _toolUnderTest.ContainsProfanity(text, user);
        result.ShouldBeTrue();
    }

    [Theory]
    [InlineData("Crap")]
    [InlineData("crap")]
    [InlineData("I am crapping")]
    [InlineData("You are an arse")]
    public void ContainsProfanity_ProfaneText_FilterOff_ReturnFalse(string text)
    {
        User user = new User
        {
            Email = "bob@bob.bob",
            Country = "Mars",
            DisplayName = "Bob",
            ProfanityFiltering = false, //filter off
        };
        var result = _toolUnderTest.ContainsProfanity(text, user);
        result.ShouldBeFalse();
    }

    [Theory]
    [InlineData("Hello!")]
    [InlineData("I am an innocent little English speaker")]
    [InlineData("I cannot curse for the life of me")]
    [InlineData("You are a lovely person!")]
    public void ContainsProfanity_HarmlessText_FilterOff_ReturnFalse(string text)
    {
        User user = new User
        {
            Email = "bob@bob.bob",
            Country = "Mars",
            DisplayName = "Bob",
            ProfanityFiltering = false, //filter off
        };
        var result = _toolUnderTest.ContainsProfanity(text, user);
        result.ShouldBeFalse();
    }

    [Theory]
    [InlineData("Hello!")]
    [InlineData("I am an innocent little English speaker")]
    [InlineData("I cannot curse for the life of me")]
    [InlineData("You are a lovely person!")]
    public void ContainsProfanity_HarmlessText_FilterOn_ReturnFalse(string text)
    {
        User user = new User
        {
            Email = "bob@bob.bob",
            Country = "Mars",
            DisplayName = "Bob",
            ProfanityFiltering = true, //filter on
        };
        var result = _toolUnderTest.ContainsProfanity(text, user);
        result.ShouldBeFalse();
    }
}
