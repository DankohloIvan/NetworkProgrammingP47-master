using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace NetworkProgrammingP47.Orm.Nbu
{
    [XmlRoot(ElementName = "exchange")]
    public class Exchange
    {
        [XmlElement(ElementName = "currency")]
        public List<Currency> Currencies { get; set; } = new();
    }

}
