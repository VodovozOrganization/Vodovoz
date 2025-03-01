using QS.Project.Journal;
using Vodovoz.Domain.Orders;

namespace Vodovoz.ViewModels.Journals.JournalNodes.Goods
{
	public class NonReturnReasonJournalNode : JournalEntityNodeBase<NonReturnReason>
	{
		public override string Title => Name;
		public string Name { get; set; }
		public bool NeedForfeit { get; set; }
	}
}
