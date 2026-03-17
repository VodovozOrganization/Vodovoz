using Vodovoz.Domain.Orders;

namespace CustomerOrdersApi.Library.Services.PaymentRefund.Models.CloudPayments
{
	/// <summary>
	/// Запрос на возврат средств в CloudPayments
	/// </summary>
	public class CloudPaymentsRefundRequest
	{
		/// <summary>
		/// Конструктор запроса возврата
		/// </summary>
		/// <param name="onlineOrder">Онлайн заказ</param>
		/// <param name="externalOrderId">Внешний идентификатор заказа</param>
		/// <param name="amount">Сумма возврата</param>
		/// <param name="transactionId">Идентификатор транзакции</param>
		public CloudPaymentsRefundRequest(
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
