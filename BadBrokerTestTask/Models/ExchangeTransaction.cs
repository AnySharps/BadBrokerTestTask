using BadBrokerTestTask.Enums;

namespace BadBrokerTestTask.Models
{
    public class ExchangeTransaction
    {
        public DateTime BuyDate { get; set; }

        public DateTime SellDate { get; set; }

        public Currency Currency { get; set; }

        public decimal Revenue { get; set; }
    }
}
