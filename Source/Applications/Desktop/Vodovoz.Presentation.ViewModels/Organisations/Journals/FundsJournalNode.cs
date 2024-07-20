using QS.Project.Journal;

namespace Vodovoz.Presentation.ViewModels.Organizations.Journals
{
	public class FundsJournalNode : JournalNodeBase
	{
		public override string Title => Name;
		public int Id { get; set; }
		public string Name { get; set; }
	}
}
