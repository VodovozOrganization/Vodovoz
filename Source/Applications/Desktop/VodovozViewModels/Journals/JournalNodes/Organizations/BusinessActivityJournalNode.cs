using QS.Project.Journal;

namespace Vodovoz.ViewModels.Journals.JournalNodes.Organizations
{
	public class BusinessActivityJournalNode : JournalNodeBase
	{
		public override string Title => Name;
		public int Id { get; set; }
		public string Name { get; set; }
	}
}
