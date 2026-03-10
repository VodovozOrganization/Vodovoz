namespace CustomerOrdersApi.Library.Services.PaymentRefund.Models.CloudPayments
{
	/// <summary>
	/// Результат операции возврата
	/// </summary>
	public class CloudPaymentsRefundResult
	{
		/// <summary>
		/// ID транзакции возврата
		/// </summary>
		public long TransactionId { get; set; }

		/// <summary>
		/// Статус транзакции возврата
		/// </summary>
		public CloudPaymentsTransactionStatus Status { get; set; }

		/// <summary>
		/// Код статуса
		/// </summary>
		public int StatusCode { get; set; }
	}
}
