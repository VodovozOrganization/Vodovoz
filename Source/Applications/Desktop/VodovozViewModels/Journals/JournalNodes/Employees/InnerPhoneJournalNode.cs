using QS.Project.Journal;

namespace Vodovoz.ViewModels.Journals.JournalNodes
{
	public class InnerPhoneJournalNode : JournalNodeBase
	{
		public override string Title => Number;

		public string Number { get; set; }
		public string Description { get; set; }
	}
}
