using Shouldly;
using SENG302.Api.Services;
using SENG302.Api.Models.Entities;
using Microsoft.Extensions.DependencyInjection;
using SENG302.Api.Tests.Integration;


namespace SENG302.Api.Tests.Unit;

public class UserDBTest
{

    private IUserService ServiceUnderTest => ServiceProvider.GetRequiredService<IUserService>();

    [Fact]
    public void VerifyTrueHashTest()
    {
        var user = GenerateNewUserAsync("s@s.com", "me", "hellobob", "NZ");
        var unhashedPass = VerifyHashedPassword(User, user.PasswordKey, "hellobob");
        unhashedPass.ShouldBe(1);
    }

    [Fact]
    public void VerifyFalseHashtest()
    {
        var user = GenerateNewUserAsync("s@s.com", "me", "hellobob", "NZ");
        var unhashedPass = VerifyHashedPassword(User, user.PasswordKey, "goodbyebob");
        unhashedPass.ShouldBe(0);
    }
}