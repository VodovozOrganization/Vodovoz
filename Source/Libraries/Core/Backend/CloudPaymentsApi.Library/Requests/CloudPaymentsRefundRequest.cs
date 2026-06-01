namespace CloudPaymentsApi.Library.Requests
{
	/// <summary>
	/// Запрос на возврат средств в CloudPayments
	/// </summary>
	public class CloudPaymentsRefundRequest
	{
		/// <summary>
		/// Конструктор запроса возврата
		/// </summary>
		/// <param name="externalOrderId">Внешний идентификатор заказа</param>
		/// <param name="amount">Сумма возврата</param>
		/// <param name="transactionId">Идентификатор транзакции</param>
		public CloudPaymentsRefundRequest(
			string externalOrderId,
			decimal amount,
			string transactionId)
		{
			ExternalOrderId = externalOrderId;
			Amount = amount;
			TransactionId = transactionId;
		}

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
