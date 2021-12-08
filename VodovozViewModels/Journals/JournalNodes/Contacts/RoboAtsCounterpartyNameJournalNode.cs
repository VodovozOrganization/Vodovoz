using QS.Project.Journal;
using Vodovoz.Domain.Contacts;

namespace Vodovoz.ViewModels.Journals.JournalNodes.Contacts
{
	public class RoboAtsCounterpartyNameJournalNode : JournalEntityNodeBase<RoboAtsCounterpartyPatronymic>
	{
		public override string Title => Name;
		public string Name { get; set; }
		public string Accent { get; set; }
	}
}
