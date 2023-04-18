using BadBrokerTestTask.Enums;
using BadBrokerTestTask.Models;

namespace BadBrokerTestTask.Interfaces
{
    public interface IRatesGatewayService
    {
        Task<DateRateHistory[]> GetRatesAsync(DateTime startDate, DateTime endDate, Currency currencyToSell, Currency[] currenciesToBuy, CancellationToken cancellationToken);
    }
}
