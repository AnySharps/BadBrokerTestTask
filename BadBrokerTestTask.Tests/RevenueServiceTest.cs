using BadBrokerTestTask.Enums;
using BadBrokerTestTask.Interfaces;
using BadBrokerTestTask.Models;
using BadBrokerTestTask.Services;
using BadBrokerTestTask.Settings;
using Microsoft.VisualBasic;
using Moq;
using NUnit.Framework.Interfaces;

namespace BadBrokerTestTask.Tests
{
    public class Tests
    {
        private const decimal moneyTolerance = 0.01m;

        private IRevenueService revenueService;
        private Mock<IRatesGatewayService> mockRatesGatewayService;

        [SetUp]
        public void Setup()
        {
            mockRatesGatewayService = new Mock<IRatesGatewayService>(MockBehavior.Strict);
            revenueService = new RevenueService(
                mockRatesGatewayService.Object,
                new CurrencySettings
                {
                    CurrenciesToBuy = new[]
                    {
                        Currency.RUB,
                        Currency.EUR,
                        Currency.GBP,
                        Currency.JPY
                    },
                    CurrencyToSell = Currency.USD
                });
        }

        [Test]
        public void Test_Example_1()
            => BaseTest(
                new[]
                {
                    new RateHistoryRow{ Date = new DateTime(2014, 12, 15), Value = 60.17m, Currency = Currency.RUB },
                    new RateHistoryRow{ Date = new DateTime(2014, 12, 16), Value = 72.99m, Currency = Currency.RUB },
                    new RateHistoryRow{ Date = new DateTime(2014, 12, 17), Value = 66.01m, Currency = Currency.RUB },
                    new RateHistoryRow{ Date = new DateTime(2014, 12, 18), Value = 61.44m, Currency = Currency.RUB },
                    new RateHistoryRow{ Date = new DateTime(2014, 12, 19), Value = 59.79m, Currency = Currency.RUB },
                    new RateHistoryRow{ Date = new DateTime(2014, 12, 20), Value = 59.79m, Currency = Currency.RUB },
                    new RateHistoryRow{ Date = new DateTime(2014, 12, 21), Value = 59.79m, Currency = Currency.RUB },
                    new RateHistoryRow{ Date = new DateTime(2014, 12, 22), Value = 54.78m, Currency = Currency.RUB },
                    new RateHistoryRow{ Date = new DateTime(2014, 12, 23), Value = 54.78m, Currency = Currency.RUB },
                },
                money: 100m,
                buyDate: new DateTime(2014, 12, 16),
                sellDate: new DateTime(2014, 12, 22),
                revenue: 27.24m,
                currency: Currency.RUB);

        [Test]
        public void Test_Example_2()
            => BaseTest(
                new[]
                {
                    new RateHistoryRow{ Date = new DateTime(2012, 1, 5), Value = 40.00m, Currency = Currency.RUB },
                    new RateHistoryRow{ Date = new DateTime(2012, 1, 7), Value = 35.00m, Currency = Currency.RUB },
                    new RateHistoryRow{ Date = new DateTime(2012, 1, 19), Value = 30.00m, Currency = Currency.RUB },
                },
                money: 50m,
                buyDate: new DateTime(2012, 1, 5),
                sellDate: new DateTime(2012, 1, 7),
                revenue: 5.14m,
                currency: Currency.RUB);

        [Test]
        public void Test_Different_Currencies()
            => BaseTest(
                new[]
                {
                    new RateHistoryRow{ Date = new DateTime(2012, 1, 5), Value = 40.00m, Currency = Currency.EUR },
                    new RateHistoryRow{ Date = new DateTime(2012, 1, 7), Value = 35.00m, Currency = Currency.EUR },
                    new RateHistoryRow{ Date = new DateTime(2012, 1, 19), Value = 30.00m, Currency = Currency.RUB },
                },
                money: 50m,
                buyDate: new DateTime(2012, 1, 5),
                sellDate: new DateTime(2012, 1, 7),
                revenue: 5.14m,
                currency: Currency.EUR);

        [Test]
        public void Test_No_Revenue()
            => BaseTest(
                new[]
                {
                    new RateHistoryRow{ Date = new DateTime(2012, 1, 7), Value = 35.00m, Currency = Currency.RUB },
                    new RateHistoryRow{ Date = new DateTime(2012, 1, 10), Value = 40.00m, Currency = Currency.RUB },
                    new RateHistoryRow{ Date = new DateTime(2012, 1, 12), Value = 45.00m, Currency = Currency.RUB },
                },
                money: 500000m,
                buyDate: new DateTime(2012, 1, 7),
                sellDate: new DateTime(2012, 1, 7),
                revenue: 0m,
                currency: Currency.RUB);

        [Test]
        public void Test_One_Day()
            => BaseTest(
                new[]
                {
                    new RateHistoryRow{ Date = new DateTime(2012, 1, 7), Value = 35.00m, Currency = Currency.RUB },
                },
                money: 500000m,
                buyDate: new DateTime(2012, 1, 7),
                sellDate: new DateTime(2012, 1, 7),
                revenue: 0m,
                currency: Currency.RUB);

        [Test]
        public void Test_60_Days_And_Hyper_Inflation()
            => BaseTest(
                Enumerable.Range(0, 60)
                    .Select(day => new RateHistoryRow
                    {
                        Date = new DateTime(2012, 1, 1).AddDays(day),
                        Value = 60 - day,
                        Currency = Currency.RUB
                    })
                    .ToArray(),
                money: 100m,
                buyDate: new DateTime(2012, 1, 1),
                sellDate: new DateTime(2012, 2, 29),
                revenue: 5841m,
                currency: Currency.RUB);

        [Test]
        public void Test_No_Rates()
        {
            var startDate = It.IsAny<DateTime>();
            var endDate = It.IsAny<DateTime>();
            mockRatesGatewayService
                .Setup(service => service
                    .GetRatesAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<Currency>(), It.IsAny<Currency[]>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(Array.Empty<DateRateHistory>()));

            var testResult = revenueService.GetMaxRevenueTransactionWithRates(startDate, endDate, money: 100m, It.IsAny<CancellationToken>()).Result;
            Assert.That(testResult.MaxRevenueTransaction.Revenue, Is.EqualTo(0m));
            Assert.That(testResult.MaxRevenueTransaction.BuyDate, Is.EqualTo(startDate));
            Assert.That(testResult.MaxRevenueTransaction.SellDate, Is.EqualTo(startDate));
        }


        private void BaseTest(
            RateHistoryRow[] rates,
            decimal money, 
            DateTime buyDate, 
            DateTime sellDate, 
            decimal revenue, 
            Currency currency)
        {
            var testData = rates
                .GroupBy(row => row.Date)
                .OrderBy(group => group.Key)
                .Select(group => new DateRateHistory
                {
                    Date = group.Key,
                    Rates = group.ToArray()
                })
                .ToArray();

            mockRatesGatewayService
                .Setup(service => service
                    .GetRatesAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<Currency>(), It.IsAny<Currency[]>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(testData));

            var testResult = revenueService.GetMaxRevenueTransactionWithRates(It.IsAny<DateTime>(), It.IsAny<DateTime>(), money, It.IsAny<CancellationToken>()).Result;
            Assert.That(testResult.MaxRevenueTransaction.Revenue, Is.EqualTo(revenue).Within(moneyTolerance));
            Assert.That(testResult.MaxRevenueTransaction.BuyDate, Is.EqualTo(buyDate));
            Assert.That(testResult.MaxRevenueTransaction.SellDate, Is.EqualTo(sellDate));
            Assert.That(testResult.MaxRevenueTransaction.Currency, Is.EqualTo(currency));
        }
    }
}