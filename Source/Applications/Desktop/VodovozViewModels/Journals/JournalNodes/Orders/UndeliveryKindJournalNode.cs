using QS.Project.Journal;
using Vodovoz.Domain.Orders;

namespace Vodovoz.ViewModels.Journals.JournalNodes.Orders
{
	public class UndeliveryKindJournalNode : JournalEntityNodeBase<UndeliveryKind>
	{
		public override string Title => Name;
		public string Name { get; set; }
		public bool IsArchive { get; set; }
		public string UndeliveryObject { get; set; }
	}
}
