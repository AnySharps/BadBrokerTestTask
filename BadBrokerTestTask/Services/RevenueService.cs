using BadBrokerTestTask.Interfaces;
using BadBrokerTestTask.Models;
using BadBrokerTestTask.Settings;

namespace BadBrokerTestTask.Services
{
    public class RevenueService : IRevenueService
    {
        private readonly IRatesGatewayService ratesGatewayService;
        private readonly CurrencySettings currencySettings;

        public RevenueService(IRatesGatewayService ratesGatewayService, CurrencySettings currencySettings)
        {
            this.ratesGatewayService = ratesGatewayService;
            this.currencySettings = currencySettings;
        }

        public async Task<MaxRevenueTransactionWithRates> GetMaxRevenueTransactionWithRates(
            DateTime startDate,
            DateTime endDate,
            decimal money, 
            CancellationToken cancellationToken)
        {
            var dateRateHistories = await ratesGatewayService.GetRatesAsync(
                startDate, 
                endDate, 
                currencySettings.CurrencyToSell, 
                currencySettings.CurrenciesToBuy,
                cancellationToken);

            var maxRevenueTransaction = MapToCurrencyRateHistory(dateRateHistories)
                .Select(history => GetMaxRevenueTransaction(history, money))
                .OrderByDescending(transaction => transaction.Revenue)
                .FirstOrDefault()
                ?? new ExchangeTransaction
                {
                    BuyDate = startDate,
                    SellDate = startDate,
                    Revenue = 0m,
                    Currency = currencySettings.CurrencyToSell
                };

            return new MaxRevenueTransactionWithRates
            {
                Rates = dateRateHistories,
                MaxRevenueTransaction = maxRevenueTransaction
            };
        }

        private ExchangeTransaction GetMaxRevenueTransaction(CurrencyRateHistory rateHistory, decimal money)
        {
            var (buyRate, sellRate, revenue) = GetOptimalBuySellDayAndRevenue(rateHistory.Rates, money);
            return new ExchangeTransaction
            {
                BuyDate = buyRate.Date,
                SellDate = sellRate.Date,
                Revenue = revenue,
                Currency = rateHistory.Currency
            };
        }
            

        private CurrencyRateHistory[] MapToCurrencyRateHistory(DateRateHistory[] dateRateHistories)
            => dateRateHistories
                .SelectMany(history => history.Rates)
                .GroupBy(rate => rate.Currency)
                .Select(groupByCurrency => new CurrencyRateHistory
                {
                    Currency = groupByCurrency.Key,
                    Rates = groupByCurrency.ToArray()
                })
                .ToArray();

        private (RateHistoryRow, RateHistoryRow, decimal) GetOptimalBuySellDayAndRevenue(RateHistoryRow[] rates, decimal money)
        {
            var (buyRateIndex, sellRateIndex) = (0, 0);

            var result = (rates[0], rates[0], maxRevenue: 0m);
            for (int rateIndex = 1; rateIndex < rates.Length; rateIndex++)
            {
                var (buyRate, sellRate) = (rates[buyRateIndex], rates[sellRateIndex]);
                var rate = rates[rateIndex];
                sellRateIndex = rateIndex;

                if (rate.Value >= sellRate.Value)
                {
                    if (rate.Value >= buyRate.Value)
                    {
                        buyRateIndex = rateIndex;
                    }

                    continue;
                }

                for (int newBuyRateIndex = buyRateIndex; newBuyRateIndex < sellRateIndex; newBuyRateIndex++)
                {
                    var revenue = CalculateRevenue(rates[newBuyRateIndex], rates[sellRateIndex], money);
                    if (result.maxRevenue < revenue)
                    {
                        buyRateIndex = newBuyRateIndex;
                        result = (rates[buyRateIndex], rates[sellRateIndex], revenue);
                    }
                }
            }

            return result;
        }

        private decimal CalculateRevenue(RateHistoryRow buyRate, RateHistoryRow sellRate, decimal money)
            => buyRate.Value * money / sellRate.Value - (sellRate.Date - buyRate.Date).Days - money;
    }
}
