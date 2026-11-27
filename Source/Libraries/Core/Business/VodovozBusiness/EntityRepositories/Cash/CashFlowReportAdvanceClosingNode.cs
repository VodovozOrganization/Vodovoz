namespace Vodovoz.EntityRepositories.Cash
{
	/// <summary>
	/// Данные закрытого аванса для распределения возврата
	/// </summary>
	public sealed class CashFlowReportAdvanceClosingNode
	{
		/// <summary>
		/// Идентификатор приходного ордера возврата
		/// </summary>
		public int ReturnId { get; set; }

		/// <summary>
		/// Идентификатор подразделения ЦФО расходного ордера
		/// </summary>
		public int? CfoSubdivisionId { get; set; }

		/// <summary>
		/// Идентификатор подразделения ЦФЗ расходного ордера
		/// </summary>
		public int? CfzSubdivisionId { get; set; }

		/// <summary>
		/// Сумма закрытия аванса
		/// </summary>
		public decimal Money { get; set; }
	}
}
