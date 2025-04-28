Feature: Used Car Page Navigation

  Scenario: Navigate to Used Cars Page and Extract Popular Cars
    Given I have opened the home page
    And I have handled the consent popup
    When I navigate to the used cars page
    And I dismiss the notification popup
    And I navigate to the city "Q2hlbm5haQ==" # Ensure this is a valid Base-64 string
    Then I should be able to extract popular cars
