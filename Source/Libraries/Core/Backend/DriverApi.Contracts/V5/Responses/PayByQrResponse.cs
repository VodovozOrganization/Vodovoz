using System.Collections.Generic;

namespace DriverApi.Contracts.V5.Responses
{
	/// <summary>
	/// Ответ по оплате по QR-коду
	/// </summary>
	public class PayByQrResponse
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
		public QrPaymentDtoStatus? QRPaymentStatus { get; set; }

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
