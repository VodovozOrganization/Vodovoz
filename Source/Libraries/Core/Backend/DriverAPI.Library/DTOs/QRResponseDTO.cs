using System;
using Vodovoz.Domain.FastPayments;

namespace DriverAPI.Library.DTOs
{
	public class QRResponseDTO
	{
		public string QRCode { get; set; }
		public string ErrorMessage { get; set; }
		public FastPaymentStatus FastPaymentStatus { get; set; }
	}
}
