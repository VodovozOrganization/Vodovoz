using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace DriverAPI.Library.Models
{
    public class APIOrderAdditionalInfo
    {
        public IEnumerable<string> AvailablePaymentTypes => availablePaymentTypes;
        private IEnumerable<string> availablePaymentTypes;
        [XmlIgnore]
        [JsonIgnore]
        public IEnumerable<APIPaymentType> AvailablePaymentEnumTypes
        {
            get => availablePaymentEnumTypes;
            set
            {
                availablePaymentTypes = value.Select(x => x.ToString());
                availablePaymentEnumTypes = value;
            }
        }
        private IEnumerable<APIPaymentType> availablePaymentEnumTypes;
        public bool CanSendSms { get; set; }
    }
}