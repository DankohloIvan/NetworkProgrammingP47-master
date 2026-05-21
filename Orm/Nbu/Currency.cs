using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace NetworkProgrammingP47.Orm.Nbu
{
    [XmlRoot(ElementName = "currency")]
    public class Currency
    {

        [XmlElement(ElementName = "r030")]
        public int CurrencyCode { get; set; }

        [XmlElement(ElementName = "txt")]
        public string FullName { get; set; } = null!;

        [XmlElement(ElementName = "rate")]
        public double Rate { get; set; }

        [XmlElement(ElementName = "cc")]
        public string ShortName { get; set; } = null!;

        [XmlElement(ElementName = "exchangedate")]
        public string ExchangeDateString { get; set; } = null!;

        [XmlElement(ElementName = "special")]
        public string? Special { get; set; }

        public DateOnly ExchangeDate => DateOnly.ParseExact(ExchangeDateString, "dd.MM.yyyy");

        public override string ToString()
        {
            return Rate > 0.01
                ? $"{ShortName} ({FullName}) {Rate:F2}"
                : $"{ShortName} ({FullName}) {Rate}";
        }
    }
}
