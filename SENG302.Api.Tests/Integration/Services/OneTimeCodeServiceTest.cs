using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using SENG302.Api.Models.Entities;
using SENG302.Api.Services;
using Shouldly;

namespace SENG302.Api.Tests.Integration.Services;

public class OneTimeCodeServiceTest : BaseIntegrationTestFixture
{
    private IOneTimeCodeService ServiceUnderTest => ServiceProvider.GetRequiredService<IOneTimeCodeService>();
    
    public OneTimeCodeServiceTest(WebApplicationFactory<Program> webAppFactory) : base(webAppFactory) { }
    
    [Fact]
    public void VerifyCode_ValidCall_ExpectSuccessfulVerification()
    {
        long generationTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - 20;
        User user = new User
        {
            Email = "Test@example.com", DisplayName = "Tester", PasswordKey = "dsdgshdfhf", Country = "New Zealand",
            CodeGenerationTime = generationTime, OneTimeCode = "333222"
        };
        Assert.True(ServiceUnderTest.VerfiyCode(user, ServiceUnderTest.GetEpochTime(),user.OneTimeCode) == CodeVerificationResult.CodeSuccessful);
    }
    
    [Fact]
    public void VerifyCode_ExpiredCode_ExpectCodeIncorrect()
    {
        long generationTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - 20;
        User user = new User
        {
            Email = "Test@example.com", DisplayName = "Tester", PasswordKey = "dsdgshdfhf", Country = "New Zealand",
            CodeGenerationTime = generationTime, OneTimeCode = "333222"
        };
        Assert.True(ServiceUnderTest.VerfiyCode(user, ServiceUnderTest.GetEpochTime(), "111111") == CodeVerificationResult.CodeIncorrect);
    }
    
    [Fact]
    public void VerifyCode_ExpiredCode_ExpectCodeExpired()
    {
        long generationTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - 320;
        User user = new User
        {
            Email = "Test@example.com", DisplayName = "Tester", PasswordKey = "dsdgshdfhf", Country = "New Zealand",
            CodeGenerationTime = generationTime, OneTimeCode = "333222"
        };
        Assert.True(ServiceUnderTest.VerfiyCode(user, ServiceUnderTest.GetEpochTime(), user.OneTimeCode) == CodeVerificationResult.CodeExpired);
    }
}