using DriverAPI.Library.DTOs;
using System.Collections.Generic;
using PaymentDtoType = DriverAPI.Library.Deprecated2.DTOs.PaymentDtoType;

namespace DriverAPI.DTOs.V2
{
	public class OrderQRPaymentStatusResponseDto
	{
		public IEnumerable<PaymentDtoType> AvailablePaymentTypes { get; set; }
		public bool CanReceiveQR { get; set; }
		public QRPaymentDTOStatus? QRPaymentStatus { get; set; }
		public string QRCode { get; set; }
	}
}
