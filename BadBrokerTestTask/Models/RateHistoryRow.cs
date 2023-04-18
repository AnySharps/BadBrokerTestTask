using BadBrokerTestTask.Enums;

namespace BadBrokerTestTask.Models
{
    public class RateHistoryRow
    {
        public decimal Value { get; set; }

        public DateTime Date { get; set; }

        public Currency Currency { get; set; }
    }
}
