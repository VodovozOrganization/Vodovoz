using DriverAPI.Library.Deprecated.DTOs;
using DriverAPI.Library.DTOs;
using System.Collections.Generic;

namespace DriverAPI.DTOs.V1
{
	public class OrderSmsPaymentStatusResponseDto
	{
		public IEnumerable<PaymentDtoType> AvailablePaymentTypes { get; set; }
		public bool CanSendSms { get; set; }
		public SmsPaymentDtoStatus? SmsPaymentStatus { get; set; }
	}
}
