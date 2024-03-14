using System.Text.Json.Serialization;

namespace DriverApi.Contracts.V5
{
	/// <summary>
	/// Статус оплаты по QR-коду
	/// </summary>
	[JsonConverter(typeof(JsonStringEnumConverter))]
	public enum QrPaymentDtoStatus
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
