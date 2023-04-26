using DriverAPI.Library.DTOs;
using System.Collections.Generic;

namespace DriverAPI.DTOs
{
	public class OrderQRPaymentStatusResponseDto
	{
		public IEnumerable<PaymentDtoType> AvailablePaymentTypes { get; set; }
		public bool CanReceiveQR { get; set; }
		public QRPaymentDTOStatus? QRPaymentStatus { get; set; }
		public string QRCode { get; set; }
	}
}
