Feature: U10 - As Tipene I want to be able to reset my password via email so that I can keep access to Outstanding if I lose my password.

Scenario: AC.1 - Clicking on forgot password brings up forgot password form
    Given I am on the log in page
    When I click forgot password
    Then I am taken to the forgot password form

Scenario Outline: AC.2 - Sending a code to be able to reset email
    Given I am on forgot password form and have a registered account
    When I click send
    Then I am send a one time code

Scenario Outline: AC.3 - Entering the code emailed to the user
    Given I have a valid one time code
    When I enter the valid one time code
    Then I am taken to the reset password form

Scenario Outline: AC.4

Scenario Outline: AC.5 

Scenario Outline: AC.6 

Scenario Outline: AC.7

Scenario Outline: AC.8 

Scenario Outline: AC.9 

Scenario Outline: AC.10 
 