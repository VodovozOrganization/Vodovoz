using QS.Project.Journal;
using Vodovoz.Core.Domain.Payments;

namespace Vodovoz.ViewModels.Journals.JournalNodes.Payments
{
	public class ProfitCategoriesJournalNode : JournalEntityNodeBase<ProfitCategory>
	{
		public override string Title => Name;

		public string Name { get; set; }
		public bool IsArchive { get; set; }
	}
}
