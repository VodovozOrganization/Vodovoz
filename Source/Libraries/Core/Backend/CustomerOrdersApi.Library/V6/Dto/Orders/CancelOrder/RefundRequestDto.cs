using Vodovoz.Domain.Orders;

namespace CustomerOrdersApi.Library.V6.Dto.Orders.CancelOrder
{
	/// <summary>
	/// DTO запроса на возврат средств
	/// </summary>
	public class RefundRequestDto
	{
		/// <summary>
		/// Онлайн заказ
		/// </summary>
		public OnlineOrder OnlineOrder { get; init; }

		/// <summary>
		/// Внешний идентификатор заказа
		/// </summary>
		public string ExternalOrderId { get; init; }

		/// <summary>
		/// Сумма возврата
		/// </summary>
		public decimal Amount { get; init; }

		/// <summary>
		/// Идентификатор транзакции
		/// </summary>
		public string TransactionId { get; init; }

		/// <summary>
		/// Проверяет, является ли возврат полным
		/// </summary>
		public bool IsFullRefund() => Amount == OnlineOrder?.OnlineOrderSum;
	}
}
