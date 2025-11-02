using QS.Project.Journal;

namespace Vodovoz.Presentation.ViewModels.Employees.Journals
{
	public class InnerPhoneJournalNode : JournalNodeBase
	{
		public override string Title => Number;

		public string Number { get; set; }
		public string Description { get; set; }
	}
}
