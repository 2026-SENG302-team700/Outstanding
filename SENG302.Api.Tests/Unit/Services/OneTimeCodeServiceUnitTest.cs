using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using SENG302.Api.Models.Entities;
using SENG302.Api.Services;
using Shouldly;

namespace SENG302.Api.Tests.Unit.Services;

public class OneTimeCodeServiceUnitTest : BaseUnitTestFixture
{
    private IOneTimeCodeService OneTimeCodeServiceTest => ServiceProvider.GetRequiredService<IOneTimeCodeService>();

    public OneTimeCodeServiceUnitTest(WebApplicationFactory<Program> webAppFactory) : base(webAppFactory)
    {
        
    }
    
    [Fact]
    public void GetServerTime_ValidCall_ExpectUTCTime()
    {
        TimeSpan curTime = OneTimeCodeServiceTest.getTimerStartTime();
        
        Console.WriteLine(curTime);
        Assert.NotNull(curTime);
        
    }

}