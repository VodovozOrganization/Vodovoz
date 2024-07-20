using QS.Project.Journal;

namespace Vodovoz.ViewModels.Journals.JournalNodes.Organizations
{
	public class BusinessAccountJournalNode : JournalNodeBase
	{
		public override string Title => Name;
		
		public int Id { get; set; }
		public string Name { get; set; }
		public string Number { get; set; }
		public string Bank { get; set; }
		public string BusinessActivity { get; set; }
		public string Funds { get; set; }
	}
}
