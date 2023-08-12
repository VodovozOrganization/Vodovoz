using System;
using System.Text.Json.Serialization;

namespace DriverAPI.Library.Deprecated2.DTOs
{
	/// <summary>
	/// Тип оплаты
	/// </summary>
	[Obsolete("Будет удален с прекращением поддержки API v2")]
	[JsonConverter(typeof(JsonStringEnumConverter))]
	public enum PaymentDtoType
	{
		/// <summary>
		/// Наличные
		/// </summary>
		Cash,
		/// <summary>
		/// Терминал
		/// </summary>
		Terminal,
		/// <summary>
		/// Мобильное приложение водителей
		/// </summary>
		DriverApplicationQR,
		/// <summary>
		/// Оплачено
		/// </summary>
		Paid,
		/// <summary>
		/// Безналичная оплата
		/// </summary>
		Cashless,
	}
}
