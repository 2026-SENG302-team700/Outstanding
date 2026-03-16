using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using SENG302.Api.Models.Requests;
using SENG302.Api.Models.Entities;
using SENG302.Api.Services;
using Shouldly;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

namespace SENG302.Api.Tests.Integration.ControllerTests;

public class UserControllerTests : BaseIntegrationTestFixture
{
    public UserControllerTests(WebApplicationFactory<Program> webApplicationFactory) : base(webApplicationFactory) { }

    private IUserService ServiceUnderTest => ServiceProvider.GetRequiredService<IUserService>();

    [Fact]
    public async Task UpdateUser_Success_ReturnOk()
    {
        // Add user to be updated to db
        await using var context = DbContextFactory.CreateDbContext();
        context.Users.Add(new User
        {
            Id = 1,
            Email = "test@example.com",
            DisplayName = "Test User",
            PasswordKey = "password",
            Country = "Test Country"
        });
        await context.SaveChangesAsync();

        // Send update request
        var data = new
        {
            Email = "updated@example.com",
            DisplayName = "Updated User",
            Country = "US"
        };
        var response = await HttpClient.PutAsJsonAsync("/api/user", data);

        // Get updated context and verify
        await using var verifyContext = DbContextFactory.CreateDbContext();

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var updatedUser = await verifyContext.Users.FirstOrDefaultAsync(u => u.Id == 1);
        updatedUser!.Email.ShouldBe("updated@example.com");
        updatedUser.DisplayName.ShouldBe("Updated User");
        updatedUser.Country.ShouldBe("US");
    }



}