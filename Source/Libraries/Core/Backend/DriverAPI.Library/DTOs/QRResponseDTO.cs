using System;
using Vodovoz.Domain.FastPayments;

namespace DriverAPI.Library.DTOs
{
	public class QRResponseDTO
	{
		public string QRCode { get; set; }

		/// <summary>
		/// Сообщение об ошибке
		/// </summary>
		public string ErrorMessage { get; set; }

		/// <summary>
		/// Статус оплаты СБП
		/// </summary>
		public FastPaymentStatus FastPaymentStatus { get; set; }
	}
}
