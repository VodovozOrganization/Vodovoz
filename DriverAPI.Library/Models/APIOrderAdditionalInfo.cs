using System.Collections.Generic;

namespace DriverAPI.Library.Models
{
    public class APIOrderAdditionalInfo
    {
        public IEnumerable<APIPaymentType> AvailablePaymentTypes { get; set; }
        public bool CanSendSms { get; set; }
    }
}