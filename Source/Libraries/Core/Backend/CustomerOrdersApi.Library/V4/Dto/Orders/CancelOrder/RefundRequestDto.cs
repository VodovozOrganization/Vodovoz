using Vodovoz.Domain.Orders;

namespace CustomerOrdersApi.Library.V4.Dto.Orders.CancelOrder
{
	/// <summary>
	/// DTO запроса на возврат средств
	/// </summary>
	public class RefundRequestDto
	{
		/// <summary>
		/// Конструктор запроса возврата
		/// </summary>
		/// <param name="onlineOrder">Онлайн заказ</param>
		/// <param name="externalOrderId">Внешний идентификатор заказа</param>
		/// <param name="amount">Сумма возврата</param>
		/// <param name="transactionId">Идентификатор транзакции</param>
		public RefundRequestDto(
			OnlineOrder onlineOrder,
			string externalOrderId,
			decimal amount,
			string transactionId)
		{
			OnlineOrder = onlineOrder;
			ExternalOrderId = externalOrderId;
			Amount = amount;
			TransactionId = transactionId;
		}

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
	}
}
