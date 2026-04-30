using System.Text.Json.Serialization;

namespace CustomerOrders.Contracts
{
	/// <summary>
	/// Статус онлайн заказа
	/// </summary>
	[JsonConverter(typeof(JsonStringEnumConverter))]
	public enum ExternalCustomerOrderStatus
	{
		/// <summary>
		/// Оформляется
		/// </summary>
		OrderProcessing,
		/// <summary>
		/// Оформлен
		/// </summary>
		OrderPerformed,
		/// <summary>
		/// Ожидание оплаты
		/// </summary>
		WaitingForPayment,
		/// <summary>
		/// Собирается
		/// </summary>
		OrderCollecting,
		/// <summary>
		/// Доставляется
		/// </summary>
		OrderDelivering,
		/// <summary>
		/// Выполнен
		/// </summary>
		OrderCompleted,
		/// <summary>
		/// Отменен
		/// </summary>
		Canceled
	}
}
