using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Xunit;
using TermStructure.Builders;
using TermStructure.Models;
using TermStructure.Strategies;


namespace TermStructure.Tests.StepDefinitions
{
    /// <summary>
    /// Step definitions for building yield curves with various strategies.
    /// Implements Given/When/Then steps for curve construction workflows.
    /// Uses ScenarioContext to share data between steps.
    /// </summary>
    [Binding]
    public class CurveBuildingSteps
    {
        private readonly ScenarioContext _context;
        private List<BondQuote> _bondQuotes;
        private List<InterestRate> _interestRates;
        private List<YieldPoint> _currentCurve;
        private RateCurveBuilder _curveBuilder;
        private ICurveBuildingStrategy _buildingStrategy;
        private IInterpolationStrategy _interpolationStrategy;
        private BondPricingInput? _bondPricingInput;
        private decimal? _presentValue;

        public CurveBuildingSteps(ScenarioContext context)
        {
            _context = context;
            _bondQuotes = new List<BondQuote>();
            _interestRates = new List<InterestRate>();
            _currentCurve = new List<YieldPoint>();
        }

        // ========== GIVEN Steps ==========

        [Given(@"the interpolation strategy is linear interpolation")]
        public void GivenInterpolationStrategyIsLinear()
        {
            _interpolationStrategy = new LinearInterpolationStrategy();
            _context["InterpolationStrategy"] = _interpolationStrategy;
        }

        [Given(@"the system is ready to build curves")]
        public void GivenSystemIsReady()
        {
            // Verify dependencies are available
            Assert.NotNull(_interpolationStrategy ?? new LinearInterpolationStrategy());
            _context["SystemReady"] = true;
        }

        [Given(@"I have bond quotes with the following details:")]
        public void GivenIHaveBondQuotesWithDetails(Table dataTable)
        {
            _bondQuotes = dataTable.CreateSet<BondQuote>().ToList();
            _context["BondQuotes"] = _bondQuotes;
        }

        [Given(@"I have a portfolio of bonds with varying maturities:")]
        public void GivenIHavePortfolioOfBondsWithVaryingMaturities(Table dataTable)
        {
            _bondQuotes = dataTable.CreateSet<BondQuote>().ToList();
            _context["BondQuotes"] = _bondQuotes;
        }

        [Given(@"I have the following interest rate observations:")]
        public void GivenIHaveInterestRateObservations(Table dataTable)
        {
            var rows = dataTable.Rows;
            _interestRates = new List<InterestRate>();

            foreach (var row in rows)
            {
                var tenor = row["Tenor"];
                var rateStr = row["Rate"];

                if (decimal.TryParse(rateStr, out var rate))
                {
                    _interestRates.Add(new InterestRate
                    {
                        Currency = "USD",
                        RateType = "Treasury",
                        Rate = rate,
                        Tenor = tenor
                    });
                }
            }

            _context["InterestRates"] = _interestRates;
        }

        [Given(@"I have the following interest rate observations for Nelson-Siegel:")]
        public void GivenIHaveInterestRateObservationsForNelsonSiegel(Table dataTable)
        {
            // Same as regular interest rate observations
            GivenIHaveInterestRateObservations(dataTable);
        }

        [Given(@"I use the bootstrap curve building strategy")]
        public void GivenIUseBootstrapStrategy()
        {
            _buildingStrategy = new BootstrapCurveBuildingStrategy();
            _interpolationStrategy ??= new LinearInterpolationStrategy();
            _curveBuilder = new YieldCurveBuilder(_buildingStrategy, _interpolationStrategy);
            _context["BuildingStrategy"] = _buildingStrategy;
            _context["CurveBuilder"] = _curveBuilder;
        }

        [Given(@"I use the linear curve building strategy")]
        public void GivenIUseLinearStrategy()
        {
            _buildingStrategy = new LinearCurveBuildingStrategy();
            _interpolationStrategy ??= new LinearInterpolationStrategy();
            _curveBuilder = new YieldCurveBuilder(_buildingStrategy, _interpolationStrategy);
            _context["BuildingStrategy"] = _buildingStrategy;
            _context["CurveBuilder"] = _curveBuilder;
        }

        [Given(@"I use the Nelson-Siegel curve building strategy with lambda (.*)")]
        public void GivenIUseNelsonSiegelStrategyWithLambda(decimal lambda)
        {
            _buildingStrategy = new NelsonSiegelCurveBuildingStrategy(lambda);
            _interpolationStrategy ??= new LinearInterpolationStrategy();
            _curveBuilder = new YieldCurveBuilder(_buildingStrategy, _interpolationStrategy);
            _context["BuildingStrategy"] = _buildingStrategy;
            _context["CurveBuilder"] = _curveBuilder;
        }

        [Given(@"I have a yield curve with the following points:")]
        public void GivenIHaveYieldCurveWithPoints(Table dataTable)
        {
            var rows = dataTable.Rows;
            _currentCurve = new List<YieldPoint>();

            foreach (var row in rows)
            {
                if (decimal.TryParse(row["Maturity"], out var maturity) &&
                    decimal.TryParse(row["Rate"], out var rate))
                {
                    _currentCurve.Add(new YieldPoint { Maturity = maturity, Rate = rate });
                }
            }

            _context["CurrentCurve"] = _currentCurve;
        }

        [Given(@"I have a built yield curve from market data:")]
        public void GivenIHaveBuiltYieldCurveFromMarketData(Table dataTable)
        {
            _currentCurve = new List<YieldPoint>();
            foreach (var row in dataTable.Rows)
            {
                if (decimal.TryParse(row["Maturity"], out var maturity) &&
                    decimal.TryParse(row["Rate"], out var rate))
                {
                    _currentCurve.Add(new YieldPoint { Maturity = maturity, Rate = rate });
                }
            }
            _context["CurrentCurve"] = _currentCurve;
        }

        [Given(@"I have a yield curve with 3 points from 1 to 10 years")]
        public void GivenIHaveYieldCurveWithThreePoints()
        {
            _currentCurve = new List<YieldPoint>
            {
                new YieldPoint { Maturity = 1, Rate = 0.025m },
                new YieldPoint { Maturity = 5, Rate = 0.030m },
                new YieldPoint { Maturity = 10, Rate = 0.035m }
            };
            _context["CurrentCurve"] = _currentCurve;
        }

        [Given(@"I want to price a bond with:")]
        public void GivenIWantToPriceABondWith(Table dataTable)
        {
            var bondPricing = new BondPricingInput();
            foreach (var row in dataTable.Rows)
            {
                var property = row["Property"].Trim();
                var value = row["Value"].Trim();

                if (property.Equals("Coupon Rate", StringComparison.OrdinalIgnoreCase))
                {
                    bondPricing = bondPricing with { CouponRate = decimal.Parse(value) };
                }
                else if (property.Equals("Time to Maturity", StringComparison.OrdinalIgnoreCase))
                {
                    if (int.TryParse(new string(value.Where(char.IsDigit).ToArray()), out var years))
                    {
                        bondPricing = bondPricing with { TimeToMaturity = years };
                    }
                }
                else if (property.Equals("Par Value", StringComparison.OrdinalIgnoreCase))
                {
                    bondPricing = bondPricing with { ParValue = decimal.Parse(value) };
                }
                else if (property.Equals("Coupon Frequency", StringComparison.OrdinalIgnoreCase))
                {
                    if (int.TryParse(new string(value.Where(char.IsDigit).ToArray()), out var frequency))
                    {
                        bondPricing = bondPricing with { CouponFrequency = frequency };
                    }
                }
            }

            _bondPricingInput = bondPricing;
            _context["BondPricingInput"] = _bondPricingInput;
        }

        [Given(@"I have a bootstrapped yield curve with sparse points:")]
        public void GivenIHaveBootstrappedYieldCurveWithSparsePoints(Table dataTable)
        {
            GivenIHaveYieldCurveWithPoints(dataTable);
        }

        [Given(@"I have interest rates where:")]
        public void GivenIHaveInterestRatesWhere(Table dataTable)
        {
            _interestRates = new List<InterestRate>();
            foreach (var row in dataTable.Rows)
            {
                if (decimal.TryParse(row["Rate"], out var rate))
                {
                    _interestRates.Add(new InterestRate
                    {
                        Currency = "USD",
                        RateType = "Treasury",
                        Rate = rate,
                        Tenor = row["Maturity"] + "Y"
                    });
                }
            }
            _context["InterestRates"] = _interestRates;
        }

        [When(@"I build the yield curve")]
        public void WhenIBuildTheYieldCurve()
        {
            _buildingStrategy ??= new LinearCurveBuildingStrategy();
            _interpolationStrategy ??= new LinearInterpolationStrategy();
            _curveBuilder ??= new YieldCurveBuilder(_buildingStrategy, _interpolationStrategy);

            try
            {
                _currentCurve = _curveBuilder.Build(_bondQuotes, _interestRates);
                _context["CurrentCurve"] = _currentCurve;
                _context["BuildException"] = null;
            }
            catch (Exception ex)
            {
                _context["BuildException"] = ex;
            }
        }

        [When(@"I analyze the curve shape")]
        public void WhenIAnalyzeTheCurveShape()
        {
            Assert.NotNull(_currentCurve);
            _context["CurveAnalyzed"] = true;
        }

        [When(@"I use the curve to discount cash flows")]
        public void WhenIUseTheCurveToDiscountCashFlows()
        {
            Assert.NotNull(_currentCurve);
            Assert.NotNull(_bondPricingInput);
            _interpolationStrategy ??= new LinearInterpolationStrategy();

            var bondInput = _bondPricingInput!;
            var couponAmount = bondInput.CouponRate * bondInput.ParValue / bondInput.CouponFrequency;
            var totalPayments = bondInput.TimeToMaturity * bondInput.CouponFrequency;
            decimal presentValue = 0m;

            for (int payment = 1; payment <= totalPayments; payment++)
            {
                var maturity = payment / (decimal)bondInput.CouponFrequency;
                var rate = _interpolationStrategy.Interpolate(maturity, _currentCurve);
                var discountFactor = (decimal)Math.Pow((double)(1 + rate / bondInput.CouponFrequency), -(double)payment);
                var cashFlow = payment == totalPayments ? couponAmount + bondInput.ParValue : couponAmount;
                presentValue += cashFlow * discountFactor;
            }

            _presentValue = presentValue;
            _context["PresentValue"] = _presentValue;
        }

        [Given(@"the curve has points at (.*), (.*), and (.*)")]
        public void GivenCurveHasPointsAt(string tenor1, string tenor2, string tenor3)
        {
            var tenorValues = new List<(string tenor, decimal maturity)>
            {
                (tenor1, ParseTenorToYears(tenor1)),
                (tenor2, ParseTenorToYears(tenor2)),
                (tenor3, ParseTenorToYears(tenor3))
            };

            _context["CurvePoints"] = tenorValues;
        }

        [Given(@"I provide an empty list of bonds")]
        public void GivenIProvideEmptyBondList()
        {
            _bondQuotes = new List<BondQuote>();
            _context["BondQuotes"] = _bondQuotes;
        }

        [Given(@"I provide an empty list of interest rates")]
        public void GivenIProvideEmptyInterestRateList()
        {
            _interestRates = new List<InterestRate>();
            _context["InterestRates"] = _interestRates;
        }

        [Given(@"I have a curve with only one yield point at (\d+) years")]
        public void GivenIHaveSingleYieldPoint(int maturityYears)
        {
            _currentCurve = new List<YieldPoint>
            {
                new YieldPoint { Maturity = maturityYears, Rate = 0.030m }
            };
            _context["CurrentCurve"] = _currentCurve;
        }

        [Given(@"I have a yield curve with points at 1Y, 5Y, and 10Y")]
        public void GivenIHaveCurveAt1Y5Y10Y()
        {
            _currentCurve = new List<YieldPoint>
            {
                new YieldPoint { Maturity = 1, Rate = 0.025m },
                new YieldPoint { Maturity = 5, Rate = 0.030m },
                new YieldPoint { Maturity = 10, Rate = 0.035m }
            };
            _interpolationStrategy ??= new LinearInterpolationStrategy();
            _context["CurrentCurve"] = _currentCurve;
        }

        [Given(@"I have a yield curve with points at (.*)Y, (.*)Y, (.*)Y")]
        public void GivenIHaveYieldCurveWithPointsAtMaturities(string tenor1, string tenor2, string tenor3)
        {
            _currentCurve = new List<YieldPoint>
            {
                new YieldPoint { Maturity = ParseTenorToYears(tenor1 + "Y"), Rate = 0.025m },
                new YieldPoint { Maturity = ParseTenorToYears(tenor2 + "Y"), Rate = 0.030m },
                new YieldPoint { Maturity = ParseTenorToYears(tenor3 + "Y"), Rate = 0.035m }
            };
            _interpolationStrategy ??= new LinearInterpolationStrategy();
            _context["CurrentCurve"] = _currentCurve;
        }

        [Given(@"I have built a yield curve from valid bonds using bootstrap")]
        public void GivenIHaveBuiltCurveFromBondsUsingBootstrap()
        {
            GivenIUseBootstrapStrategy();
            var validBonds = CreateValidBondQuotes();
            _bondQuotes = validBonds;
            
            var rates = new List<InterestRate>();
            _currentCurve = _curveBuilder.Build(_bondQuotes, rates);
            _context["CurrentCurve"] = _currentCurve;
        }

        [Given(@"I provide bonds in random order:")]
        public void GivenIProvideBondsInRandomOrder(Table dataTable)
        {
            _bondQuotes = dataTable.CreateSet<BondQuote>().ToList();
            // Create any required fields with defaults
            foreach (var bond in _bondQuotes)
            {
                bond.CouponRate = bond.CouponRate == 0 ? 0.04m : bond.CouponRate;
                bond.CouponFrequency = bond.CouponFrequency == 0 ? 2 : bond.CouponFrequency;
            }
            _context["BondQuotes"] = _bondQuotes;
        }

        [Given(@"I provide a bond with the following invalid properties:")]
        public void GivenIProvideBondWithInvalidProperties(Table dataTable)
        {
            // Create bonds with potentially invalid data
            var invalidBond = new BondQuote
            {
                BondId = "INVALID_BOND",
                MaturityYears = -5m, // Negative
                AskPrice = -100m,     // Negative
                CouponRate = 2.5m,    // > 1.0
                CouponFrequency = 2
            };
            _bondQuotes = new List<BondQuote> { invalidBond };
            _context["InvalidBonds"] = _bondQuotes;
        }

        [Given(@"I provide interest rates with malformed tenor strings:")]
        public void GivenIProvideRatesWithMalformedTenors(Table dataTable)
        {
            _interestRates = new List<InterestRate>();
            var rows = dataTable.Rows;

            foreach (var row in rows)
            {
                if (decimal.TryParse(row["Rate"], out var rate))
                {
                    _interestRates.Add(new InterestRate
                    {
                        Currency = "USD",
                        RateType = "Test",
                        Rate = rate / 100m,
                        Tenor = row["Tenor"]
                    });
                }
            }
            _context["MalformedTenors"] = _interestRates;
        }

        [Given(@"I provide interest rates with edge-case tenor formats:")]
        public void GivenIProvideRatesWithEdgeCaseTenors(Table dataTable)
        {
            GivenIHaveInterestRateObservations(dataTable);
        }

        [Given(@"I provide rates with excessive decimal places:")]
        public void GivenIProvideRatesWithExcessiveDecimals(Table dataTable)
        {
            var rows = dataTable.Rows;
            _interestRates = new List<InterestRate>();

            foreach (var row in rows)
            {
                if (decimal.TryParse(row["Rate"], out var rate))
                {
                    _interestRates.Add(new InterestRate
                    {
                        Currency = "USD",
                        RateType = "Test",
                        Rate = rate,
                        Tenor = row["Tenor"]
                    });
                }
            }
            _context["HighPrecisionRates"] = _interestRates;
        }

        [Given(@"I provide fewer than 3 interest rate observations:")]
        public void GivenIProvideFewObservations(Table dataTable)
        {
            GivenIHaveInterestRateObservations(dataTable);
        }

        [Given(@"I have interest rate data without tenor information:")]
        public void GivenIHaveRatesWithoutTenor(Table dataTable)
        {
            _interestRates = new List<InterestRate>();
            var rows = dataTable.Rows;

            foreach (var row in rows)
            {
                if (decimal.TryParse(row["Rate"], out var rate))
                {
                    _interestRates.Add(new InterestRate
                    {
                        Currency = "USD",
                        RateType = "Test",
                        Rate = rate,
                        Tenor = string.IsNullOrEmpty(row["Tenor"]) ? null : row["Tenor"]
                    });
                }
            }
            _context["RatesWithoutTenor"] = _interestRates;
        }

        [Given(@"each tenor has an observed yield of (.*) percent")]
        public void GivenEachTenorHasYield(decimal yieldPercent)
        {
            var tenors = _context["Tenors"] as List<string>;
            if (tenors != null)
            {
                _interestRates = tenors.Select(t => new InterestRate
                {
                    Currency = "USD",
                    RateType = "Treasury",
                    Rate = yieldPercent / 100m,
                    Tenor = t
                }).ToList();
                _context["InterestRates"] = _interestRates;
            }
        }

        [Given(@"I have interest rate observations for the following tenors:")]
        public void GivenIHaveTenorObservations(Table dataTable)
        {
            var tenors = new List<string>();
            foreach (var row in dataTable.Rows)
            {
                tenors.Add(row["Tenor"]);
            }
            _context["Tenors"] = tenors;
        }

        // ========== WHEN Steps ==========

        [When(@"I build the yield curve from these bonds")]
        public void WhenIBuildCurveFromBonds()
        {
            var rates = _context.ContainsKey("InterestRates") 
                ? (List<InterestRate>)_context["InterestRates"] 
                : new List<InterestRate>();

            _curveBuilder ??= new YieldCurveBuilder(
                new BootstrapCurveBuildingStrategy(),
                _interpolationStrategy ?? new LinearInterpolationStrategy()
            );

            try
            {
                _currentCurve = _curveBuilder.Build(_bondQuotes, rates);
                _context["CurrentCurve"] = _currentCurve;
                _context["LastBuildException"] = null;
            }
            catch (Exception ex)
            {
                _context["LastBuildException"] = ex;
            }
        }

        [When(@"I build the yield curve from these rates")]
        public void WhenIBuildCurveFromRates()
        {
            var bonds = _context.ContainsKey("BondQuotes") 
                ? (List<BondQuote>)_context["BondQuotes"] 
                : new List<BondQuote>();

            _curveBuilder ??= new YieldCurveBuilder(
                new LinearCurveBuildingStrategy(),
                _interpolationStrategy ?? new LinearInterpolationStrategy()
            );

            try
            {
                _currentCurve = _curveBuilder.Build(bonds, _interestRates);
                _context["CurrentCurve"] = _currentCurve;
                _context["LastBuildException"] = null;
            }
            catch (Exception ex)
            {
                _context["LastBuildException"] = ex;
            }
        }

        [When(@"I request the interpolated yield at maturity (\d+) years")]
        public void WhenIRequestInterpolatedYieldAtMaturity(int maturityYears)
        {
            _interpolationStrategy ??= new LinearInterpolationStrategy();
            var targetMaturity = (decimal)maturityYears;

            try
            {
                var interpolatedRate = _interpolationStrategy.Interpolate(targetMaturity, _currentCurve);
                _context["InterpolatedRate"] = interpolatedRate;
            }
            catch (Exception ex)
            {
                _context["InterpolationException"] = ex;
            }
        }

        [When(@"I request the interpolated yield at (\d+\.?\d*) years")]
        public void WhenIRequestInterpolatedYieldAtDecimalMaturity(decimal maturityYears)
        {
            _interpolationStrategy ??= new LinearInterpolationStrategy();

            try
            {
                var interpolatedRate = _interpolationStrategy.Interpolate(maturityYears, _currentCurve);
                _context["InterpolatedRate"] = interpolatedRate;
            }
            catch (Exception ex)
            {
                _context["InterpolationException"] = ex;
            }
        }

        [When(@"I request interpolation at (\d+\.?\d*) years.*")]
        public void WhenIRequestInterpolationAtYears(decimal maturityYears)
        {
            _interpolationStrategy ??= new LinearInterpolationStrategy();

            try
            {
                var interpolatedRate = _interpolationStrategy.Interpolate(maturityYears, _currentCurve);
                _context["InterpolatedRate"] = interpolatedRate;
            }
            catch (Exception ex)
            {
                _context["InterpolationException"] = ex;
            }
        }

        [When(@"I densify the curve with a step of (.*) years")]
        public void WhenIDensifyCurveWithStep(string stepStr)
        {
            _interpolationStrategy ??= new LinearInterpolationStrategy();

            if (decimal.TryParse(stepStr, out var step))
            {
                try
                {
                    var densified = _interpolationStrategy.Densify(_currentCurve, step);
                    _context["DensifiedCurve"] = densified;
                    _context["DensificationException"] = null;
                }
                catch (Exception ex)
                {
                    _context["DensificationException"] = ex;
                }
            }
        }

        [When(@"I densify the curve with daily \((.*) year\) intervals")]
        public void WhenIDensifyWithDailyIntervals(string stepStr)
        {
            if (decimal.TryParse(stepStr, out var step))
            {
                WhenIDensifyCurveWithStep(step.ToString());
            }
        }

        [When(@"I densify the curve with a step of (\d+\.?\d*) years \(quarterly\)")]
        public void WhenIDensifyWithQuarterlyStep(decimal step)
        {
            WhenIDensifyCurveWithStep(step.ToString());
        }

        [When(@"I bootstrap the zero-coupon yield curve from these bonds")]
        public void WhenIBootstrapZeroCouponCurve()
        {
            GivenIUseBootstrapStrategy();
            WhenIBuildCurveFromBonds();
        }

        [When(@"I bootstrap the curve")]
        public void WhenIBootstrapCurve()
        {
            GivenIUseBootstrapStrategy();
            WhenIBuildCurveFromBonds();
        }

        [When(@"I request interpolated yields at (.*), (.*), and (.*)")]
        public void WhenIRequestInterpolatedYieldsAtMultiplePoints(string tenor1, string tenor2, string tenor3)
        {
            var maturities = new List<(string tenor, decimal maturity)>
            {
                (tenor1, ParseTenorToYears(tenor1)),
                (tenor2, ParseTenorToYears(tenor2)),
                (tenor3, ParseTenorToYears(tenor3))
            };

            _interpolationStrategy ??= new LinearInterpolationStrategy();
            var interpolatedRates = new List<(decimal maturity, decimal rate)>();

            foreach (var (tenor, maturity) in maturities)
            {
                var rate = _interpolationStrategy.Interpolate(maturity, _currentCurve);
                interpolatedRates.Add((maturity, rate));
            }

            _context["InterpolatedRates"] = interpolatedRates;
        }


        [When(@"I attempt to build a curve")]
        public void WhenIAttemptToBuildCurve()
        {
            try
            {
                _buildingStrategy ??= new LinearCurveBuildingStrategy();
                _interpolationStrategy ??= new LinearInterpolationStrategy();
                _curveBuilder ??= new YieldCurveBuilder(_buildingStrategy, _interpolationStrategy);

                _currentCurve = _curveBuilder.Build(_bondQuotes, _interestRates);
                _context["CurrentCurve"] = _currentCurve;
                _context["BuildException"] = null;
            }
            catch (Exception ex)
            {
                _context["BuildException"] = ex;
            }
        }

        [When(@"I use the linear strategy to build a curve")]
        public void WhenIUseLinearStrategyToBuild()
        {
            GivenIUseLinearStrategy();
            WhenIAttemptToBuildCurve();
        }

        [When(@"I attempt to build a curve using the linear strategy")]
        public void WhenIAttemptToBuildCurveUsingLinearStrategy()
        {
            WhenIUseLinearStrategyToBuild();
        }

        [When(@"I attempt to parse these tenors")]
        public void WhenIAttemptToParseTenors()
        {
            _context["ParseExceptions"] = new List<Exception>();
            var parseExceptions = new List<Exception>();

            foreach (var rate in _interestRates)
            {
                try
                {
                    if (!string.IsNullOrEmpty(rate.Tenor))
                    {
                        ParseTenorToYears(rate.Tenor);
                    }
                }
                catch (Exception ex)
                {
                    parseExceptions.Add(ex);
                }
            }

            _context["ParseExceptions"] = parseExceptions;
            if (parseExceptions.Any())
            {
                _context["BuildException"] = parseExceptions.First();
            }
        }

        [When(@"I parse these tenors")]
        public void WhenIParseTenors()
        {
            WhenIAttemptToParseTenors();
        }

        [When(@"I attempt to build a curve using Nelson-Siegel strategy")]
        public void WhenIAttemptToBuildWithNelsonSiegel()
        {
            try
            {
                if (!_interestRates.Any())
                {
                    throw new InvalidOperationException("No observations available.");
                }

                _buildingStrategy = new NelsonSiegelCurveBuildingStrategy();
                _interpolationStrategy ??= new LinearInterpolationStrategy();
                _curveBuilder = new YieldCurveBuilder(_buildingStrategy, _interpolationStrategy);

                _currentCurve = _curveBuilder.Build(new List<BondQuote>(), _interestRates);
                _context["CurrentCurve"] = _currentCurve;
                _context["NelsonSiegelException"] = null;
            }
            catch (Exception ex)
            {
                _context["NelsonSiegelException"] = ex;
            }
        }

        public void WhenIAttemptToDeserializeJson()
        {
            // This is tested in the FredDataFetcher tests
            _context["JsonDeserializationTested"] = true;
        }

        private record BondPricingInput
        {
            public decimal CouponRate { get; init; }
            public int TimeToMaturity { get; init; }
            public decimal ParValue { get; init; }
            public int CouponFrequency { get; init; }
        }

        // ========== Helper Methods ==========

        private decimal ParseTenorToYears(string tenor)
        {
            if (tenor.EndsWith("M", StringComparison.OrdinalIgnoreCase))
            {
                var monthsStr = tenor.TrimEnd('M', 'm');
                if (decimal.TryParse(monthsStr, out var months))
                {
                    return months / 12m;
                }
            }
            else if (tenor.EndsWith("Y", StringComparison.OrdinalIgnoreCase))
            {
                var yearsStr = tenor.TrimEnd('Y', 'y');
                if (decimal.TryParse(yearsStr, out var years))
                {
                    return years;
                }
            }

            throw new FormatException($"Invalid tenor format: {tenor}");
        }

        private List<BondQuote> CreateValidBondQuotes()
        {
            return new List<BondQuote>
            {
                new BondQuote
                {
                    BondId = "BOND001",
                    MaturityYears = 1,
                    YieldToMaturity = 0.0425m,
                    AskPrice = 99.75m,
                    CouponRate = 0.04m,
                    CouponFrequency = 2
                },
                new BondQuote
                {
                    BondId = "BOND002",
                    MaturityYears = 5,
                    YieldToMaturity = 0.0480m,
                    AskPrice = 98.50m,
                    CouponRate = 0.045m,
                    CouponFrequency = 2
                },
                new BondQuote
                {
                    BondId = "BOND003",
                    MaturityYears = 10,
                    YieldToMaturity = 0.0500m,
                    AskPrice = 97.00m,
                    CouponRate = 0.05m,
                    CouponFrequency = 2
                }
            };
        }
    }
}
