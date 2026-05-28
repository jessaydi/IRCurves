using System;
using System.Collections.Generic;
using System.Linq;
using TermStructure.Models;
using TermStructure.Strategies;

namespace TermStructure.Tests
{
    public class BootstrapCurveBuildingStrategyTests
    {
        [Fact]
        public void BuildCurve_WithNoBonds_ReturnsEmptyList()
        {
            // Arrange
            var strategy = new BootstrapCurveBuildingStrategy();
            var bonds = new List<BondQuote>();
            var rates = new List<InterestRate>
            {
                new InterestRate { Currency = "USD", RateType = "LIBOR", Rate = 0.01m, Tenor = "1Y" }
            };

            // Act
            var result = strategy.BuildCurve(bonds, rates);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public void BuildCurve_SortsBondsByMaturity_BeforeBootstrapping()
        {
            // Arrange
            var strategy = new BootstrapCurveBuildingStrategy();
            var bonds = new List<BondQuote>
            {
                CreateBond("BOND002", maturityYears: 2m, couponRate: 0.04m, askPrice: 95.0772m, couponFrequency: 2),
                CreateBond("BOND001", maturityYears: 1m, couponRate: 0.04m, askPrice: 98.04m, couponFrequency: 1)
            };
            var rates = new List<InterestRate>();

            // Act
            var result = strategy.BuildCurve(bonds, rates);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal(1m, result[0].Maturity);
            Assert.Equal(2m, result[1].Maturity);
        }

        [Fact]
        public void BuildCurve_CalculatesZeroRatesForCouponBonds()
        {
            // Arrange
            var strategy = new BootstrapCurveBuildingStrategy();
            var bonds = new List<BondQuote>
            {
                CreateBond("BOND001", maturityYears: 1m, couponRate: 0.04m, askPrice: 98.04m, couponFrequency: 1),
                CreateBond("BOND002", maturityYears: 2m, couponRate: 0.04m, askPrice: 95.0772m, couponFrequency: 2)
            };
            var rates = Enumerable.Empty<InterestRate>();

            // Act
            var result = strategy.BuildCurve(bonds, rates);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal(1m, result[0].Maturity);
            Assert.InRange(result[0].Rate, 0.02019m, 0.02020m);
            Assert.Equal(2m, result[1].Maturity);
            Assert.InRange(result[1].Rate, 0.02564m, 0.02565m);
        }

        [Fact]
        public void BuildCurve_UsesSemiAnnualPayments_AndInterpolatesIntermediateDiscountFactors()
        {
            // Arrange
            var strategy = new BootstrapCurveBuildingStrategy();
            var bonds = new List<BondQuote>
            {
                CreateBond("BOND001", maturityYears: 1m, couponRate: 0.04m, askPrice: 98.04m, couponFrequency: 1),
                CreateBond("BOND002", maturityYears: 2m, couponRate: 0.04m, askPrice: 95.0772m, couponFrequency: 2),
                CreateBond("BOND003", maturityYears: 3m, couponRate: 0.04m, askPrice: 92.1149m, couponFrequency: 2)
            };
            var rates = new List<InterestRate>();

            // Act
            var result = strategy.BuildCurve(bonds, rates);

            // Assert
            Assert.Equal(3, result.Count);
            Assert.Equal(1m, result[0].Maturity);
            Assert.Equal(2m, result[1].Maturity);
            Assert.Equal(3m, result[2].Maturity);
            Assert.InRange(result[2].Rate, 0.02779m, 0.02780m);
        }

        [Fact]
        public void BuildCurve_IgnoresRatesParameter_WhenRatesAreProvided()
        {
            // Arrange
            var strategy = new BootstrapCurveBuildingStrategy();
            var bonds = new List<BondQuote>
            {
                CreateBond("BOND001", maturityYears: 1m, couponRate: 0.04m, askPrice: 98.04m, couponFrequency: 1)
            };
            var emptyRates = Enumerable.Empty<InterestRate>();
            var providedRates = new List<InterestRate>
            {
                new InterestRate { Currency = "EUR", RateType = "EONIA", Rate = 0.10m, Tenor = "1Y" }
            };

            // Act
            var resultWithEmptyRates = strategy.BuildCurve(bonds, emptyRates);
            var resultWithProvidedRates = strategy.BuildCurve(bonds, providedRates);

            // Assert
            Assert.Single(resultWithEmptyRates);
            Assert.Single(resultWithProvidedRates);
            Assert.Equal(resultWithEmptyRates[0].Rate, resultWithProvidedRates[0].Rate);
        }

        private static BondQuote CreateBond(string bondId, decimal maturityYears, decimal couponRate, decimal askPrice, int couponFrequency)
        {
            return new BondQuote
            {
                BondId = bondId,
                Isin = $"ISIN-{bondId}",
                BidPrice = askPrice - 0.25m,
                AskPrice = askPrice,
                YieldToMaturity = 0m,
                MaturityYears = maturityYears,
                CouponRate = couponRate,
                CouponFrequency = couponFrequency
            };
        }
    }
}
