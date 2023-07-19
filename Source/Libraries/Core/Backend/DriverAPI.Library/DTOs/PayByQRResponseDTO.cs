using System.Collections.Generic;

namespace DriverAPI.Library.DTOs
{
	/// <summary>
	/// Ответ по оплате по QR-коду
	/// </summary>
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

		public string QRCode { get; set; }

		/// <summary>
		/// Сообщение об ошибке
		/// </summary>
		public string ErrorMessage { get; set; }
	}
}
