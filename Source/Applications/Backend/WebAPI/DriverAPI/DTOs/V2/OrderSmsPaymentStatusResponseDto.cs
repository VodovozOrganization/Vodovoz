using DriverAPI.Library.DTOs;
using System.Collections.Generic;

namespace DriverAPI.DTOs.V2
{
	public class OrderSmsPaymentStatusResponseDto
	{
		public IEnumerable<PaymentDtoType> AvailablePaymentTypes { get; set; }
		public bool CanSendSms { get; set; }
		public SmsPaymentDtoStatus? SmsPaymentStatus { get; set; }
	}
}
