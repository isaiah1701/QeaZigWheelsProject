Feature: Upcoming Honda Cruiser Bike Test

  # This test verifies navigation to upcoming Honda cruiser bikes
  # The manufacturer name is Base64 encoded for security
  
  Scenario: Navigate to Honda Cruiser Bikes
    Given I navigate to the home page
    And I handle the consent popup
    When I navigate to the New Bikes page
    And I navigate to see all upcoming bikes
    And I select the manufacturer "SG9uZGE="
    And I click the Cruiser button
    Then I should see the list of Honda Cruiser bikes
