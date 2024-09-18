using QS.Project.Journal;

namespace Vodovoz.Presentation.ViewModels.Organisations.Journals
{
	public class BusinessActivityJournalNode : JournalNodeBase
	{
		public override string Title => Name;
		public int Id { get; set; }
		public string Name { get; set; }
		public bool IsArchive { get; set; }
	}
}
