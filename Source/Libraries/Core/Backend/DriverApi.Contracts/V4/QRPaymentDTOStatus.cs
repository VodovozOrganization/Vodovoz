using System.Text.Json.Serialization;

namespace DriverApi.Contracts.V4
{
	/// <summary>
	/// Статус оплаты по QR-коду
	/// </summary>
	[JsonConverter(typeof(JsonStringEnumConverter))]
	public enum QRPaymentDTOStatus
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
