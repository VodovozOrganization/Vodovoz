using System.Text.Json.Serialization;

namespace DriverApi.Contracts.V5
{
	/// <summary>
	/// Тип оплаты
	/// </summary>
	[JsonConverter(typeof(JsonStringEnumConverter))]
	public enum PaymentDtoType
	{
		/// <summary>
		/// Наличные
		/// </summary>
		Cash,
		/// <summary>
		/// Терминал по карте
		/// </summary>
		TerminalCard,
		/// <summary>
		/// Терминал по QR-коду
		/// </summary>
		TerminalQR,
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
