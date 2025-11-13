using QS.Project.Journal;
using Vodovoz.Core.Domain.Employees;

namespace Vodovoz.ViewModels.Journals.Nodes.Employees
{
	public class FineCategoryJournalNode : JournalEntityNodeBase<FineCategory>
	{
		public override string Title => FineCategoryName;

		public string FineCategoryName { get; set; }
	}
}
