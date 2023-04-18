using BadBrokerTestTask.Exceptions;
using BadBrokerTestTask.Interfaces;
using BadBrokerTestTask.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.Net;

namespace BadBrokerTestTask.Controllers
{
    [ApiController]
    [Route("rates")]
    public class RevenueController : ControllerBase
    {
        private readonly IRevenueService revenueService;
        private IStringLocalizer<RevenueController> stringLocalizer;

        public RevenueController(IRevenueService revenueService, IStringLocalizer<RevenueController> stringLocalizer)
        {
            this.revenueService = revenueService;
            this.stringLocalizer = stringLocalizer;
        }

        [HttpGet]
        [Route("best")]
        public async Task<object> GetBestRates(
            [FromQuery]DateTime startDate, 
            [FromQuery]DateTime endDate, 
            [FromQuery]decimal moneyUsd,
            CancellationToken cancellationToken)
        {
            if (startDate > endDate || endDate > DateTime.UtcNow)
            {
                return new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    ReasonPhrase = stringLocalizer.GetString("DateOutOfBounds")
                };
            }

            if ((endDate - startDate).Days > Constants.RatesMaxPeriod)
            {
                return new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    ReasonPhrase = string.Format(stringLocalizer.GetString("RatesPeriodMoreThanMax"), Constants.RatesMaxPeriod)
                };
            }

            if (moneyUsd < 0m)
            {
                return new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    ReasonPhrase = stringLocalizer.GetString("MoneyShouldBeGreaterOrEqualZero")
                };
            }

            try
            {
                var maxRevenueTransactionWithRate = await revenueService.GetMaxRevenueTransactionWithRates(startDate, endDate, moneyUsd, cancellationToken);
                var transaction = maxRevenueTransactionWithRate.MaxRevenueTransaction;
                return new BestRateResponse
                {
                    Rates = maxRevenueTransactionWithRate.Rates
                        .Select(ratesGroupedByDate => ratesGroupedByDate.Rates
                            .Select(rate => new KeyValuePair<string, object>(
                                key: rate.Currency.ToString().ToLower(),
                                value: rate.Value))
                            .Append(new KeyValuePair<string, object>(
                                key: "date",
                                value: ratesGroupedByDate.Date))
                            .ToDictionary(pair => pair.Key, pair => pair.Value))
                        .ToArray(),
                    BuyDate = transaction.BuyDate,
                    SellDate = transaction.SellDate,
                    Revenue = transaction.Revenue,
                    Tool = transaction.Currency
                };
            }
            catch(ApiException)
            {
                return new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    ReasonPhrase = stringLocalizer.GetString("ProblemsWithApi")
                };
            }
            catch (Exception)
            {
                return new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    ReasonPhrase = stringLocalizer.GetString("UnknownException")
                };
            }
        }
    }
}
