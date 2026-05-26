using System;
using QS.Project.Journal;
using Vodovoz.Core.Domain.Payments;
using Vodovoz.Core.Domain.Refunds;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Payments;


namespace Vodovoz.ViewModels.Journals.JournalNodes.Refunds
{
	public class RefundJournalNode : JournalEntityNodeBase<RefundEntity>
	{
		public override string Title => $"{Order}";
		public DateTime Date {  get; set; }
		public string OrderOnlineId { get; set; }
		public string Order { get; set; }
	}
}
