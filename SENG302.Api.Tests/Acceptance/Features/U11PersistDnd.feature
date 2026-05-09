Feature: U11 - As a Tipene I want to reorder tasks within a task list using drag and drop So that I can order and prioritise items according to my errands’ location.

    Scenario: AC1 - Reordering a task persists the new order
        Given I am a registered user
        Given I have a task list
        Given I have 3 tasks (Task 1, Task 2, Task 3) in order
        When I reorder Task 3 to the top of the list
        Then The tasks are returned in the order (Task 3, Task 1, Task 2)
        