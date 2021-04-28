using DriverAPI.Library.Models;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace DriverAPI.Models
{
    public class OrderPaymentStatusResponseModel
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
        public string SmsPaymentStatus => smsPaymentStatus;
        private string smsPaymentStatus;
        [XmlIgnore]
        [JsonIgnore]
        public APISmsPaymentStatus? SmsPaymentStatusEnum
        {
            get => smsPaymentStatusEnum;
            set
            {
                smsPaymentStatus = value.ToString();
                smsPaymentStatusEnum = value;
            }
        }
        private APISmsPaymentStatus? smsPaymentStatusEnum;
    }
}
