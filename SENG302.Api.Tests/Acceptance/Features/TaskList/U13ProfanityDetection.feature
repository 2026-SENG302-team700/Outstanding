Feature: U13 - As Sarah I want task list titles to be moderated so that I can rest easy knowing that they are appropriate

    Background:
        Given I am a registered user

    Scenario Outline: AC3.1 - Cannot create a task list with profanity when profanity filter is enabled
        Given I have the profanity filter enabled
        And I am on the create task list form
        When I create a task list with the name <inappropriate-name>
        Then I should receive a bad request response
        And the error message should say "List name cannot contain profanity."

        Examples:
            | inappropriate-name |
            | shit list          |
            | fuck list          |
            | damn               |

    Scenario Outline: AC3.2 - Can create a task list with profanity when profanity filter is false
        Given I have the profanity filter disabled
        And I am on the create task list form
        When I create a task list with the name <inappropriate-name>
        Then The task list should be created

        Examples:
            | inappropriate-name |
            | shit list          |
            | fuck list          |
            | damn               |