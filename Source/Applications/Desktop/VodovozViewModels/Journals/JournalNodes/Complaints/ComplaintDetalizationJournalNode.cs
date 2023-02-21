using QS.Project.Journal;
using Vodovoz.Domain.Complaints;

namespace Vodovoz.ViewModels.Journals.JournalNodes.Complaints
{
	public class ComplaintDetalizationJournalNode : JournalEntityNodeBase<ComplaintDetalization>
	{
		public override string Title => Name;
		public string Name { get; set; }
		public string ComplaintObject { get; set; }
		public string ComplaintKind { get; set; }
		public bool IsArchive { get; set; }
	}
}
