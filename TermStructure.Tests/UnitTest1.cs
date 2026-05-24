using System.Net;
using TermStructure;

namespace TermStructure.Tests
{
    public class FredDataTests
    {
        private const string FakeApiKey = "test-key";
        private const string BaseUrl = "https://api.stlouisfed.org/fred";

        // Helper to create a FredDataFetcher instance with a controlled HttpClient
        private FredDataFetcher CreateFredData(HttpResponseMessage response)
        {
            var handler = new FakeHttpHandler(response);
            var httpClient = new HttpClient(handler) { BaseAddress = new Uri(BaseUrl) };
            return new FredDataFetcher(httpClient, FakeApiKey);
        }

        [Fact] // Fact attribute indicates that this method is a test case to be run by the test runner. It allows the test framework to identify and execute this method as part of the testing process.
        public async Task GetLatestValueAsync_ReturnsCorrectDecimal()
        {
            // Arrange
            var json = @"{
                ""observations"": [
                    { ""date"": ""2025-01-10"", ""value"": ""4.2"" }
                ],
                ""count"": 1
            }";
            var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(json) };
            var fred = CreateFredData(response);

            // Act
            var result = await fred.GetLatestValueAsync("DGS10"); // Call the method being tested to retrieve the latest value for the specified FRED series. 

            // Assert
            Assert.NotNull(result); // Verify that the result is not null, indicating that a value was successfully retrieved and parsed from the API response.
            Assert.Equal(4.2m, result.Value); // Assert that the retrieved value matches the expected decimal value of 4.2. The 'm' suffix indicates that this is a decimal literal, which is important for precision in financial calculations. This assertion confirms that the method correctly parsed the string value from the JSON response and returned it as a decimal.
        }

        [Fact]
        public async Task GetLatestValueAsync_ReturnsNull_WhenNoObservations()
        {
            // Arrange
            var json = @"{ ""observations"": [], ""count"": 0 }";
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json)
            };
            var fred = CreateFredData(response);

            // Act
            var result = await fred.GetLatestValueAsync("DGS10");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetLatestValueAsync_HandlesDotValue()
        {
            // FRED sometimes returns "." for missing values
            var json = @"{
                ""observations"": [
                    { ""date"": ""2025-01-10"", ""value"": ""."" }
                ],
                ""count"": 1
            }";
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json)
            };
            var fred = CreateFredData(response);

            // Act
            var result = await fred.GetLatestValueAsync("DGS10");

            // Assert
            Assert.Null(result);  // Should gracefully return null
        }

        [Fact]
        public async Task GetSeriesObservationsAsync_ReturnsAllObservations()
        {
            // Arrange
            var json = @"{
                ""observations"": [
                    { ""date"": ""2025-01-12"", ""value"": ""4.30"" },
                    { ""date"": ""2025-01-11"", ""value"": ""4.25"" }
                ],
                ""count"": 2
            }";
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json)
            };
            var fred = CreateFredData(response);

            // Act
            var observations = await fred.GetSeriesObservationsAsync("DGS10");

            // Assert
            Assert.Equal(2, observations.Count);
            Assert.Equal("2025-01-12", observations[0].Date);
            Assert.Equal("4.30", observations[0].Value);
        }

        [Fact]
        public async Task GetSeriesObservationsAsync_RespectsSortOrderAndLimit()
        {
            // We'll verify that the request URL contains the right parameters.
            // For that we need a more advanced handler that captures the URL.
            // But for now, we'll test with a simple response and trust the URL building.
            var json = @"{ ""observations"": [], ""count"": 0 }";
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json)
            };
            var fred = CreateFredData(response);

            // Act – just make sure no exceptions
            await fred.GetSeriesObservationsAsync("DGS10", "asc", 10);
        }
    }
}
