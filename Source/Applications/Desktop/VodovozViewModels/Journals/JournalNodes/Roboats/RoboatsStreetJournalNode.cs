using QS.Project.Journal;
using Vodovoz.Domain.Roboats;

namespace Vodovoz.ViewModels.Journals.JournalNodes.Roboats
{
	public class RoboatsStreetJournalNode : JournalEntityNodeBase<RoboatsStreet>
	{
		public override string Title => Name;
		public string Street { get; set; }
		public string StreetType { get; set; }
		public string Name => $"{StreetType} {Street}";
		public string RoboatsAudioFileName { get; set; }
		public virtual bool ReadyForRoboats => !string.IsNullOrWhiteSpace(RoboatsAudioFileName);
	}
}
