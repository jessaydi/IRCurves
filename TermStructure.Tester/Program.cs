using Microsoft.Extensions.Configuration;
using TermStructure;

class Program
{
    static async Task Main(string[] args)
    {
        var config = new ConfigurationBuilder().AddUserSecrets<Program>().Build();
        var apiKey = config["FRED_API_KEY"];
        if (string.IsNullOrEmpty(apiKey))        {
            Console.WriteLine("Please set your FRED API key in user secrets with the key 'FRED_API_KEY'.");
            return;
        }

        using var httpClient = new HttpClient();
        var fred = new FredDataFetcher(httpClient, apiKey);

        Console.WriteLine("Fetching latest 10-year Treasury yield...");
        var yield = await fred.GetLatestValueAsync("DGS10");

        if (yield.HasValue)
        {
            Console.WriteLine($"Latest 10-year Treasury yield: {yield.Value}%");
        }
        else
        {
            Console.WriteLine("No data available for the specified series.");
        }

        Console.WriteLine("\nFetching last 5 observations for 10-year Treasury yield...");
        var observations = await fred.GetSeriesObservationsAsync("DGS10", "desc", 5);

        foreach (var obs in observations)
        {
            Console.WriteLine($"Date: {obs.Date}, Value: {obs.Value}%");
        }
    }
}