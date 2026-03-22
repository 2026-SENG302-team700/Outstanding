using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using SENG302.Api.Models.Entities;
using SENG302.Api.Services;
using Shouldly;
using NSubstitute;

namespace SENG302.Api.Tests.Unit.Services;

public class OneTimeCodeServiceUnitTest : BaseUnitTestFixture
{
    private IOneTimeCodeService OneTimeCodeServiceUnderTest => ServiceProvider.GetRequiredService<IOneTimeCodeService>();

    public OneTimeCodeServiceUnitTest(WebApplicationFactory<Program> webAppFactory) : base(webAppFactory) { }
    
    [Fact]
    public void GetServerTime_ValidCall_ExpectUTCTimeSeconds()
    {
        long curTime = OneTimeCodeServiceUnderTest.GetEpochTime();
        
        Assert.True(curTime == DateTimeOffset.UtcNow.ToUnixTimeSeconds());
        Assert.True(curTime > 0);
    }

    [Fact]
    public void CompareTimes_DifferenceBelowTimeoutValue_ExpectTrue()
    {
        Assert.True(OneTimeCodeServiceUnderTest.CompareTimes(1774126459, 1774126758));
        Assert.True(OneTimeCodeServiceUnderTest.CompareTimes(1774126459, 1774126460));
    }
    
    [Fact]
    public void CompareTimes_DifferenceBelowTimeoutValue_ExpectFalse()
    {
        Assert.False(OneTimeCodeServiceUnderTest.CompareTimes(1774126459, 1774126760));
        Assert.False(OneTimeCodeServiceUnderTest.CompareTimes(1774126459, 1774126759));
    }




    [Fact]
    public void CompareCodes_SameCodes_ExpectTrue()
    {
        Assert.True(OneTimeCodeServiceUnderTest.CompareCodes("987456", "987456"));
        Assert.True(OneTimeCodeServiceUnderTest.CompareCodes("000111", "000111"));
    }
    
    [Fact]
    public void CompareCodes_DifferentCodes_ExpectFalse()
    {
        Assert.False(OneTimeCodeServiceUnderTest.CompareCodes("987456", "987546"));
        Assert.False(OneTimeCodeServiceUnderTest.CompareCodes("000111", "000123"));
    }
    
    [Fact]
    public void CompareCodes_InvalidLengthCodes_ExpectFalse()
    {
        Assert.False(OneTimeCodeServiceUnderTest.CompareCodes("9874561", "9875461"));
        Assert.False(OneTimeCodeServiceUnderTest.CompareCodes("00011123", "000111"));
    }

    [Fact]
    public void GenerateCodes_ValidCall_ExpectValidLength()
    {
        string code = OneTimeCodeServiceUnderTest.GenerateOneTimeCode();
        Assert.True(code.Length == 6);
    }
}