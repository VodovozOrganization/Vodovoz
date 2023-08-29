using System;
using Vodovoz.Domain.FastPayments;

namespace DriverAPI.Library.DTOs
{
	/// <summary>
	/// Ответ FastPaymentApi
	/// </summary>
	public class QRResponseDTO
	{
		/// <summary>
		/// Изображение QR-кода в формате Png закодированное в строку Base64
		/// </summary>
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
