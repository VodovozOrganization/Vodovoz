using System.Text.Json.Serialization;
using Vodovoz.Core.Domain.FastPayments;

namespace FastPaymentsApi.Contracts.Responses.OnlineOrderRegistration
{
	/// <summary>
	/// Данные ответа при регистрации онлайн заказа
	/// </summary>
	public class ResponseRegisterOnlineOrder : IFastPaymentStatusDto
	{
		/// <summary>
		/// Изображение QR-кода в формате Png закодированное в строку Base64
		/// </summary>
		public string QrCode { get; set; }
		/// <summary>
		/// Ссылка на оплату
		/// </summary>
		public string PayUrl { get; set; }
		/// <summary>
		/// Статус платежа
		/// </summary>
		[JsonIgnore]
		public FastPaymentStatus? FastPaymentStatus { get; set; }
		/// <summary>
		/// Сообщение об ошибке/проблеме
		/// </summary>
		public string ErrorMessage { get; set; }
	}
}
