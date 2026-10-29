using QS.Project.Journal;
using Vodovoz.Domain.Orders;

namespace Vodovoz.ViewModels.Journals.JournalNodes.Goods
{
	public class PromotionalSetJournalNode : JournalEntityNodeBase<PromotionalSet>
	{
		public override string Title => Name;
		public string Name { get; set; }
		public string PromoSetDiscountReasonName { get; set; }
		public bool IsArchive { get; set; }
	}
}
