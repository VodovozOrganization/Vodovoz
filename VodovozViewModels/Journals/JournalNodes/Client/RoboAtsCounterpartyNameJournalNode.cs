using QS.Project.Journal;
using Vodovoz.Domain.Client;

namespace Vodovoz.ViewModels.Journals.JournalNodes.Client
{
	public class RoboAtsCounterpartyNameJournalNode : JournalEntityNodeBase<RoboAtsCounterpartyName>
	{
		public override string Title => Name;
		public string Name { get; set; }
		public string Accent { get; set; }
		public string RoboatsAudioFileName { get; set; }
		public virtual bool ReadyForRoboats => !string.IsNullOrWhiteSpace(RoboatsAudioFileName);
	}
}
