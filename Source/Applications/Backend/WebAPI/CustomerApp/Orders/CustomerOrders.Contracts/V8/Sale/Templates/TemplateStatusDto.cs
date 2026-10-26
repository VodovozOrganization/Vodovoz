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
		/// Проблема с оплатой
		/// </summary>
		PaymentProblem,
		/// <summary>
		/// Активен
		/// </summary>
		Active,
		/// <summary>
		/// Приостановлен
		/// </summary>
		Inactive,
		/// <summary>
		/// Архивирован
		/// </summary>
		Archived
	}
}
