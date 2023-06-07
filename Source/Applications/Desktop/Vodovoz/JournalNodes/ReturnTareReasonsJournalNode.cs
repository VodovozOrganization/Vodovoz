using QS.Project.Journal;
using Vodovoz.Domain.Orders;

namespace Vodovoz.JournalNodes
{
	public class ReturnTareReasonsJournalNode : JournalEntityNodeBase<ReturnTareReason>
	{
		public override string Title => Name;

		public string Name { get; set; }

		public bool IsArchive { get; set; }
	}
}
