Feature: Accessibility Testing for HomePage

  As a user with accessibility needs,
  I want to ensure that the HomePage is accessible
  So that I can navigate and interact with it using a screen reader and keyboard.

  @Accessibility
  Scenario: Verify accessibility for the HomePage
    Given I navigate to the HomePage
    When I run the screen reader accessibility test on the HomePage
    Then the screen reader should correctly announce all elements on the HomePage
    And I should be able to navigate through all interactive elements on the HomePage using the keyboard
