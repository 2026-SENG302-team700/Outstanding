using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Reqnroll;
using SENG302.Api.Models.Entities;
using SENG302.Api.Tests.Acceptance.Setup;
using Shouldly;

namespace SENG302.Api.Tests.Acceptance.StepDefinitions
{
    [Binding]
    public class StepDefinitions
    {
        private readonly AcceptanceTestFixture _fixture;
        private HttpResponseMessage? _lastResponse;
        private DbContext _context;

        public StepDefinitions(AcceptanceTestFixture fixture)
        {
            _fixture = fixture;
            _context = _fixture.DbContextFactory.CreateDbContext();
        }

        // AC 2
        [Given("I am on forgot password form and have a registered account")]
        public void GivenIAmOnForgotPasswordFormAndHaveARegisteredAccount()
        {
            User user = new User
            {
                Email = "test@example.com",
                Country = "NZ",
                DisplayName = "Test",
                PasswordKey = "Team700!",
            };

            _context.Add(user);
            _context.SaveChanges();

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