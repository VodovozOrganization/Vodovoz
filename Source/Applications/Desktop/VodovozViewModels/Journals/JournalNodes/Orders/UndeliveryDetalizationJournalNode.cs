using QS.Project.Journal;
using Vodovoz.Domain.Orders;

namespace Vodovoz.ViewModels.Journals.JournalNodes.Orders
{
	public class UndeliveryDetalizationJournalNode : JournalEntityNodeBase<UndeliveryDetalization>
	{
		public override string Title => Name;
		public string Name { get; set; }
		public string UndeliveryObject { get; set; }
		public string UndeliveryKind { get; set; }
		public bool IsArchive { get; set; }
	}
}
