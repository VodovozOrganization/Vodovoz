using System.Collections.Generic;

namespace DriverAPI.Library.DTOs
{
	public class OrderAdditionalInfoDto
	{
		public IEnumerable<PaymentDtoType> AvailablePaymentTypes { get; set; }
		public bool CanSendSms { get; set; }
		public bool CanReceiveQRCode { get; set; }
	}
}
