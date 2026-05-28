Feature: Building interest rate curves with different strategies
  As a portfolio manager
  I want to build yield curves using different construction strategies
  So that I can price instruments and perform risk analysis with the most appropriate method

  Background:
    Given the interpolation strategy is linear interpolation
    And the system is ready to build curves

  Scenario: Build a yield curve using bootstrap strategy with valid bonds
    Given I have bond quotes with the following details:
      | BondId  | MaturityYears | YieldToMaturity | AskPrice | CouponRate | CouponFrequency |
      | BOND001 | 1             | 0.0425          | 99.75    | 0.04       | 2               |
      | BOND002 | 5             | 0.0480          | 98.50    | 0.045      | 2               |
      | BOND003 | 10            | 0.0500          | 97.00    | 0.05       | 2               |
    And I use the bootstrap curve building strategy
    When I build the yield curve from these bonds
    Then the curve should contain 3 yield points
    And the curve should be sorted by maturity in ascending order
    And the first point should have maturity 1 year
    And the last point should have maturity 10 years
    And all yields should be positive

  Scenario: Build a yield curve using linear strategy with interest rate data
    Given I have the following interest rate observations:
      | Tenor | Rate |
      | 1Y    | 2.50 |
      | 2Y    | 2.75 |
      | 5Y    | 3.00 |
      | 10Y   | 3.15 |
    And I use the linear curve building strategy
    When I build the yield curve from these rates
    Then the curve should contain 4 yield points
    And the curve should be sorted by maturity in ascending order
    And the point at 1 year should have rate approximately 0.0250
    And the point at 10 years should have rate approximately 0.0315

  Scenario: Build a yield curve using Nelson-Siegel strategy with calibration
    Given I have the following interest rate observations for Nelson-Siegel:
      | Tenor | Rate |
      | 1Y    | 2.50 |
      | 5Y    | 3.00 |
      | 10Y   | 3.15 |
      | 20Y   | 3.25 |
    And I use the Nelson-Siegel curve building strategy with lambda 0.5
    When I build the yield curve from these rates
    Then the curve should contain at least 50 yield points
    And the curve should be smooth and differentiable
    And the curve should respect the observed rates at the input tenors

  Scenario: Interpolate yields at intermediate maturities
    Given I have a yield curve with the following points:
      | Maturity | Rate  |
      | 1        | 0.025 |
      | 5        | 0.030 |
      | 10       | 0.035 |
    When I request the interpolated yield at maturity 3 years
    Then the interpolated rate should be approximately 0.0275
    And the rate should be between the rates at 1 year and 5 years

  Scenario: Densify a sparse curve into daily points
    Given I have a yield curve with 3 points from 1 to 10 years
    When I densify the curve with a step of 0.25 years (quarterly)
    Then the output curve should contain at least 37 points
    And all maturities should be evenly spaced
    And the first point should have maturity 1 year
    And the last point should have maturity 10 years

  Scenario: Get interpolated rate at arbitrary maturity
    Given I have built a yield curve from valid bonds using bootstrap
    And the curve has points at 1Y, 5Y, and 10Y
    When I request the interpolated yield at 7.5 years
    Then the system should return a decimal rate value
    And the rate should be between the 5Y and 10Y rates
    And the rate should be closer to the 10Y rate than the 1Y rate

  Scenario Outline: Build curves with different tenor combinations
    Given I have interest rate observations for the following tenors:
      | Tenor |
      | <tenor1> |
      | <tenor2> |
      | <tenor3> |
    And each tenor has an observed yield of <yield_value> percent
    And I use the linear curve building strategy
    When I build the yield curve
    Then the curve should contain <expected_points> yield points
    And all maturities should be in ascending order

    Examples:
      | tenor1 | tenor2 | tenor3 | yield_value | expected_points |
      | 1Y     | 5Y     | 10Y    | 3.0         | 3               |
      | 6M     | 1Y     | 2Y     | 2.5         | 3               |
      | 2Y     | 7Y     | 30Y    | 3.5         | 3               |
      | 3M     | 6M     | 1Y     | 2.0         | 3               |

  Scenario: Build curve respects bond maturity ordering
    Given I provide bonds in random order:
      | BondId  | MaturityYears | AskPrice |
      | BOND003 | 10            | 97.00    |
      | BOND001 | 1             | 99.75    |
      | BOND002 | 5             | 98.50    |
    And I use the bootstrap curve building strategy
    When I build the yield curve
    Then the output curve should automatically sort points by maturity ascending
    And the first point should have maturity 1 year
    And the middle point should have maturity 5 years
    And the last point should have maturity 10 years
