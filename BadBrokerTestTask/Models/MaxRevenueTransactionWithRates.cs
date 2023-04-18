namespace BadBrokerTestTask.Models
{
    public class MaxRevenueTransactionWithRates
    {
        public DateRateHistory[] Rates { get; set; }

        public ExchangeTransaction MaxRevenueTransaction { get; set; }
    }
}
