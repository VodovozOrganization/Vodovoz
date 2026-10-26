using System.Text.Json.Serialization;

namespace CustomerOrders.Contracts.V8.Sale.Templates
{
	[JsonConverter(typeof(JsonStringEnumConverter))]
	public enum TemplatePaymentType
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
		/// Онлайн
		/// </summary>
		Online,
		/// <summary>
		/// СБП(сервис быстрых платежей)
		/// </summary>
		Sbp
	}
}
