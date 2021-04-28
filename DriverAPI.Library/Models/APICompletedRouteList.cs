using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace DriverAPI.Library.Models
{
    public class APICompletedRouteList
    {
        public int RouteListId { get; set; }
        public string RouteListStatus => routeListStatus;
        private string routeListStatus;
        [XmlIgnore]
        [JsonIgnore]
        public APIRouteListStatus RouteListStatusEnum
        {
            get => routeListStatusEnum;
            set
            {
                routeListStatus = value.ToString();
                routeListStatusEnum = value;
            }
        }
        private APIRouteListStatus routeListStatusEnum;
        public decimal CashMoney { get; set; }
        public decimal TerminalMoney { get; set; }
        public int TerminalOrdersCount { get; set; }
        public int FullBottlesToReturn { get; set; }
        public int EmptyBottlesToReturn { get; set; }
    }
}
