using QS.Project.Journal;
using Vodovoz.Domain.Client;

namespace Vodovoz.ViewModels.Journals.JournalNodes.Client
{
	public class UnsubscribingReasonJournalNode : JournalEntityNodeBase<UnsubscribingReason>
	{
		public override string Title => Name;
		public string Name { get; set; }
		public bool IsArchive { get; set; }
	}
}
