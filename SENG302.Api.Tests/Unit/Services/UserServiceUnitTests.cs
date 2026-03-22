using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using SENG302.Api.Models.Entities;
using SENG302.Api.Services;
using Shouldly;
using NSubstitute;

namespace SENG302.Api.Tests.Unit.Services;

public class UserServiceUnitTests : BaseUnitTestFixture
{
    private IUserService UserService => ServiceProvider.GetRequiredService<IUserService>();

    public UserServiceUnitTests(WebApplicationFactory<Program> webAppFactory) : base(webAppFactory) { }

   
}