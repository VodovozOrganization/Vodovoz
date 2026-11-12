using System.Text.Json.Serialization;

namespace CustomerOrders.Contracts.V8.Sale.Templates
{
	/// <summary>
	/// Статус шаблона автозаказа
	/// </summary>
	[JsonConverter(typeof(JsonStringEnumConverter))]
	public enum TemplateStatusDto
	{
		/// <summary>
		/// Проблема с оплатой
		/// </summary>
		PaymentProblem,

		/// <summary>
		/// На обработке у менеджера
		/// </summary>
		ManualProcessing,

		/// <summary>
		/// Ожидание оплаты
		/// </summary>
		AwaitingPayment,

		/// <summary>
		/// Активен
		/// </summary>
		Active,

		/// <summary>
		/// Остановлен
		/// </summary>
		Paused,

		/// <summary>
		/// Архивирован
		/// </summary>
		Archived
	}
}
