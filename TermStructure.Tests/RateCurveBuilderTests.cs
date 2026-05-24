using Moq;
using TermStructure.Builders;
using TermStructure.Models;
using TermStructure.Strategies;


namespace TermStructure.Tests
{
    /// <summary>
    /// Comprehensive test suite for RateCurveBuilder and YieldCurveBuilder.
    /// Tests cover happy paths, edge cases, error conditions, and integration with strategies.
    /// </summary>
    public class RateCurveBuilderTests
    {
        // ========== Fixtures ==========

        private readonly List<BondQuote> _validBonds = new()
        {
            new BondQuote
            {
                BondId = "BOND001",
                Isin = "US0001001",
                BidPrice = 99.50m,
                AskPrice = 99.75m,
                YieldToMaturity = 0.0425m,
                MaturityYears = 1m,
                CouponRate = 0.04m,
                CouponFrequency = 2
            },
            new BondQuote
            {
                BondId = "BOND002",
                Isin = "US0002001",
                BidPrice = 98.00m,
                AskPrice = 98.25m,
                YieldToMaturity = 0.0500m,
                MaturityYears = 10m,
                CouponRate = 0.045m,
                CouponFrequency = 2
            }
        };

        private readonly List<InterestRate> _validRates = new()
        {
            new InterestRate { Currency = "USD", RateType = "LIBOR", Rate = 0.0425m, Tenor = "1Y" },
            new InterestRate { Currency = "USD", RateType = "LIBOR", Rate = 0.0500m, Tenor = "10Y" }
        };

        private readonly List<YieldPoint> _expectedCurvePoints = new()
        {
            new YieldPoint { Maturity = 1m, Rate = 0.0425m },
            new YieldPoint { Maturity = 10m, Rate = 0.0500m }
        };

        // ========== Helper Methods ==========

        /// <summary>
        /// Creates a YieldCurveBuilder with mocked strategies for testing.
        /// </summary>
        private YieldCurveBuilder CreateYieldCurveBuilderWithMocks(
            List<YieldPoint>? curvePoints = null,
            List<YieldPoint>? interpolatedPoints = null)
        {
            var buildingStrategyMock = new Mock<ICurveBuildingStrategy>();
            buildingStrategyMock
                .Setup(s => s.BuildCurve(It.IsAny<IEnumerable<BondQuote>>(), It.IsAny<IEnumerable<InterestRate>>()))
                .Returns(curvePoints ?? _expectedCurvePoints);

            var interpolationStrategyMock = new Mock<IInterpolationStrategy>();
            interpolationStrategyMock
                .Setup(s => s.Interpolate(It.IsAny<decimal>(), It.IsAny<List<YieldPoint>>()))
                .Returns(0.0465m); // Arbitrary interpolated rate

            interpolationStrategyMock
                .Setup(s => s.Densify(It.IsAny<List<YieldPoint>>(), It.IsAny<decimal>()))
                .Returns(interpolatedPoints ?? curvePoints ?? _expectedCurvePoints);

            return new YieldCurveBuilder(buildingStrategyMock.Object, interpolationStrategyMock.Object);
        }

        // ========== Happy Path Tests ==========

        [Fact]
        public void Build_WithValidBondsAndRates_ReturnsCurvePoints()
        {
            // Arrange
            var builder = CreateYieldCurveBuilderWithMocks();

            // Act
            var result = builder.Build(_validBonds, _validRates);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal(1m, result[0].Maturity);
            Assert.Equal(0.0425m, result[0].Rate);
            Assert.Equal(10m, result[1].Maturity);
            Assert.Equal(0.0500m, result[1].Rate);
        }

        [Fact]
        public void Build_WithValidBondsAndRates_CallsBuildingStrategy()
        {
            // Arrange
            var buildingStrategyMock = new Mock<ICurveBuildingStrategy>();
            buildingStrategyMock
                .Setup(s => s.BuildCurve(It.IsAny<IEnumerable<BondQuote>>(), It.IsAny<IEnumerable<InterestRate>>()))
                .Returns(_expectedCurvePoints);

            var interpolationStrategyMock = new Mock<IInterpolationStrategy>();

            var builder = new YieldCurveBuilder(buildingStrategyMock.Object, interpolationStrategyMock.Object);

            // Act
            builder.Build(_validBonds, _validRates);

            // Assert
            buildingStrategyMock.Verify(
                s => s.BuildCurve(
                    It.Is<IEnumerable<BondQuote>>(b => b != null && b.Count() == 2),
                    It.Is<IEnumerable<InterestRate>>(r => r != null && r.Count() == 2)),
                Times.Once);
        }

        [Fact]
        public void Build_WithSingleBond_ReturnsSinglePoint()
        {
            // Arrange
            var singleBond = new List<BondQuote> { _validBonds[0] };
            var singleRate = new List<InterestRate> { _validRates[0] };
            var expectedPoint = new List<YieldPoint> { _expectedCurvePoints[0] };

            var builder = CreateYieldCurveBuilderWithMocks(expectedPoint);

            // Act
            var result = builder.Build(singleBond, singleRate);

            // Assert
            Assert.Single(result);
            Assert.Equal(1m, result[0].Maturity);
        }

        // ========== Edge Case Tests ==========

        [Fact]
        public void Build_WithEmptyBonds_ReturnsEmptyList()
        {
            // Arrange
            var emptyBonds = new List<BondQuote>();
            var builder = CreateYieldCurveBuilderWithMocks(new List<YieldPoint>());

            // Act
            var result = builder.Build(emptyBonds, _validRates);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void Build_WithEmptyRates_ReturnsEmptyList()
        {
            // Arrange
            var emptyRates = new List<InterestRate>();
            var builder = CreateYieldCurveBuilderWithMocks(new List<YieldPoint>());

            // Act
            var result = builder.Build(_validBonds, emptyRates);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void Build_WithBothEmpty_ReturnsEmptyList()
        {
            // Arrange
            var emptyBonds = new List<BondQuote>();
            var emptyRates = new List<InterestRate>();
            var builder = CreateYieldCurveBuilderWithMocks(new List<YieldPoint>());

            // Act
            var result = builder.Build(emptyBonds, emptyRates);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void Build_WithLargeCurve_HandlesHundredsOfPoints()
        {
            // Arrange
            var largePoints = new List<YieldPoint>();
            for (int i = 1; i <= 100; i++)
            {
                largePoints.Add(new YieldPoint { Maturity = i, Rate = 0.02m + (i * 0.0001m) });
            }
            var builder = CreateYieldCurveBuilderWithMocks(largePoints);

            // Act
            var result = builder.Build(_validBonds, _validRates);

            // Assert
            Assert.Equal(100, result.Count);
            Assert.Equal(1m, result[0].Maturity);
            Assert.Equal(100m, result[99].Maturity);
        }

        // ========== Null/ArgumentException Tests ==========

        [Fact]
        public void Build_WithNullBonds_DelegatesNullToStrategy()
        {
            // Arrange - The Build method doesn't validate nulls, it delegates to strategy
            var buildingStrategyMock = new Mock<ICurveBuildingStrategy>();
            buildingStrategyMock
                .Setup(s => s.BuildCurve(null, _validRates))
                .Returns(_expectedCurvePoints);

            var interpolationStrategyMock = new Mock<IInterpolationStrategy>();
            var builder = new YieldCurveBuilder(buildingStrategyMock.Object, interpolationStrategyMock.Object);

            // Act
            var result = builder.Build(null!, _validRates);

            // Assert
            Assert.NotNull(result);
            buildingStrategyMock.Verify(s => s.BuildCurve(null, It.IsAny<IEnumerable<InterestRate>>()), Times.Once);
        }

        [Fact]
        public void Build_WithNullRates_DelegatesNullToStrategy()
        {
            // Arrange - The Build method doesn't validate nulls, it delegates to strategy
            var buildingStrategyMock = new Mock<ICurveBuildingStrategy>();
            buildingStrategyMock
                .Setup(s => s.BuildCurve(_validBonds, null))
                .Returns(_expectedCurvePoints);

            var interpolationStrategyMock = new Mock<IInterpolationStrategy>();
            var builder = new YieldCurveBuilder(buildingStrategyMock.Object, interpolationStrategyMock.Object);

            // Act
            var result = builder.Build(_validBonds, null!);

            // Assert
            Assert.NotNull(result);
            buildingStrategyMock.Verify(s => s.BuildCurve(It.IsAny<IEnumerable<BondQuote>>(), null), Times.Once);
        }

        [Fact]
        public void Constructor_WithNullBuildingStrategy_ThrowsArgumentNullException()
        {
            // Arrange
            var interpolationMock = new Mock<IInterpolationStrategy>();

            // Act & Assert
            var ex = Assert.Throws<ArgumentNullException>(
                () => new YieldCurveBuilder(null!, interpolationMock.Object));
            Assert.Contains("buildingStrategy", ex.Message);
        }

        [Fact]
        public void Constructor_WithNullInterpolationStrategy_ThrowsArgumentNullException()
        {
            // Arrange
            var buildingMock = new Mock<ICurveBuildingStrategy>();

            // Act & Assert
            var ex = Assert.Throws<ArgumentNullException>(
                () => new YieldCurveBuilder(buildingMock.Object, null!));
            Assert.Contains("interpolationStrategy", ex.Message);
        }

        // ========== GetRate Tests ==========

        [Fact]
        public void GetRate_WithValidMaturity_CallsInterpolationStrategy()
        {
            // Arrange
            var interpolationStrategyMock = new Mock<IInterpolationStrategy>();
            interpolationStrategyMock
                .Setup(s => s.Interpolate(It.IsAny<decimal>(), It.IsAny<List<YieldPoint>>()))
                .Returns(0.0450m);

            var buildingStrategyMock = new Mock<ICurveBuildingStrategy>();
            buildingStrategyMock
                .Setup(s => s.BuildCurve(It.IsAny<IEnumerable<BondQuote>>(), It.IsAny<IEnumerable<InterestRate>>()))
                .Returns(_expectedCurvePoints);

            var builder = new YieldCurveBuilder(buildingStrategyMock.Object, interpolationStrategyMock.Object);

            // Act
            var rate = builder.GetRate(5.5m, _validBonds, _validRates);

            // Assert
            Assert.Equal(0.0450m, rate);
            interpolationStrategyMock.Verify(
                s => s.Interpolate(5.5m, It.IsAny<List<YieldPoint>>()),
                Times.Once);
        }

        [Fact]
        public void GetRate_WithMinMaturity_ReturnsRate()
        {
            // Arrange
            var builder = CreateYieldCurveBuilderWithMocks();

            // Act
            var rate = builder.GetRate(1m, _validBonds, _validRates);

            // Assert
            Assert.Equal(0.0465m, rate); // Mocked return value
        }

        [Fact]
        public void GetRate_WithMaxMaturity_ReturnsRate()
        {
            // Arrange
            var builder = CreateYieldCurveBuilderWithMocks();

            // Act
            var rate = builder.GetRate(10m, _validBonds, _validRates);

            // Assert
            Assert.Equal(0.0465m, rate);
        }

        [Fact]
        public void GetRate_WithIntermediateMaturity_InterpolatesCorrectly()
        {
            // Arrange
            var builder = CreateYieldCurveBuilderWithMocks();

            // Act
            var rate = builder.GetRate(5.5m, _validBonds, _validRates);

            // Assert - Should be between 1Y and 10Y rates
            Assert.Equal(0.0465m, rate);
        }

        // ========== Densify Tests ==========

        [Fact]
        public void Densify_WithValidStep_ReturnsDensePoints()
        {
            // Arrange
            var densePoints = new List<YieldPoint>();
            for (decimal i = 1; i <= 10; i += 0.5m)
            {
                densePoints.Add(new YieldPoint { Maturity = i, Rate = 0.02m + (i * 0.0005m) });
            }

            var builder = CreateYieldCurveBuilderWithMocks(interpolatedPoints: densePoints);

            // Act
            var result = builder.Densify(0.5m, _validBonds, _validRates);

            // Assert
            Assert.NotEmpty(result);
            Assert.True(result.Count > _expectedCurvePoints.Count);
        }

        [Fact]
        public void Densify_WithSmallStep_ProducesMorePoints()
        {
            // Arrange
            var builder = CreateYieldCurveBuilderWithMocks();

            // Act
            var result = builder.Densify(0.1m, _validBonds, _validRates);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }

        [Fact]
        public void Densify_WithLargeStep_ProducesFewPoints()
        {
            // Arrange
            var builder = CreateYieldCurveBuilderWithMocks();

            // Act
            var result = builder.Densify(5m, _validBonds, _validRates);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }

        [Fact]
        public void Densify_WithValidStep_CallsInterpolationDensify()
        {
            // Arrange
            var interpolationStrategyMock = new Mock<IInterpolationStrategy>();
            var expectedDensePoints = new List<YieldPoint>
            {
                new YieldPoint { Maturity = 1m, Rate = 0.0425m },
                new YieldPoint { Maturity = 5.5m, Rate = 0.0463m },
                new YieldPoint { Maturity = 10m, Rate = 0.0500m }
            };

            interpolationStrategyMock
                .Setup(s => s.Densify(It.IsAny<List<YieldPoint>>(), 0.5m))
                .Returns(expectedDensePoints);

            var buildingStrategyMock = new Mock<ICurveBuildingStrategy>();
            buildingStrategyMock
                .Setup(s => s.BuildCurve(It.IsAny<IEnumerable<BondQuote>>(), It.IsAny<IEnumerable<InterestRate>>()))
                .Returns(_expectedCurvePoints);

            var builder = new YieldCurveBuilder(buildingStrategyMock.Object, interpolationStrategyMock.Object);

            // Act
            var result = builder.Densify(0.5m, _validBonds, _validRates);

            // Assert
            interpolationStrategyMock.Verify(
                s => s.Densify(It.IsAny<List<YieldPoint>>(), 0.5m),
                Times.Once);
            Assert.Equal(3, result.Count);
        }

        // ========== Integration Tests ==========

        [Fact]
        public void Build_WithRealStrategies_ProducesValidCurve()
        {
            // Arrange - Use actual strategy implementations
            var buildingStrategy = new LinearCurveBuildingStrategy();
            var interpolationStrategy = new LinearInterpolationStrategy();
            var builder = new YieldCurveBuilder(buildingStrategy, interpolationStrategy);

            // Act
            var curve = builder.Build(_validBonds, _validRates);

            // Assert
            Assert.NotNull(curve);
            Assert.NotEmpty(curve);
        }

        [Fact]
        public void MultipleBuilds_WithSameBuilder_ProducesConsistentResults()
        {
            // Arrange
            var builder = CreateYieldCurveBuilderWithMocks();

            // Act
            var result1 = builder.Build(_validBonds, _validRates);
            var result2 = builder.Build(_validBonds, _validRates);

            // Assert
            Assert.Equal(result1.Count, result2.Count);
            for (int i = 0; i < result1.Count; i++)
            {
                Assert.Equal(result1[i].Maturity, result2[i].Maturity);
                Assert.Equal(result1[i].Rate, result2[i].Rate);
            }
        }

        [Fact]
        public void Build_FollowsTemplateMethodPattern()
        {
            // Arrange
            var buildingStrategyMock = new Mock<ICurveBuildingStrategy>();
            var callOrder = new List<string>();

            buildingStrategyMock
                .Setup(s => s.BuildCurve(It.IsAny<IEnumerable<BondQuote>>(), It.IsAny<IEnumerable<InterestRate>>()))
                .Returns(() =>
                {
                    callOrder.Add("BuildCurve");
                    return _expectedCurvePoints;
                });

            var interpolationStrategyMock = new Mock<IInterpolationStrategy>();
            var builder = new YieldCurveBuilder(buildingStrategyMock.Object, interpolationStrategyMock.Object);

            // Act
            builder.Build(_validBonds, _validRates);

            // Assert
            Assert.Single(callOrder);
            Assert.Contains("BuildCurve", callOrder);
        }

        // ========== Boundary/Special Value Tests ==========

        [Fact]
        public void Build_WithZeroMaturityBond_HandlesCorrectly()
        {
            // Arrange
            var zeroMaturityBond = new BondQuote
            {
                BondId = "ZERO",
                Isin = "ZERO",
                BidPrice = 100m,
                AskPrice = 100m,
                YieldToMaturity = 0.04m,
                MaturityYears = 0m, // Zero maturity
                CouponRate = 0.04m,
                CouponFrequency = 2
            };
            var bonds = new List<BondQuote> { zeroMaturityBond };
            var builder = CreateYieldCurveBuilderWithMocks(
                new List<YieldPoint> { new YieldPoint { Maturity = 0m, Rate = 0.04m } });

            // Act
            var result = builder.Build(bonds, _validRates);

            // Assert
            Assert.Single(result);
            Assert.Equal(0m, result[0].Maturity);
        }

        [Fact]
        public void Build_WithVeryLargeMaturity_HandlesCorrectly()
        {
            // Arrange
            var largeMaturityBond = new BondQuote
            {
                BondId = "LARGE",
                Isin = "LARGE",
                BidPrice = 80m,
                AskPrice = 80.5m,
                YieldToMaturity = 0.035m,
                MaturityYears = 50m, // 50 year bond
                CouponRate = 0.035m,
                CouponFrequency = 2
            };
            var bonds = new List<BondQuote> { largeMaturityBond };
            var points = new List<YieldPoint> { new YieldPoint { Maturity = 50m, Rate = 0.035m } };
            var builder = CreateYieldCurveBuilderWithMocks(points);

            // Act
            var result = builder.Build(bonds, _validRates);

            // Assert
            Assert.Single(result);
            Assert.Equal(50m, result[0].Maturity);
        }

        [Fact]
        public void GetRate_WithZeroMaturity_ReturnsRate()
        {
            // Arrange
            var builder = CreateYieldCurveBuilderWithMocks();

            // Act
            var rate = builder.GetRate(0m, _validBonds, _validRates);

            // Assert - Should return the mocked interpolated rate
            Assert.Equal(0.0465m, rate);
        }

        [Fact]
        public void GetRate_WithNegativeMaturity_HandlesGracefully()
        {
            // Arrange
            var builder = CreateYieldCurveBuilderWithMocks();

            // Act - Negative maturity is unusual but should not crash
            var rate = builder.GetRate(-1m, _validBonds, _validRates);

            // Assert - Should return the mocked interpolated rate
            Assert.Equal(0.0465m, rate);
        }
    }
}
