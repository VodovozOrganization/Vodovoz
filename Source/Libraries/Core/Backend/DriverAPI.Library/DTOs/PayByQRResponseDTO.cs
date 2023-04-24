using System.Collections.Generic;

namespace DriverAPI.Library.DTOs
{
	public class PayByQRResponseDTO
	{
		public IEnumerable<PaymentDtoType> AvailablePaymentTypes { get; set; }
		public bool CanReceiveQR { get; set; }
		public QRPaymentDTOStatus? QRPaymentStatus { get; set; }
		public string QRCode { get; set; }
		public string ErrorMessage { get; set; }
	}
}
