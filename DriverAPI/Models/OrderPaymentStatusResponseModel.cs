using DriverAPI.Library.Models;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DriverAPI.Models
{
    public class OrderPaymentStatusResponseModel
    {
        public IEnumerable<APIPaymentType> AvailablePaymentTypes { get; set; }
        public bool CanSendSms { get; set; }
        public APISmsPaymentStatus? SmsPaymentStatus { get; set; }
    }
}
