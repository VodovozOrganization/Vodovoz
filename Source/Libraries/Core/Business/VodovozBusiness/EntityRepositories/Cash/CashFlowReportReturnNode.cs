namespace Vodovoz.EntityRepositories.Cash
{
	/// <summary>
	/// Приходный возврат для расчёта расходов
	/// </summary>
	public sealed class CashFlowReportReturnNode
	{
		/// <summary>
		/// Идентификатор приходного ордера
		/// </summary>
		public int Id { get; set; }

		/// <summary>
		/// Идентификатор статьи расхода в приходном ордере
		/// </summary>
		public int ExpenseCategoryId { get; set; }

		/// <summary>
		/// Сумма приходного ордера
		/// </summary>
		public decimal Money { get; set; }
	}
}
