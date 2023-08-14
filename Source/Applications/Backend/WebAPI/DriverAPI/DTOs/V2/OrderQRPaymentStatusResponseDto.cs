using DriverAPI.Library.DTOs;
using System.Collections.Generic;
using PaymentDtoType = DriverAPI.Library.Deprecated2.DTOs.PaymentDtoType;

namespace DriverAPI.DTOs.V2
{
	/// <summary>
	/// Ответ сервера о статусе платежа по QR-коду
	/// </summary>
	public class OrderQRPaymentStatusResponseDto
	{
		/// <summary>
		/// Доступные типы оплаты для смена
		/// </summary>
		public IEnumerable<PaymentDtoType> AvailablePaymentTypes { get; set; }

		/// <summary>
		/// Возможно ли получить QR-код
		/// </summary>
		public bool CanReceiveQR { get; set; }

		/// <summary>
		/// Статус оплаты по QR-коду
		/// </summary>
		public QRPaymentDTOStatus? QRPaymentStatus { get; set; }

		/// <summary>
		/// Изображение QR-кода в формате Png закодированное в строку Base64
		/// </summary>
		public string QRCode { get; set; }
	}
}
