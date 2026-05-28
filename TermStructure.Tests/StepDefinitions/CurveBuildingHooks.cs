using System;
using TechTalk.SpecFlow;


namespace TermStructure.Tests.StepDefinitions
{
    /// <summary>
    /// SpecFlow hooks for BDD test setup and teardown.
    /// Runs before/after scenarios to initialize test context and clean up resources.
    /// </summary>
    [Binding]
    public class CurveBuildingHooks
    {
        private readonly ScenarioContext _scenarioContext;

        public CurveBuildingHooks(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext;
        }

        /// <summary>
        /// Runs before each scenario to initialize common test data and dependencies.
        /// </summary>
        [BeforeScenario]
        public void BeforeScenario()
        {
            // Initialize test data structures
            _scenarioContext["Initialized"] = true;

            // Log scenario start
            Console.WriteLine($"[Scenario Started] {_scenarioContext.ScenarioInfo.Title}");
        }

        /// <summary>
        /// Runs after each scenario to clean up resources and log results.
        /// </summary>
        [AfterScenario]
        public void AfterScenario()
        {
            // Clean up HTTP clients
            if (_scenarioContext.ContainsKey("HttpClient"))
            {
                var httpClient = _scenarioContext["HttpClient"] as System.Net.Http.HttpClient;
                httpClient?.Dispose();
            }

            // Log scenario end
            Console.WriteLine($"[Scenario Completed] {_scenarioContext.ScenarioInfo.Title}");
        }

        /// <summary>
        /// Runs after failed scenarios to capture debug information.
        /// </summary>
        [AfterScenario("Error")]
        public void AfterFailedScenario()
        {
            Console.WriteLine($"[Scenario Failed] {_scenarioContext.ScenarioInfo.Title}");
            
            if (_scenarioContext.ContainsKey("LastBuildException"))
            {
                var exception = _scenarioContext["LastBuildException"] as Exception;
                if (exception != null)
                {
                    Console.WriteLine($"Exception: {exception.Message}");
                }
            }
        }

        /// <summary>
        /// Runs before scenarios tagged with @Integration to set up API mocks.
        /// </summary>
        [BeforeScenario("@Integration")]
        public void BeforeIntegrationScenario()
        {
            _scenarioContext["IntegrationTest"] = true;
            Console.WriteLine("[Integration Test] Setting up mock API");
        }

        /// <summary>
        /// Runs before scenarios tagged with @Performance to initialize performance tracking.
        /// </summary>
        [BeforeScenario("@Performance")]
        public void BeforePerformanceScenario()
        {
            _scenarioContext["StartTime"] = DateTime.Now;
            Console.WriteLine("[Performance Test] Starting performance measurement");
        }

        /// <summary>
        /// Runs after scenarios tagged with @Performance to report timing.
        /// </summary>
        [AfterScenario("@Performance")]
        public void AfterPerformanceScenario()
        {
            if (_scenarioContext.ContainsKey("StartTime"))
            {
                var startTime = (DateTime)_scenarioContext["StartTime"];
                var duration = DateTime.Now - startTime;
                Console.WriteLine($"[Performance] Scenario completed in {duration.TotalMilliseconds}ms");
            }
        }
    }
}
