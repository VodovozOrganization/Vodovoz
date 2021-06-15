using DriverAPI.Library.Models;
using System.Collections.Generic;

namespace DriverAPI.Models
{
	public class OrderPaymentStatusResponseModel
	{
		public IEnumerable<PaymentDtoType> AvailablePaymentTypes { get; set; }
		public bool CanSendSms { get; set; }
		public SmsPaymentDtoStatus? SmsPaymentStatus { get; set; }
	}
}
