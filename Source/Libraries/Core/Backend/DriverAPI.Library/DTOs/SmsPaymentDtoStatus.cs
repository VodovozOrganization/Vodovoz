using System.Text.Json.Serialization;

namespace DriverAPI.Library.DTOs
{
	/// <summary>
	/// Статус оплаты по смс
	/// </summary>
	[JsonConverter(typeof(JsonStringEnumConverter))]
	public enum SmsPaymentDtoStatus
	{
		/// <summary>
		/// Ожидает оплаты
		/// </summary>
		WaitingForPayment,
		/// <summary>
		/// Оплачено
		/// </summary>
		Paid,
		/// <summary>
		/// Отменено
		/// </summary>
		Cancelled
	}
}
