@forgot-password
Feature: U10 - As Tipene I want to be able to reset my password via email so that I can keep access to Outstanding if I lose my password.

Scenario Outline: AC.2 - Sending a code to be able to reset email
    Given I am on forgot password form and have a registered account
    When I click send
    Then I am send a one time code
