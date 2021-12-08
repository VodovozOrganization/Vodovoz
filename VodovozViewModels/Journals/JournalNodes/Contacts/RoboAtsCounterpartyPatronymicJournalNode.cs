using QS.Project.Journal;
using Vodovoz.Domain.Contacts;

namespace Vodovoz.ViewModels.Journals.JournalNodes.Contacts
{
	public class RoboAtsCounterpartyPatronymicJournalNode : JournalEntityNodeBase<RoboAtsCounterpartyPatronymic>
	{
		public override string Title => Patronymic;
		public string Patronymic { get; set; }
		public string Accent { get; set; }
	}
}
