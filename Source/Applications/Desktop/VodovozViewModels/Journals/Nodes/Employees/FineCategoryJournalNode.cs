using QS.DomainModel.Entity;
using QS.Project.Journal;
using Vodovoz.Domain.Employees;

namespace Vodovoz.ViewModels.Journals.Nodes.Employees
{
	public class FineCategoryJournalNode : JournalEntityNodeBase<FineCategory>
	{
		public override string Title => $"{EntityType.GetSubjectNames()} №{Id}";

		public string FineCategoryName { get; set; }
	}
}
