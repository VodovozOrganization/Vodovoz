using QS.Project.Journal;
using Vodovoz.Domain.Client;

namespace Vodovoz.ViewModels.Journals.JournalNodes.Client
{
	public class RoboAtsCounterpartyPatronymicJournalNode : JournalEntityNodeBase<RoboAtsCounterpartyPatronymic>
	{
		public override string Title => Patronymic;
		public string Patronymic { get; set; }
		public string Accent { get; set; }
		public string RoboatsAudioFileName { get; set; }
		public virtual bool ReadyForRoboats => !string.IsNullOrWhiteSpace(RoboatsAudioFileName);
	}
}
