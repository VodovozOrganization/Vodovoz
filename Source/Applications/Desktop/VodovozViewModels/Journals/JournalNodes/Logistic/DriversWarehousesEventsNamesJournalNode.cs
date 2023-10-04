using QS.Project.Journal;
using Vodovoz.Domain.Logistic.Drivers;

namespace Vodovoz.ViewModels.Journals.JournalNodes.Logistic
{
	public class DriversWarehousesEventsNamesJournalNode : JournalEntityNodeBase<DriverWarehouseEventName>
	{
		public string EventName { get; set; }
		public override string Title => EventName;
	}
}
