using QS.Project.Journal;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.JournalNodes
{
	public class LateArrivalReasonsJournalNode : JournalEntityNodeBase<LateArrivalReason>
	{
		public string Name { get; set; }

		public bool IsArchive { get; set; }
	}
}
