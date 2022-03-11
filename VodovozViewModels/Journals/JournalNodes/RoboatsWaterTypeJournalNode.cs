using QS.Project.Journal;
using Vodovoz.Domain.Organizations;

namespace Vodovoz.ViewModels.Journals.JournalNodes
{
	public class RoboatsWaterTypeJournalNode : JournalEntityNodeBase<RoboatsWaterType>
	{
		public string Nomenclature { get; set; }
		public string RoboatsAudioFileName { get; set; }
		public virtual bool ReadyForRoboats => !string.IsNullOrWhiteSpace(RoboatsAudioFileName);
	}
}
