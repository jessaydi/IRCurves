Feature: End-to-end interest rate curve workflows
  As a portfolio manager
  I want to build yield curves from market data
  So that I can price instruments and evaluate economic signals

  Scenario: Complete workflow - Bootstrap curve from bond data
    Given I have a portfolio of bonds with varying maturities:
      | BondId  | MaturityYears | YieldToMaturity | AskPrice | CouponRate | CouponFrequency |
      | BOND001 | 1             | 0.0425          | 99.75    | 0.04       | 2               |
      | BOND002 | 3             | 0.0460          | 98.80    | 0.045      | 2               |
      | BOND003 | 5             | 0.0480          | 98.50    | 0.048      | 2               |
      | BOND004 | 10            | 0.0500          | 97.00    | 0.05       | 2               |
    When I bootstrap the zero-coupon yield curve from these bonds
    And I request interpolated yields at 2Y, 4Y, and 7Y
    Then I should receive a smoothly interpolated curve
    And the intermediate yields should remain smooth
    And I can use these rates for instrument pricing

  Scenario: Detect curve inversion
    Given I have interest rates where:
      | Maturity | Rate  |
      | 1        | 0.050 |
      | 5        | 0.048 |
      | 10       | 0.045 |
    When I build the yield curve
    Then the system should detect an inverted yield curve
    And short-term rates should be higher than long-term rates
    And this is a signal of potential economic recession

  Scenario: Price a bond using the constructed curve
    Given I have a built yield curve from market data:
      | Maturity | Rate  |
      | 1        | 0.025 |
      | 5        | 0.030 |
      | 10       | 0.035 |
    And I want to price a bond with:
      | Property         | Value           |
      | Coupon Rate      | 0.04            |
      | Time to Maturity | 7               |
      | Par Value        | 100             |
      | Coupon Frequency | 2               |
    When I use the curve to discount cash flows
    Then I should calculate the bond's present value
    And the bond price should reflect the current yield curve
    And the price should be different from par value

  Scenario: Densify a sparse bootstrapped curve
    Given I have a bootstrapped yield curve with sparse points:
      | Maturity | Rate  |
      | 1        | 0.025 |
      | 5        | 0.030 |
      | 10       | 0.035 |
    When I densify the curve with daily (0.00274 year) intervals
    Then I should receive approximately 3,285 yield points
    And each point should have a unique maturity
    And the curve should be continuous and smooth
