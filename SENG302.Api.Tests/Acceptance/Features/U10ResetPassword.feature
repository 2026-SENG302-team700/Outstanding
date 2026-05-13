@forgot-password
Feature: U10 - As Tipene I want to be able to reset my password via email so that I can keep access to Outstanding if I lose my password.

    Scenario Outline: AC.2 - Sending a code to be able to reset email
        Given I am on forgot password form and have a registered account
        When I click send
        Then I am send a one time code

    Scenario: AC8.1 - Login while a reset password has been requested cancels the reset
        Given I am a registered user with valid credentials
        And I have requested a password reset
        When I login with valid credentials
        Then I am logged in successfully
    # uncomment when the email template is complete
    # And a warning email is sent to me that the reset was cancelled

    Scenario: AC8.2 - Login without a pending reset does not send a warning email
        Given I am a registered user with valid credentials
        And I have not requested a password reset
        When I login with valid credentials
        Then I am logged in successfully
        And no warning email is sent to me