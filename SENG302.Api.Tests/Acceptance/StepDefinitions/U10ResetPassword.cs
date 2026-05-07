using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Reqnroll;
using SENG302.Api.Models.Entities;
using SENG302.Api.Tests.Acceptance.Setup;
using Shouldly;
using Microsoft.AspNetCore.Identity;
using NSubstitute;
using SENG302.Api.Services;

namespace SENG302.Api.Tests.Acceptance.StepDefinitions
{
    [Binding]
    public class StepDefinitions
    {
        private readonly AcceptanceTestFixture _fixture;
        private HttpResponseMessage? _lastResponse;

        public StepDefinitions(AcceptanceTestFixture fixture)
        {
            _fixture = fixture;
        }

        // AC 2
        [Given("I am on forgot password form and have a registered account")]
        public async Task GivenIAmOnForgotPasswordFormAndHaveARegisteredAccount()
        {
            await using var context = await _fixture.DbContextFactory.CreateDbContextAsync();

            User user = new User
            {
                Email = "test@example.com",
                Country = "NZ",
                DisplayName = "Test",
                PasswordKey = "Team700!",
            };

            context.Add(user);
            context.SaveChanges();

        }

        // AC 8
        [Given(@"I am a registered user with valid credentials")]
        public async Task GivenIAmARegisteredUserWithValidCredentials()
        {
            await using var context = await _fixture.DbContextFactory.CreateDbContextAsync();
            PasswordHasher<User> passwordHasher = new();
            var user = new User
            {
                Id = 2,
                Email = "login@example.com",
                DisplayName = "Login User",
                Country = "NZ",
                EmailVerified = true,
                TimeCreated = DateTime.UtcNow
            };
            user.PasswordKey = passwordHasher.HashPassword(user, "Team700!");
            context.Users.Add(user);
            await context.SaveChangesAsync();
            _fixture.CurrentUserId = 2;
        }

        [Given(@"I have requested a password reset")]
        public async Task GivenIHaveRequestedAPasswordReset()
        {
            var response = await _fixture.HttpClient.PostAsJsonAsync(
                "/api/user/password/reset/code/generation", new
                {
                    Email = "login@example.com"
                });
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
        }

        [Given(@"I have not requested a password reset")]
        public void GivenIHaveNotRequestedAPasswordReset()
        {
            // no setup needed
        }

        [When(@"I login with valid credentials")]
        public async Task WhenILoginWithValidCredentials()
        {
            _lastResponse = await _fixture.HttpClient.PostAsJsonAsync("/api/login", new
            {
                Email = "login@example.com",
                PasswordString = "Team700!"
            });
        }

        [Then(@"I am logged in successfully")]
        public void ThenIAmLoggedInSuccessfully()
        {
            _lastResponse.ShouldNotBeNull();
            _lastResponse!.StatusCode.ShouldBe(HttpStatusCode.OK);
        }

        [Then(@"a warning email is sent to me that the reset was cancelled")]
        public async Task ThenAWarningEmailIsSentToMeThatTheResetWasCancelled()
        {
            await _fixture.EmailMock
                .Received(1)
                .SendEmailAsync(
                    "login@example.com",
                    EmailTemplate.ResetCancelledWarning,
                    Arg.Any<Dictionary<string, string>>()
                );
        }

        [Then(@"no warning email is sent to me")]
        public async Task ThenNoWarningEmailIsSentToMe()
        {
            await _fixture.EmailMock
                .DidNotReceive()
                .SendEmailAsync(
                    "login@example.com",
                    EmailTemplate.ResetCancelledWarning,
                    Arg.Any<Dictionary<string, string>>()
                );
        }

        [When("I click send")]
        public async Task WhenIClickSend()
        {
            _lastResponse = await _fixture.HttpClient.PutAsJsonAsync("/api/user/password/code/generation",
                new { Email = "test@example.com" });

        }

        [Then("I am send a one time code")]
        public void ThenIAmSendAOneTimeCode()
        {
            _lastResponse.ShouldNotBeNull();
            _lastResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        }
    }
}