using System.Text.Json.Serialization;

namespace DriverApi.Contracts.V6
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
