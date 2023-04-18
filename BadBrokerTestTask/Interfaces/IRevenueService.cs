using BadBrokerTestTask.Models;

namespace BadBrokerTestTask.Interfaces
{
    public interface IRevenueService
    {
        Task<MaxRevenueTransactionWithRates> GetMaxRevenueTransactionWithRates(DateTime startDate, DateTime endDate, decimal money, CancellationToken cancellationToken);
    }
}
