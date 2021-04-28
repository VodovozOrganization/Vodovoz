using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace DriverAPI.Library.Models
{
    public class APIIncompletedRouteList
    {
        public int RouteListId { get; set; }
        public string RouteListStatus => RouteListStatusEnum.ToString();
        [XmlIgnore]
        [JsonIgnore]
        public APIRouteListStatus RouteListStatusEnum { get; set; }
        public IEnumerable<APIRouteListAddress> RouteListAddresses { get; set; }
    }
}
