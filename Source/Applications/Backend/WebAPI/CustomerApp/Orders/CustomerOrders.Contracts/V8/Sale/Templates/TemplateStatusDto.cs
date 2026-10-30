using System.Text.Json.Serialization;

namespace CustomerOrders.Contracts.V8.Sale.Templates
{
	[JsonConverter(typeof(JsonStringEnumConverter))]
	public enum TemplateStatusDto
	{
		/// <summary>
		/// Ожидание оплаты
		/// </summary>
		AwaitingPayment,

		/// <summary>
		/// На обработке у менеджера
		/// </summary>
		ManualProcessing,

		/// <summary>
		/// Активен
		/// </summary>
		Active,

		/// <summary>
		/// Остановлен
		/// </summary>
		Paused,

		/// <summary>
		/// Проблема с оплатой
		/// </summary>
		PaymentProblem,

		/// <summary>
		/// Архивирован
		/// </summary>
		Archived
	}
}
