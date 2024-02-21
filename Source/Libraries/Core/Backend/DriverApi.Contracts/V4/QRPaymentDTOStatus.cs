using System;
using System.Text.Json.Serialization;

namespace DriverAPI.Library.DTOs
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
