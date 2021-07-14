using QS.Project.Journal;
using Vodovoz.Domain.Complaints;

namespace Vodovoz.ViewModels.Journals.JournalNodes.Complaints
{
	public class DriverComplaintReasonJournalNode : JournalEntityNodeBase<DriverComplaintReason>
	{
		public override string Title => Name;
		public string Name { get; set; }
		public bool IsPopular { get; set; }
	}
}
