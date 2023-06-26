using System;
using System.Collections.Generic;

namespace DriverAPI.Library.Deprecated2.DTOs
{
	[Obsolete("Будет удален с прекращением поддержки API v2")]
	public class OrderAdditionalInfoDto
	{
		public IEnumerable<PaymentDtoType> AvailablePaymentTypes { get; set; }
		public bool CanSendSms { get; set; }
		public bool CanReceiveQRCode { get; set; }
	}
}
