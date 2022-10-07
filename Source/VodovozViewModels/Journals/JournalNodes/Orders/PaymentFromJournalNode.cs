using QS.Project.Journal;
using Vodovoz.Domain.Orders;

namespace Vodovoz.ViewModels.Journals.JournalNodes.Orders
{
	public class PaymentFromJournalNode : JournalEntityNodeBase<PaymentFrom>
	{
		public override string Title => Name;
		public string Name { get; set; }
		public string OrganizationName { get; set; }
	}
}
