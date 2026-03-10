using System;

namespace CustomerOrdersApi.Library.Services.PaymentRefund.Models.CloudPayments
{
	/// <summary>
	/// Модель транзакции из CloudPayments
	/// </summary>
	public class CloudPaymentsTransaction
	{
		/// <summary>
		/// Номер транзакции
		/// </summary>
		public long TransactionId { get; set; }

		/// <summary>
		/// Сумма операции
		/// </summary>
		public decimal Amount { get; set; }

		/// <summary>
		/// Тип операции
		/// </summary>
		public CloudPaymentsOperationType Type { get; set; }

		/// <summary>
		/// Статус транзакции
		/// </summary>
		public CloudPaymentsTransactionStatus Status { get; set; }

		/// <summary>
		/// Числовой код статуса
		/// </summary>
		public int StatusCode { get; set; }

		/// <summary>
		/// Флаг, указывающий, был ли по этой транзакции выполнен возврат
		/// </summary>
		public bool Refunded { get; set; }

		/// <summary>
		/// ID родительской транзакции
		/// </summary>
		public long? OriginalTransactionId { get; set; }

		/// <summary>
		/// Дата и время создания транзакции
		/// </summary>
		public DateTime CreatedDateIso { get; set; }
	}
}
