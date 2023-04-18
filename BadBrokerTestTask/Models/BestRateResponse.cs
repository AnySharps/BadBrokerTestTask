using BadBrokerTestTask.Enums;
using System.Text.Json.Serialization;

namespace BadBrokerTestTask.Models
{
    public class BestRateResponse
    {
        public Dictionary<string, object>[] Rates { get; set; }

        public DateTime BuyDate { get; set; }

        public DateTime SellDate { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public Currency Tool { get; set; }

        public decimal Revenue { get; set; }
    }
}
