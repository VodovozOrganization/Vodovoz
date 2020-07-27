using QS.Project.Journal;
using Vodovoz.Domain.Orders;

namespace Vodovoz.JournalNodes
{
	public class ReturnTareReasonsJournalNode : JournalEntityNodeBase<ReturnTareReason>
	{
		public string Name { get; set; }

		public bool IsArchive { get; set; }
	}
}
