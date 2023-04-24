using System;
using System.Collections.Generic;

namespace DriverAPI.Library.Deprecated.DTOs
{
	[Obsolete("Будет удален с прекращением поддержки API v1")]
	public class OrderAdditionalInfoDto
	{
		public IEnumerable<PaymentDtoType> AvailablePaymentTypes { get; set; }
		public bool CanSendSms { get; set; }
		public bool CanReceiveQRCode { get; set; }
	}
}
