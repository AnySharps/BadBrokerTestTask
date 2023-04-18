using BadBrokerTestTask.Enums;

namespace BadBrokerTestTask.Models
{
    public class CurrencyRateHistory
    {
        public Currency Currency { get; set; }

        public RateHistoryRow[] Rates { get; set; }
    }
}
