using QS.Project.Journal;

namespace Vodovoz.ViewModels.Journals.JournalNodes.Orders
{
	public class OrdersRatingReasonsJournalNode : JournalEntityNodeBase
	{
		public override string Title => Name;
		public string Name { get; set; }
		public bool IsArchive { get; set; }
		public string AvailableForRatings { get; set; }
	}
}
