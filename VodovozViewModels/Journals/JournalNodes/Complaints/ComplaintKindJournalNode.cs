using QS.Project.Journal;
using Vodovoz.Domain.Complaints;

namespace Vodovoz.ViewModels.Journals.JournalNodes.Complaints
{
	public class ComplaintKindJournalNode : JournalEntityNodeBase<ComplaintKind>
	{
		public override string Title => Name;
		public string Name { get; set; }
		public string ComplaintObject { get; set; }
	}
}
