using BadBrokerTestTask.Enums;

namespace BadBrokerTestTask.Models
{
    public class ExchangeRatesApiResponse
    {
        public bool Success { get; set; }

        public bool TimeSeries { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public Dictionary<DateTime, Dictionary<Currency, decimal>> Rates { get; set; }
    }
}
