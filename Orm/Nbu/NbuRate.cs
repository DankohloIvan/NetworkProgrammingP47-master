using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace NetworkProgrammingP47.Orm.Nbu
{
    public class NbuRate
    {
        [JsonPropertyName("r030")]
        public int CurrencyCode{ get; set; }

        [JsonPropertyName("txt")]
        public string FullName { get; set; } = null!;

        [JsonPropertyName("rate")]
        public double Rate { get; set; }

        [JsonPropertyName("cc")]
        public string ShortName { get; set; } = null!;

        [JsonPropertyName("exchangedate")]
        public string ExchangeDateString { get; set; } = null!;

        [JsonPropertyName("special")]
        public string? Special { get; set; }

        public override string ToString()
        {
            return Rate > 0.01 
                ? $"{ShortName} ({FullName}) {Rate:F2}"
                : $"{ShortName} ({FullName}) {Rate}";
        }
    }

}
