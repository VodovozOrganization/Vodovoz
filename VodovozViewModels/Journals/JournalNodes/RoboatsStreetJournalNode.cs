using QS.Project.Journal;
using Vodovoz.Domain.Organizations;

namespace Vodovoz.ViewModels.Journals.JournalNodes
{
	public class RoboatsStreetJournalNode : JournalEntityNodeBase<RoboatsStreet>
	{
		public string Street { get; set; }
		public string StreetType { get; set; }
		public string Name => $"{StreetType} {Street}";
		public string RoboatsAudioFileName { get; set; }
		public virtual bool ReadyForRoboats => !string.IsNullOrWhiteSpace(RoboatsAudioFileName);
	}
}
