Feature: Google Login Test

  # This test verifies the Google login functionality using Base64-encoded credentials.
  # The email and password are provided as valid Base64 strings to avoid format exceptions.

  Scenario Outline: Attempt to login using Google account and expect a warning after entering username
    Given I navigate to the Google login home page
    And I handle the Google login consent popup
    When I attempt to login with Google using email "<encodedEmail>" and password "<encodedPassword>"
    Then I should see a warning after entering the Google login username

  Examples:
    | encodedEmail                     | encodedPassword |
    | aW1pY2hhZWwxNDAzQGdtYWlsLmNvbQ== | Z2c=            | 
