using DriverAPI.Library.DTOs;
using System;
using System.Collections.Generic;

namespace DriverAPI.Library.Deprecated.DTOs
{
	[Obsolete("Будет удален с прекращением поддержки API v1")]
	public class PayByQRResponseDTO
	{
		public IEnumerable<PaymentDtoType> AvailablePaymentTypes { get; set; }
		public bool CanReceiveQR { get; set; }
		public QRPaymentDTOStatus? QRPaymentStatus { get; set; }
		public string QRCode { get; set; }
		public string ErrorMessage { get; set; }
	}
}
