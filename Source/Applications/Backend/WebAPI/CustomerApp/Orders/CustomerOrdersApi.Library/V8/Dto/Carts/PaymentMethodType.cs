using System.Text.Json.Serialization;

namespace CustomerOrdersApi.Library.V8.Dto.Carts
{
	/// <summary>
	/// Тип оплаты
	/// </summary>
	[JsonConverter(typeof(JsonStringEnumConverter))]
	public enum PaymentMethodType
	{
		/// <summary>
		/// Наличка
		/// </summary>
		Cash,
		/// <summary>
		/// Терминал
		/// </summary>
		Terminal,
		/// <summary>
		/// Онлайн
		/// </summary>
		Online,
		/// <summary>
		/// СБП (сервис быстрых платежей)
		/// </summary>
		Sbp,
		/// <summary>
		/// Я Сплит
		/// </summary>
		YandexSplit
	}
}
