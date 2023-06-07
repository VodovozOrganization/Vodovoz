using QS.Project.Journal;
using System;
using Vodovoz.Domain.Orders;

namespace Vodovoz.ViewModels.Journals.JournalNodes.Orders
{
	public class UndeliveryTransferAbsenceReasonJournalNode : JournalEntityNodeBase<UndeliveryTransferAbsenceReason>
	{
		public override string Title => Name;
		public string Name { get; set; }
		public DateTime CreateDate { get; set; }
	}
}
