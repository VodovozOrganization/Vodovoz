using QS.Project.Journal;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.ViewModels.Journals.JournalNodes.Logistic
{
	public class CarEventTypeJournalNode : JournalEntityNodeBase<CarEventType>
	{
		public override string Title => Name;
		public string Name { get; set; }
		public string ShortName { get; set; }
		public bool NeedComment { get; set; }
		public bool IsArchive { get; set; }
		public bool IsDoNotShowInOperation { get; set; }
	}
}
