using BadBrokerTestTask.Enums;

namespace BadBrokerTestTask.Settings
{
    public class CurrencySettings
    {
        public Currency CurrencyToSell { get; set; }

        public Currency[] CurrenciesToBuy { get; set; }
    }
}
