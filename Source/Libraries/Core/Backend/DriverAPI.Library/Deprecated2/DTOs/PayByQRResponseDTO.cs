using System;
using System.Collections.Generic;
using DriverAPI.Library.DTOs;

namespace DriverAPI.Library.Deprecated2.DTOs
{
	[Obsolete("Будет удален с прекращением поддержки API v2")]
	public class PayByQRResponseDTO
	{
		public IEnumerable<PaymentDtoType> AvailablePaymentTypes { get; set; }
		public bool CanReceiveQR { get; set; }
		public QRPaymentDTOStatus? QRPaymentStatus { get; set; }
		public string QRCode { get; set; }
		public string ErrorMessage { get; set; }
	}
}
