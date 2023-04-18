namespace BadBrokerTestTask.Models
{
    public class DateRateHistory
    {
        public DateTime Date { get; set; }

        public RateHistoryRow[] Rates { get; set; }
    }
}
