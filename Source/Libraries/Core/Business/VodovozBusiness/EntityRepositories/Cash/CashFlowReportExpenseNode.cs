namespace Vodovoz.EntityRepositories.Cash
{
	/// <summary>
	/// Сумма расхода по статье и финансовым центрам
	/// </summary>
	public sealed class CashFlowReportExpenseNode
	{
		/// <summary>
		/// Идентификатор статьи расхода
		/// </summary>
		public int ExpenseCategoryId { get; set; }

		/// <summary>
		/// Идентификатор подразделения ЦФО
		/// </summary>
		public int? CfoSubdivisionId { get; set; }

		/// <summary>
		/// Идентификатор подразделения ЦФЗ
		/// </summary>
		public int? CfzSubdivisionId { get; set; }

		/// <summary>
		/// Сумма расхода с учётом знака операции
		/// </summary>
		public decimal Money { get; set; }
	}
}
