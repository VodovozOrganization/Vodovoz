using System;
using System.Collections.Generic;
using QRPaymentDTOStatus = DriverAPI.Library.DTOs.QRPaymentDTOStatus;

namespace DriverAPI.Library.Deprecated2.DTOs
{
	/// <summary>
	/// Ответ по оплате по QR-коду
	/// </summary>
	[Obsolete("Будет удален с прекращением поддержки API v2")]
	public class PayByQRResponseDTO
	{
		/// <summary>
		/// Доступные для смены типы оплаты
		/// </summary>
		public IEnumerable<PaymentDtoType> AvailablePaymentTypes { get; set; }

		/// <summary>
		/// Можно ли получить QR-код
		/// </summary>
		public bool CanReceiveQR { get; set; }

		/// <summary>
		/// Статус оплаты QR-кода
		/// </summary>
		public QRPaymentDTOStatus? QRPaymentStatus { get; set; }

		/// <summary>
		/// Изображение QR-кода в формате Png закодированное в строку Base64
		/// </summary>
		public string QRCode { get; set; }

		/// <summary>
		/// Сообщение об ошибке
		/// </summary>
		public string ErrorMessage { get; set; }
	}
}
