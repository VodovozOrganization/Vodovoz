using System;
using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace DriverAPI.Library.Models
{
    public class APIRouteListAddress
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public string Status => status;
        private string status;
        [XmlIgnore]
        [JsonIgnore]
        public APIRouteListAddressStatus StatusEnum
        {
            get => statusEnum;
            set
            {
                status = value.ToString();
                statusEnum = value;
            } 
        }
        private APIRouteListAddressStatus statusEnum;
        public DateTime DeliveryTime { get; set; }
        public int FullBottlesCount { get; set; }
        public APIAddress Address { get; set; }
    }
}
