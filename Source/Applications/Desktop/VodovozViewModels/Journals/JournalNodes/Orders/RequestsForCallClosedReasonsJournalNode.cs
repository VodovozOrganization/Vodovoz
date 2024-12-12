using QS.Project.Journal;

namespace Vodovoz.ViewModels.Journals.JournalNodes.Orders
{
	public class RequestsForCallClosedReasonsJournalNode : JournalEntityNodeBase
	{
		public override string Title => Name;
		public string Name { get; set; }
		public bool IsArchive { get; set; }
	}
}
