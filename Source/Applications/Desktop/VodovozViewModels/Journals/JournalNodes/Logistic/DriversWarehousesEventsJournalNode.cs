using QS.Project.Journal;
using Vodovoz.Core.Domain.Logistics.Drivers;
using Vodovoz.Domain.Logistic.Drivers;

namespace Vodovoz.ViewModels.Journals.JournalNodes.Logistic
{
	public class DriversWarehousesEventsJournalNode : JournalEntityNodeBase<DriverWarehouseEvent>
	{
		public string EventName { get; set; }
		public bool IsArchive { get; set; }
		public decimal? Latitude { get; set; }
		public decimal? Longitude { get; set; }
		public DriverWarehouseEventType Type { get; set; }
		public EventQrDocumentType? DocumentType { get; set; }
		public EventQrPositionOnDocument? QrPositionOnDocument { get; set; }
		public override string Title => EventName;
	}
}
