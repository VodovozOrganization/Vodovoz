using System;

namespace DriverAPI.Models
{
    public class ChangeOrderPaymentTypeRequestModel
    {
        public int OrderId { get; set; }
        public string NewPaymentType { get; set; }
        public DateTime ActionTime { get; set; }
    }
}
