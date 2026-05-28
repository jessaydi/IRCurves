Feature: Handle edge cases and error conditions
  As a system administrator
  I want the curve building system to handle errors gracefully
  So that data quality issues don't cause application failures

  Background:
    Given the system is initialized with error handling enabled

  Scenario: Handle empty bond collection
    Given I provide an empty list of bonds
    When I attempt to build a curve
    Then the system should return an empty yield point list
    And no exception should be thrown
    And the system should remain operational

  Scenario: Handle empty interest rate collection
    Given I provide an empty list of interest rates
    When I attempt to build a curve using the linear strategy
    Then the system should return an empty yield point list
    And the operation should complete without error
    And appropriate logging should occur

  Scenario: Reject invalid tenor formats
    Given I provide interest rates with malformed tenor strings:
      | Rate  | Tenor  |
      | 0.025 | INVALID |
      | 0.030 | XYZ     |
    When I attempt to parse these tenors
    Then the system should raise a FormatException
    And the error message should indicate the invalid tenor
    And no partial curve should be returned

  Scenario: Handle insufficient Nelson-Siegel observations
    Given I provide fewer than 3 interest rate observations:
      | Tenor | Rate  |
      | 1Y    | 0.025 |
      | 5Y    | 0.030 |
    When I attempt to build a curve using Nelson-Siegel strategy
    Then the system should raise an InvalidOperationException
    And the error message should state "Need at least 3 observations"

  Scenario: Handle interpolation beyond curve boundaries
    Given I have a yield curve with points at 1Y, 5Y, 10Y
    When I request interpolation at 15 years (beyond the curve)
    Then the system should return the rate at 10Y (last point)
    And no extrapolation should occur
    When I request interpolation at 0.5 years (before the curve)
    Then the system should return the rate at 1Y (first point)

  Scenario: Handle missing FRED API key
    Given the FRED service is initialized without an API key
    When I instantiate the FredDataFetcher
    Then the constructor should raise an ArgumentNullException
    And a clear error message should specify the missing key

  Scenario: Handle malformed FRED observations
    Given the FRED API returns observations with unparseable values:
      | Value           |
      | NOT_AVAILABLE   |
      | invalid_number  |
    When I attempt to parse these as decimals
    Then decimal.TryParse should fail
    And the service should return null
    And valid observations should still be processed
