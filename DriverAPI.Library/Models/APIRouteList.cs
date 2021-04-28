using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace DriverAPI.Library.Models
{
    public class APIRouteList
    {
        public string CompletionStatus => completionStatus;
        private string completionStatus;
        [XmlIgnore]
        [JsonIgnore]
        public APIRouteListCompletionStatus CompletionStatusEnum {
            get => completionStatusEnum; 
            set
            {
                completionStatus = value.ToString();
                completionStatusEnum = value;
            }
        }
        private APIRouteListCompletionStatus completionStatusEnum;
        public APIIncompletedRouteList IncompletedRouteList { get; set; }
        public APICompletedRouteList CompletedRouteList { get; set; }
    }
}
