using System.Collections.Generic;

namespace DriverAPI.Library.Models
{
	public class OrderAdditionalInfoDto
	{
		public IEnumerable<PaymentDtoType> AvailablePaymentTypes { get; set; }
		public bool CanSendSms { get; set; }
	}
}