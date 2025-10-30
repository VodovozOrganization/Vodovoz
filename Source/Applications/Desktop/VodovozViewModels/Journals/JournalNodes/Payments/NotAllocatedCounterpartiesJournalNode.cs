using QS.Project.Journal;

namespace Vodovoz.ViewModels.Journals.JournalNodes.Payments
{
	/// <summary>
	/// Строка журнала клиентов, которые не участвуют в распределении
	/// </summary>
	public class NotAllocatedCounterpartiesJournalNode : JournalEntityNodeBase
	{
		/// <summary>
		/// Имя, отображаемое в Entry
		/// </summary>
		public override string Title => Name;
		/// <summary>
		/// Название контрагента
		/// </summary>
		public string Name { get; set; }
		/// <summary>
		/// ИНН
		/// </summary>
		public string Inn { get; set; }
		/// <summary>
		/// Категория дохода
		/// </summary>
		public string ProfitCategory { get; set; }
	}
}
