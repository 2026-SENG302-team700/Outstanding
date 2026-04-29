using System;
using System.ComponentModel.DataAnnotations;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Reqnroll;
using SENG302.Api.Models.Entities;
using SENG302.Api.Models.Requests;
using SENG302.Api.Services;
using SENG302.Api.Tests.Acceptance.Setup;
using Shouldly;

namespace MyNamespace
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
        
        // AC 1
        [Given("I am on the log in page")]
        public void GivenIAmOnTheLogInPage()
        {
        }
              
        [When("I click forgot password")]
        public void WhenIClickForgotPassword()
        {
            throw new PendingStepException();
        }
              
        [Then("I am taken to the forgot password form")]
        public void ThenIAmTakenToTheForgotPasswordForm()
        {
            throw new PendingStepException();
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
            
        }
              
        [When("I click Send")]
        public async void WhenIClickSend(string send0)
        {
            _lastResponse =  await _fixture.HttpClient.PostAsJsonAsync("/api/user/password/code/generation", new NewOneTimeCodeRequest {Email = "test@example.com", ResendingCode = false});
            
        }
              
        [Then("I am send a one time code")]
        public void ThenIAmSendAOneTimeCode()
        {
            _lastResponse.ShouldNotBeNull();
            _lastResponse.IsSuccessStatusCode.ShouldBeTrue();
        }
        
        // AC 3
        [Given("I have a valid one time code")]
        public void GivenIHaveAValidOneTimeCode()
        {
            throw new PendingStepException();
        }
              
        [When("I enter the valid one time code")]
        public void WhenIEnterTheValidOneTimeCode()
        {
            throw new PendingStepException();
        }
              
        [Then("I am taken to the reset password form")]
        public void ThenIAmTakenToTheResetPasswordForm()
        {
            throw new PendingStepException();
        }
    }
}