using QS.Project.Journal;
using Vodovoz.Domain.Employees;

namespace Vodovoz.ViewModels.Journals.Nodes.Employees
{
	public class FineCategoryJournalNode : JournalEntityNodeBase<FineCategory>
	{
		public override string Title => "Что это?";
		public string FineCategoryName { get; set; }
	}
}
