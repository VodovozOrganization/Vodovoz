using QS.Project.Journal;
using Vodovoz.Domain.Logistic.Drivers;

namespace Vodovoz.ViewModels.Journals.JournalNodes.Logistic
{
	public class DriversWarehousesEventsJournalNode : JournalEntityNodeBase<DriverWarehouseEvent>
	{
		public string EventName { get; set; }
		public DriverWarehouseEventType Type { get; set; }
		public override string Title => EventName;
	}
}
