using QS.Project.Journal;
using Vodovoz.Domain.Orders;

namespace Vodovoz.ViewModels.Journals.JournalNodes
{
	public class DiscountReasonJournalNode : JournalEntityNodeBase<DiscountReason>
	{
		public override string Title => Name;
		public string Name { get; set; }
		
		public bool IsArchive { get; set; }
	}
}