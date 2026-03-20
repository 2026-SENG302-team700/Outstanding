using Microsoft.AspNetCore.Identity;
using SENG302.Api.Models.Entities;
using SENG302.Api.Services;
using Shouldly;

namespace SENG302.Api.Tests.Unit;

public class ExampleTest
{
    [Fact]
    public Task Example()
    {
        var a = 1;
        a.ShouldBe(1);
    }
}