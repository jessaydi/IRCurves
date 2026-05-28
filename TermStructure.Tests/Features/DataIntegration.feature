Feature: Integrating market data from FRED API
  As a data analyst
  I want to fetch interest rate data from the FRED API
  So that I can build accurate yield curves with real market data

  Background:
    Given the FRED API service is configured
    And the API key is valid
    And the HTTP client is properly initialized

  Scenario: Fetch latest interest rate observation from FRED
    Given the FRED service has data for series "DGS10" (10-Year Treasury Rate)
    And the latest observation has a value of 3.25 percent
    When I request the latest value for series DGS10
    Then the service should return a decimal value
    And the returned value should be approximately 0.0325
    And the observation should be recent (within 1 month)

  Scenario: Fetch multiple observations for a treasury series
    Given the FRED service has observations for series "DFF" (Effective Federal Funds Rate)
    And there are at least 10 observations in the database
    When I request the series observations with limit 10
    Then the result should contain exactly 10 observations
    And each observation should have a valid date and value
    And observations should be sorted in descending order by date

  Scenario: Build yield curve from multiple FRED treasury series
    Given I fetch data from the following FRED treasury series:
      | SeriesId | ExpectedTenor | Description         |
      | DGS1     | 1Y            | 1-Year Treasury     |
      | DGS5     | 5Y            | 5-Year Treasury     |
      | DGS10    | 10Y           | 10-Year Treasury    |
    And each series returns a valid latest observation
    When I aggregate the observations into interest rate data
    And I build a yield curve using the linear strategy
    Then the curve should contain 3 points
    And the curve should represent the current treasury yield curve shape
    And the curve should respect the standard upward-sloping treasury curve

  Scenario: Convert FRED observations to interest rate model
    Given the FRED API returns the following raw observation:
      | Field | Value           |
      | date  | 2026-05-27      |
      | value | 3.25            |
    When I convert the FRED observation to an InterestRate object
    Then the resulting InterestRate should have:
      | Property | Value    |
      | Currency | USD      |
      | RateType | FRED     |
      | Rate     | 0.0325   |
      | Tenor    | Unknown  |
    And the rate should be converted from percent to decimal format

  Scenario Outline: Fetch yield data for different treasury tenors
    Given the FRED API has data available for series "<series_id>"
    And the series represents the "<tenor>" Treasury yield
    When I fetch the latest observation for series "<series_id>"
    Then the returned value should represent a valid yield percentage
    And the value should be between 0.5% and 8% (realistic range)
    And I should be able to parse it as a decimal

    Examples:
      | series_id | tenor |
      | DGS3MO    | 3M    |
      | DGS6MO    | 6M    |
      | DGS1      | 1Y    |
      | DGS2      | 2Y    |
      | DGS5      | 5Y    |
      | DGS10     | 10Y   |
      | DGS30     | 30Y   |

  Scenario: Handle FRED API rate limiting gracefully
    Given the FRED API has rate limiting enabled (60 requests per minute)
    When I make multiple requests in rapid succession
    Then the service should respect HTTP status codes
    And 429 (Too Many Requests) responses should be handled appropriately
    And the client should not crash or hang

  Scenario: Build curve from mixed data sources
    Given I have bond quotes from a market data feed:
      | Maturity | YTM    |
      | 1        | 0.025  |
      | 5        | 0.030  |
    And I have interest rate observations from FRED:
      | Tenor | Rate |
      | 10Y   | 0.035 |
    When I merge the data sources
    And I build a unified yield curve
    Then the resulting curve should contain data from both sources
    And short-term points should come from bond quotes
    And longer-term points should come from FRED data

  Scenario: Validate FRED observation data quality
    Given the FRED API returns an observation with value "NOT_AVAILABLE"
    When I attempt to parse the value as a decimal
    Then the parsing should fail gracefully
    And the service should return null instead of throwing an exception
    And no corrupted yield points should be added to the curve

  Scenario: Cache FRED data to reduce API calls
    Given the FRED service is configured with caching enabled
    When I fetch data for series DGS10 for the first time
    Then the data should be retrieved from the FRED API
    And the result should be cached in memory
    When I request the same series DGS10 again within 1 hour
    Then the data should be retrieved from cache
    And no additional API call should be made
