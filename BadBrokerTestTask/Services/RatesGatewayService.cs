using BadBrokerTestTask.Enums;
using BadBrokerTestTask.Exceptions;
using BadBrokerTestTask.Interfaces;
using BadBrokerTestTask.Models;
using BadBrokerTestTask.Settings;
using Microsoft.AspNetCore.WebUtilities;
using System.Net.Http;
using static System.Net.Mime.MediaTypeNames;

namespace BadBrokerTestTask.Services
{
    public class RatesGateWayService : IRatesGatewayService
    {
        private readonly IHttpClientFactory clientFactory;
        private readonly ExchangeRatesApiSettings exchangeRatesApiSettings;
        
        public RatesGateWayService(IHttpClientFactory clientFactory, ExchangeRatesApiSettings exchangeRatesApiSettings)
        {
            this.clientFactory = clientFactory;
            this.exchangeRatesApiSettings = exchangeRatesApiSettings;
        }

        public async Task<DateRateHistory[]> GetRatesAsync(
            DateTime startDate, 
            DateTime endDate, 
            Currency currencyToSell, 
            Currency[] currenciesToBuy,
            CancellationToken cancellationToken)
        {
            var url = QueryHelpers.AddQueryString(
                exchangeRatesApiSettings.TimeSeriesUrl,
                new Dictionary<string, string?>
                {
                    { "start_date", startDate.ToString("yyyy-MM-dd") },
                    { "end_date", endDate.ToString("yyyy-MM-dd") },
                    { "base", currencyToSell.ToString() },
                    { "symbols",  string.Join(",", currenciesToBuy) }
                });
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("Accept", Application.Json);
            request.Headers.Add("apikey", exchangeRatesApiSettings.ApiKey);

            var client = clientFactory.CreateClient();
            try
            {
                var response = await client.SendAsync(request, cancellationToken);
                var exchangeRatesResponse = await response.Content.ReadFromJsonAsync<ExchangeRatesApiResponse>();
                return Map(exchangeRatesResponse.Rates);
            }
            catch (Exception exception)
            {
                throw new ApiException(exception.Message, exception);
            }
        }

        private DateRateHistory[] Map(Dictionary<DateTime, Dictionary<Currency, decimal>> ratesGroupedByDate)
            => ratesGroupedByDate
                .Select(groupByDate => new DateRateHistory
                {
                    Date = groupByDate.Key,
                    Rates = groupByDate.Value
                        .Select(rate => new RateHistoryRow
                        {
                            Currency = rate.Key,
                            Value = rate.Value,
                            Date = groupByDate.Key
                        })
                        .ToArray()
                })
                .ToArray();
    }
}
