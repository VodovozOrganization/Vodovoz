using System;
using QS.Project.Journal;
using Vodovoz.Domain.Orders;

namespace Vodovoz.JournalNodes
{
	public class PromotionalSetJournalNode : JournalEntityNodeBase<PromotionalSet>
	{
		public string Name { get; set; }
		public string PromoSetDiscountReasonName { get; set; }
		public bool IsArchive { get; set; }
	}
}
