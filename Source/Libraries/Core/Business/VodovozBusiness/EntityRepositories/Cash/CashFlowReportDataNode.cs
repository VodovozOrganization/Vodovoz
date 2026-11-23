using System.Collections.Generic;

namespace Vodovoz.EntityRepositories.Cash
{
	/// <summary>
	/// Данные кассовых движений за период отчёта
	/// </summary>
	public sealed class CashFlowReportDataNode
	{
		/// <summary>
		/// Доходы по идентификаторам статей
		/// </summary>
		public IReadOnlyDictionary<int, decimal> IncomeTotals { get; set; }

		/// <summary>
		/// Суммы расходных ордеров
		/// </summary>
		public IList<CashFlowReportExpenseNode> ExpenseMovements { get; set; }

		/// <summary>
		/// Приходные ордера возврата
		/// </summary>
		public IList<CashFlowReportReturnNode> Returns { get; set; }

		/// <summary>
		/// Закрытия авансов приходным ордером
		/// </summary>
		public IList<CashFlowReportAdvanceClosingNode> DirectClosings { get; set; }

		/// <summary>
		/// Закрытия авансов отчётами с возвратом сдачи
		/// </summary>
		public IList<CashFlowReportAdvanceClosingNode> AdvanceReportClosings { get; set; }

		/// <summary>
		/// Названия подразделений по идентификаторам центров
		/// </summary>
		public IReadOnlyDictionary<int, string> SubdivisionNames { get; set; }
	}
}
