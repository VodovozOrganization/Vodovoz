using System;
using QS.Project.Journal;
using Vodovoz.Domain.Logistic.Drivers;

namespace Vodovoz.ViewModels.Journals.JournalNodes.Logistic
{
	public class CompletedDriversWarehousesEventsJournalNode : JournalEntityNodeBase<CompletedDriverWarehouseEvent>
	{
		public string EventName { get; set; }
		public DriverWarehouseEventType Type { get; set; }
		public string DriverName { get; set; }
		public string Car { get; set; }
		public DateTime CompletedDate { get; set; }
		public decimal? DistanceMetersBetweenPoints { get; set; }
		public override string Title => EventName;
	}
}
