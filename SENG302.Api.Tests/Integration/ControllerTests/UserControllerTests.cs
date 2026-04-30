using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using SENG302.Api.Models.Entities;
using SENG302.Api.Services;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using Shouldly;
using Microsoft.EntityFrameworkCore;
using SENG302.Api.Controllers;

namespace SENG302.Api.Tests.Integration.ControllerTests;

public class UserControllerTests : BaseIntegrationTestFixture
{
    private readonly IEmailService _mockEmailService;
    private readonly IOneTimeCodeService _mockOneTimeCodeService;
    private readonly IFileService _mockFileService;
    private readonly UserController _controller;

    private IUserService ServiceUnderTest => ServiceProvider.GetRequiredService<IUserService>();
    public UserControllerTests(WebApplicationFactory<Program> webApplicationFactory) : base(webApplicationFactory)
    {
        _mockOneTimeCodeService = Substitute.For<IOneTimeCodeService>();
        _mockEmailService = Substitute.For<IEmailService>();
        _mockFileService = Substitute.For<IFileService>();
        _controller = new UserController(ServiceUnderTest,_mockFileService, _mockOneTimeCodeService, _mockEmailService);
    }
    

    private async Task AddTestUser()
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
    }
    
    private void SetupUserContext(string userId, string name, string email)
    {
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId), new Claim(ClaimTypes.Name, name), new Claim(ClaimTypes.Email, email) };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
    }
    
    private void SetupUserContext2(string userId, string name)
    {
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId), new Claim(ClaimTypes.Name, name)};
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
    }
    
    

    [Fact]
    public async Task UpdateUser_Success_ReturnOk()
    {
        await AddTestUser();

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

    [Theory]
    [InlineData("", "test", "NZ")]
    [InlineData("updated@example.com", "", "NZ")]
    [InlineData("updated@example.com", "test", "")]
    public async Task UpdateUser_MissingField_BadRequest(string newEmail, string newDisplayName, string newCountry)
    {
        await AddTestUser();

        // Send update request
        var data = new
        {
            Email = newEmail,
            DisplayName = newDisplayName,
            Country = newCountry
        };
        var response = await HttpClient.PutAsJsonAsync("/api/user", data);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData("test.userexample.com")]      // Missing @ symbol
    [InlineData("@example.com")]              // Missing username
    [InlineData("test.user@")]                // Missing domain
    [InlineData("test.user@example")]         // Missing top-level domain (TLD)
    [InlineData("t st.user@example.com")]     // Spaces
    [InlineData("te..st.user@example.com")]   // Consecutive periods
    [InlineData("test@user@example.com")]     // Multiple @ symbols
    [InlineData("test.user@gmail,com")]       // Missing dot in domain
    public async Task UpdateUser_MalformedEmail_BadRequest(string newEmail)
    {
        await AddTestUser();

        // Send update request
        var data = new
        {
            Email = newEmail,
            DisplayName = "Test",
            Country = "NZ"
        };
        var response = await HttpClient.PutAsJsonAsync("/api/user", data);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateUser_EmailAlreadyExists_BadRequest()
    {
        await AddTestUser();

        // Add user to be updated to db
        await using var context = DbContextFactory.CreateDbContext();
        context.Users.Add(new User
        {
            Id = 2,
            Email = "update@example.com",
            DisplayName = "Test User",
            PasswordKey = "password",
            Country = "Test Country"
        });
        await context.SaveChangesAsync();

        // Send update request
        var data = new
        {
            Email = "update@example.com",
            DisplayName = "Test",
            Country = "NZ"
        };
        var response = await HttpClient.PutAsJsonAsync("/api/user", data);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData("Hi")]
    [InlineData("Hi!")]
    [InlineData("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa")] // 65 'a's
    public async Task UpdateUser_InvalidDisplayName_BadRequest(string newDisplayName)
    {
        await AddTestUser();

        // Send update request
        var data = new
        {
            Email = "update@example.com",
            DisplayName = newDisplayName,
            Country = "NZ"
        };
        var response = await HttpClient.PutAsJsonAsync("/api/user", data);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

    }
    
    [Fact]
    public async Task InitiateOneTimeCode_ValidEmail_UpdatesDatabaseAndReturnsOk()
    {
        await AddTestUser();
        var request = new { Email = "test@example.com" };

        var response = await HttpClient.PutAsJsonAsync("/api/user/password/code/generation", request);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        await using var verifyContext = DbContextFactory.CreateDbContext();
        var updatedUser = await verifyContext.Users.FirstOrDefaultAsync(u => u.Email == "test@example.com");
        
        updatedUser.ShouldNotBeNull();
        updatedUser.OneTimeCode.ShouldNotBeNull();
        updatedUser.OneTimeCode.Length.ShouldBe(6);
    }
    
    [Fact]
    public async Task InitiateOneTimeCode_UserNotFound_NotFound()
    {
        var request = new { Email = "nonexistent@example.com" };

        var response = await HttpClient.PutAsJsonAsync("/api/user/password/code/generation", request);
        
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
    
    [Fact]
    public async Task ValidateOneTimeCode_CorrectCode_ReturnsOk()
    {
        var email = "test@example.com";
        var secretCode = "123456";
        
        await using (var context = DbContextFactory.CreateDbContext())
        {
            context.Users.Add(new User
            {
                Email = email,
                DisplayName = "ValidateUser",
                PasswordKey = "password",
                Country = "NZ",
                OneTimeCode = secretCode
            });
            await context.SaveChangesAsync();
        }
        var request = new { Email = email, Code = secretCode, TimeLimitExists = false, };

        var response = await HttpClient.PostAsJsonAsync("/api/user/password/code/validation", request);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }
    
    [Fact]
    public async Task ValidateOneTimeCode_WrongCode_ReturnsBadRequest()
    {
        var email = "test@example.com";
        await using (var context = DbContextFactory.CreateDbContext())
        {
            context.Users.Add(new User
            {
                Email = email,
                DisplayName = "ValidateUser",
                PasswordKey = "password",
                Country = "NZ",
                OneTimeCode = "111111" // stored code
            });
            await context.SaveChangesAsync();
        }

        var request = new { Email = email, Code = "222222" }; // wrong code

        var response = await HttpClient.PostAsJsonAsync("/api/user/password/code/validation", request);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task ValidateOneTimeCode_EmailNotFound_ReturnsNotFound()
    {
        var request = new { Email = "missing@test.com", Code = "123456" };

        var response = await HttpClient.PostAsJsonAsync("/api/user/password/code/validation", request);
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
    
    

    [Fact]
    public async Task ValidateUpdatedPassword_PasswordMismatch_ReturnsBadRequest()
    {
        var email = "test@example.com";
        string password = "password";
        string displayName = "Test User";
        string oneTimeCode = "111111";
        
        SetupUserContext("1", displayName, email);
        
        
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
        
        var emailDictionary = new Dictionary<string, string>
        {
            {"DISPLAY_NAME", displayName},
            {"CODE", oneTimeCode},
            {"MINUTES", "5"}
        };

        UpdatePasswordRequest request = new UpdatePasswordRequest { OldPassword = password, NewPassword = "Newpassword@2003", NewPasswordConfirm = "NewPasswordd@2004" };

        _mockEmailService.SendEmailAsync(email, EmailTemplate.VerifyEmailCode, emailDictionary).Returns(Task.CompletedTask);;

        var response = await _controller.updatePassword(request);
        response.ShouldBeOfType<BadRequestObjectResult>();
        BadRequestObjectResult responseObject = (BadRequestObjectResult)response;
        // responseObject.Value.ShouldBe("");
    }
    
    [Fact]
    public async Task ValidateUpdatedPassword_InvalidPassword_ReturnsBadRequest()
    {
        var email = "test@example.com";
        string password = "password";
        string displayName = "Test User";
        string oneTimeCode = "111111";
        
        SetupUserContext("1", displayName, email);
        
        
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
        
        var emailDictionary = new Dictionary<string, string>
        {
            {"DISPLAY_NAME", displayName},
            {"CODE", oneTimeCode},
            {"MINUTES", "5"}
        };

        UpdatePasswordRequest request = new UpdatePasswordRequest() { OldPassword = password, NewPassword = "newpassword", NewPasswordConfirm = "newpassword" };

        _mockEmailService.SendEmailAsync(email, EmailTemplate.VerifyEmailCode, emailDictionary).Returns(Task.CompletedTask);;

        var response = await _controller.updatePassword(request);
        response.ShouldBeOfType<BadRequestObjectResult>();
    }
    
    [Fact]
    public async Task ValidateUpdatedPassword_ValidPassword_ReturnsOKResponse()
    {
        var email = "test@example.com";
        string password = "password";
        string displayName = "Test User";
        string oneTimeCode = "111111";
        
        SetupUserContext("1", displayName, email);
        
        
        await using var context = DbContextFactory.CreateDbContext();
        var user = new User
        {
            Id = 1,
            Email = "test@example.com",
            DisplayName = "Test User",
            Country = "Test Country"
        };
        PasswordHasher<User> passwordHasher = new();
        var passwordKey = passwordHasher.HashPassword(user, password);
        user.PasswordKey = passwordKey;
        context.Users.Add(user);

        await context.SaveChangesAsync();
        
        var emailDictionary = new Dictionary<string, string>
        {
            {"DISPLAY_NAME", displayName},
            {"CODE", oneTimeCode},
            {"MINUTES", "5"}
        };

        UpdatePasswordRequest request = new UpdatePasswordRequest() { OldPassword = password, NewPassword = "NewPassword@1999", NewPasswordConfirm = "NewPassword@1999" };

        _mockEmailService.SendEmailAsync(email, EmailTemplate.VerifyEmailCode, emailDictionary).Returns(Task.CompletedTask);;

        var response = await _controller.updatePassword(request);
        response.ShouldBeOfType<OkResult>();
    }
    
    [Fact]
    public async Task ValidateUpdatedPassword_UnauthorizedUser_ReturnsUnauthorizedResponse()
    {
        var email = "test@example.com";
        string password = "password";
        string displayName = "Test User";
        string oneTimeCode = "111111";

        SetupUserContext2("1", displayName);
        
        var emailDictionary = new Dictionary<string, string>
        {
            {"DISPLAY_NAME", displayName},
            {"CODE", oneTimeCode},
            {"MINUTES", "5"}
        };

        UpdatePasswordRequest request = new UpdatePasswordRequest() { OldPassword = password, NewPassword = "NewPassword@1999", NewPasswordConfirm = "NewPassword@1999" };

        _mockEmailService.SendEmailAsync(email, EmailTemplate.VerifyEmailCode, emailDictionary).Returns(Task.CompletedTask);;

        var response = await _controller.updatePassword(request);
        response.ShouldBeOfType<UnauthorizedObjectResult>();
    }
}