using Vodovoz.Core.Domain.FastPayments;

namespace FastPaymentsApi.Contracts.Responses
{
	/// <summary>
	/// Данные для ответа водительскому приложению
	/// </summary>
	public class QRResponseDTO : IFastPaymentStatusDto
	{
		/// <summary>
		/// Изображение QR-кода в формате Png закодированное в строку Base64
		/// </summary>
		public string QRCode { get; set; }
		/// <summary>
		/// Статус оплаты СБП
		/// </summary>
		public FastPaymentStatus? FastPaymentStatus { get; set; }
		/// <summary>
		/// Сообщение об ошибке/проблеме
		/// </summary>
		public string ErrorMessage { get; set; }
	}
}
